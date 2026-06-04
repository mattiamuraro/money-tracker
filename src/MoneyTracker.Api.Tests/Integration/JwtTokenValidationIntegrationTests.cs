using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.BusinessLogic.Features.Auth;
using Xunit;

namespace MoneyTracker.Api.Tests.Integration;

/// <summary>
/// Integration tests verifying that the JWT bearer middleware rejects tokens that violate
/// the algorithm, signature, issuer, audience, or expiry constraints configured in the API host.
/// </summary>
public sealed class JwtTokenValidationIntegrationTests : IDisposable
{
    private readonly ApiWebFactory _factory;

    public JwtTokenValidationIntegrationTests()
    {
        _factory = new ApiWebFactory();
    }

    public void Dispose() => _factory.Dispose();

    // ── algorithm enforcement ────────────────────────────────────────────────

    /// <summary>
    /// A token signed with RS256 must be rejected even if the structure is otherwise valid,
    /// because only HS256 is accepted by the algorithm validator.
    /// </summary>
    [Fact]
    public async Task Token_SignedWithRs256_IsRejected()
    {
        using var client = _factory.CreateClient();
        var rsa = System.Security.Cryptography.RSA.Create(2048);
        var key = new RsaSecurityKey(rsa);
        var token = BuildToken(key, SecurityAlgorithms.RsaSha256);

        Authorize(client, token);
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A token using the "none" algorithm (unsigned) must be rejected.
    /// </summary>
    [Fact]
    public async Task Token_WithNoneAlgorithm_IsRejected()
    {
        using var client = _factory.CreateClient();

        // Build an unsigned JWT manually (header.payload with no signature).
        var header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var now = DateTimeOffset.UtcNow;
        var payload = Base64UrlEncode(
            $"{{\"sub\":\"{Guid.NewGuid()}\",\"iss\":\"MoneyTracker.Api.Tests\"," +
            $"\"aud\":\"MoneyTracker.Tests\"," +
            $"\"exp\":{now.AddHours(1).ToUnixTimeSeconds()}," +
            $"\"iat\":{now.ToUnixTimeSeconds()}}}");

        var unsignedToken = $"{header}.{payload}.";

        Authorize(client, unsignedToken);
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A token signed with HS512 (a supported HMAC variant but not HS256) must be rejected.
    /// HS512 requires a 512-bit key, so we use a dedicated 64-byte key to build the token.
    /// </summary>
    [Fact]
    public async Task Token_SignedWithHs512_IsRejected()
    {
        using var client = _factory.CreateClient();
        // HS512 requires >=512 bits; use a dedicated 64-byte key so the library can sign it.
        var hs512Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "hs512-test-key-exactly-sixty-four-bytes-long-padding-padding-!!!"));
        var token = BuildToken(hs512Key, SecurityAlgorithms.HmacSha512);

        Authorize(client, token);
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── signature enforcement ────────────────────────────────────────────────

    /// <summary>
    /// A token with a tampered signature must be rejected.
    /// </summary>
    [Fact]
    public async Task Token_WithTamperedSignature_IsRejected()
    {
        using var client = _factory.CreateClient();
        var validToken = BuildValidHs256Token(Guid.NewGuid());

        // Corrupt the middle of the signature (the last char may have unused padding bits
        // that don't affect decoding, so we target an early character instead).
        var parts = validToken.Split('.');
        var sig = parts[2];
        var mid = sig.Length / 2;
        var flipped = sig[mid] == 'A' ? 'B' : 'A';
        parts[2] = sig[..mid] + flipped + sig[(mid + 1)..];
        var tamperedToken = string.Join('.', parts);

        Authorize(client, tamperedToken);
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A token signed with a different HMAC key (key-substitution attack) must be rejected.
    /// </summary>
    [Fact]
    public async Task Token_SignedWithWrongKey_IsRejected()
    {
        using var client = _factory.CreateClient();
        var wrongKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("totally-different-key-32bytes!!!"));
        var token = BuildToken(wrongKey, SecurityAlgorithms.HmacSha256);

        Authorize(client, token);
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── claim/metadata enforcement ───────────────────────────────────────────

    /// <summary>
    /// An expired token must be rejected even when the signature is valid.
    /// </summary>
    [Fact]
    public async Task Token_Expired_IsRejected()
    {
        using var client = _factory.CreateClient();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiredToken = new JwtSecurityToken(
            issuer: "MoneyTracker.Api.Tests",
            audience: "MoneyTracker.Tests",
            claims: PermissionClaims(Guid.NewGuid()),
            notBefore: DateTime.UtcNow.AddHours(-2),
            expires: DateTime.UtcNow.AddHours(-1),  // expired 1 hour ago
            signingCredentials: creds);

        Authorize(client, new JwtSecurityTokenHandler().WriteToken(expiredToken));
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A token with a wrong issuer must be rejected.
    /// </summary>
    [Fact]
    public async Task Token_WithWrongIssuer_IsRejected()
    {
        using var client = _factory.CreateClient();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "https://attacker.example.com",
            audience: "MoneyTracker.Tests",
            claims: PermissionClaims(Guid.NewGuid()),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        Authorize(client, new JwtSecurityTokenHandler().WriteToken(token));
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A token with a wrong audience must be rejected.
    /// </summary>
    [Fact]
    public async Task Token_WithWrongAudience_IsRejected()
    {
        using var client = _factory.CreateClient();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MoneyTracker.Api.Tests",
            audience: "SomeOtherService",
            claims: PermissionClaims(Guid.NewGuid()),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        Authorize(client, new JwtSecurityTokenHandler().WriteToken(token));
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// A structurally valid HS256 token that is missing the required permission claims must
    /// be rejected by the authorization policy.
    /// </summary>
    [Fact]
    public async Task Token_MissingPermissionClaims_IsForbiddenOrUnauthorized()
    {
        using var client = _factory.CreateClient();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MoneyTracker.Api.Tests",
            audience: "MoneyTracker.Tests",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        Authorize(client, new JwtSecurityTokenHandler().WriteToken(token));
        var response = await client.GetAsync("/api/v1/payments?month=2024-01");

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Expected 401 or 403 for missing permission claims, got {response.StatusCode}");
    }

    /// <summary>
    /// A token with only a read permission claim must be rejected on a write endpoint.
    /// </summary>
    [Fact]
    public async Task Token_WithReadOnlyPermission_IsRejectedOnWriteEndpoint()
    {
        using var client = _factory.CreateClient();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "MoneyTracker.Api.Tests",
            audience: "MoneyTracker.Tests",
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Read)
                // no Write permission
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        Authorize(client, new JwtSecurityTokenHandler().WriteToken(token));
        var response = await client.PostAsync("/api/v1/payments",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected 403 or 401 for read-only token on write endpoint, got {response.StatusCode}");
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private static void Authorize(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    private static string BuildValidHs256Token(Guid userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiWebFactory.TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "MoneyTracker.Api.Tests",
            audience: "MoneyTracker.Tests",
            claims: PermissionClaims(userId),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string BuildToken(SecurityKey key, string algorithm)
    {
        var creds = new SigningCredentials(key, algorithm);
        var token = new JwtSecurityToken(
            issuer: "MoneyTracker.Api.Tests",
            audience: "MoneyTracker.Tests",
            claims: PermissionClaims(Guid.NewGuid()),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Claim[] PermissionClaims(Guid userId) =>
    [
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Read),
        new Claim(AuthAuthorization.PermissionClaimType, AuthAuthorization.Permissions.Write)
    ];

    private static string Base64UrlEncode(string input) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
