using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoneyTracker.Api.Endpoints.Auth.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly MoneyTrackerDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public JwtTokenService(IConfiguration configuration, MoneyTrackerDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<string?> LoginAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user is null)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        return BuildToken(user);
    }

    public async Task<string?> RegisterAsync(string username, string password, CancellationToken cancellationToken)
    {
        var usernameTaken = await _dbContext.Users
            .AnyAsync(u => u.Username == username, cancellationToken);

        if (usernameTaken)
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(null!, password),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildToken(user);
    }

    private string BuildToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "480"));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
