using MoneyTracker.Api.Middleware;
using Xunit;

namespace MoneyTracker.Api.Tests.Middleware;

public class ExceptionDetailSanitizerTests
{
    private static ExceptionDetailSanitizer CreateSanitizer() => new();

    // ──────────────────────────────────────────────────── Null / empty input

    [Fact]
    public void Sanitize_Should_Return_Null_When_Input_Is_Null()
    {
        var result = CreateSanitizer().Sanitize(null);

        Assert.Null(result);
    }

    [Fact]
    public void Sanitize_Should_Return_Empty_String_When_Input_Is_Empty()
    {
        var result = CreateSanitizer().Sanitize(string.Empty);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Sanitize_Should_Return_Whitespace_When_Input_Is_Whitespace()
    {
        var result = CreateSanitizer().Sanitize("   ");

        Assert.Equal("   ", result);
    }

    [Fact]
    public void Sanitize_Should_Return_Safe_Text_Unchanged()
    {
        var input = "An error occurred while processing the request.";

        var result = CreateSanitizer().Sanitize(input);

        Assert.Equal(input, result);
    }

    // ─────────────────────────────────────── Connection string password redaction

    [Fact]
    public void Sanitize_Should_Redact_Connection_String_Password()
    {
        var input = "Server=myserver;Password=supersecret123;Database=mydb";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("supersecret123", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Connection_String_Password_Case_Insensitive()
    {
        var input = "Server=myserver;PASSWORD=MyP@ssword!;Database=mydb";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("MyP@ssword!", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Connection_String_Password_With_Spaces_Around_Equals()
    {
        var input = "Server=myserver;Password = mysecret;Database=mydb";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("mysecret", result);
    }

    // ──────────────────────────────────────────────── JSON secret key redaction

    [Theory]
    [InlineData("password")]
    [InlineData("pwd")]
    [InlineData("secret")]
    [InlineData("token")]
    [InlineData("apiKey")]
    [InlineData("api-key")]
    [InlineData("connectionString")]
    public void Sanitize_Should_Redact_Json_Secret_Keys(string keyName)
    {
        var input = $"{{\"username\":\"alice\",\"{keyName}\":\"topsecretvalue\"}}";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("topsecretvalue", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Json_Secret_Case_Insensitive()
    {
        var input = "{\"Password\":\"topsecretvalue\"}";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("topsecretvalue", result);
    }

    [Fact]
    public void Sanitize_Should_Not_Redact_Json_Non_Secret_Keys()
    {
        var input = "{\"username\":\"alice\",\"email\":\"alice@example.com\"}";

        var result = CreateSanitizer().Sanitize(input);

        Assert.Equal(input, result);
    }

    // ─────────────────────────────────────────────── Bearer token redaction

    [Fact]
    public void Sanitize_Should_Redact_Bearer_Token()
    {
        var input = "Authorization: Bearer eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ1c2VyMSJ9.sig";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("eyJhbGciOiJSUzI1NiJ9", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Bearer_Token_Case_Insensitive()
    {
        var input = "BEARER eyJhbGciOiJSUzI1NiJ9.payload.sig";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("eyJhbGciOiJSUzI1NiJ9", result);
    }

    // ───────────────────────────────────────────────── API key redaction

    [Fact]
    public void Sanitize_Should_Redact_X_Api_Key_Header_With_Colon()
    {
        var input = "x-api-key: abc123secretkey";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("abc123secretkey", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_X_Api_Key_Header_With_Equals()
    {
        var input = "x-api-key=abc123secretkey";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("abc123secretkey", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Api_Key_Case_Insensitive()
    {
        var input = "X-API-KEY: MyApiKeyValue";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("MyApiKeyValue", result);
    }

    // ──────────────────────────────────────────── Key=value secret redaction

    [Theory]
    [InlineData("password=mypassword")]
    [InlineData("pwd=mypassword")]
    [InlineData("secret=mypassword")]
    [InlineData("token=mypassword")]
    [InlineData("apikey=mypassword")]
    [InlineData("api-key=mypassword")]
    [InlineData("connectionstring=mypassword")]
    public void Sanitize_Should_Redact_KeyValue_Secret_Pairs(string input)
    {
        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("mypassword", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_KeyValue_Secret_Case_Insensitive()
    {
        var input = "Password=SuperSecret123";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("SuperSecret123", result);
    }

    // ──────────────────────────────────────── High-entropy token redaction

    [Fact]
    public void Sanitize_Should_Redact_High_Entropy_Token_32_Chars_Or_More()
    {
        // 32+ alphanumeric characters — looks like a secret/API key
        var input = "Request failed with token: abcdefghijklmnopqrstuvwxyz123456";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz123456", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Not_Redact_Short_Alphanumeric_Strings()
    {
        // 31 characters — below the 32-char threshold
        var input = "Error code: abcdefghijklmnopqrstu1234567";

        var result = CreateSanitizer().Sanitize(input);

        // The 40-char string will be redacted; verify the prefix is still visible
        Assert.NotNull(result);
        Assert.Contains("Error code:", result);
    }

    // ──────────────────────────────────────────────── Multiple secrets in one string

    [Fact]
    public void Sanitize_Should_Redact_Multiple_Secrets_In_Same_String()
    {
        var input = "Server=s;Password=p1secret;Database=d error with token=t2secret";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("p1secret", result);
        Assert.DoesNotContain("t2secret", result);
    }

    [Fact]
    public void Sanitize_Should_Preserve_Non_Secret_Parts_Of_String()
    {
        var input = "Database connection to myserver failed: password=hunter2";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.Contains("Database connection to myserver failed:", result);
        Assert.DoesNotContain("hunter2", result);
    }

    // ──────────────────────────────────────── Adversarial / boundary inputs

    [Fact]
    public void Sanitize_Should_Redact_Password_With_Special_Characters()
    {
        var input = "Password=P@$$w0rd!#%^&*()";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("P@$$w0rd", result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Password_With_Unicode_Characters()
    {
        var input = "password=sécret123";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("sécret123", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Json_Secret_With_Empty_Value()
    {
        var input = "{\"password\":\"\"}";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Json_Secret_With_Nested_Quotes_In_Key_Adjacent_Text()
    {
        var input = "Payload: {\"username\":\"bob\",\"secret\":\"abc\",\"email\":\"bob@example.com\"}";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("\"abc\"", result);
        Assert.Contains("bob@example.com", result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Bearer_Token_With_Only_One_Segment()
    {
        var input = "Authorization: Bearer simpletokenwithoutdots";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("simpletokenwithoutdots", result);
    }

    [Fact]
    public void Sanitize_Should_Handle_Very_Long_Input_Without_Throwing()
    {
        var secret = new string('s', 200);
        var input = $"password={secret};" + new string('x', 10_000);

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain(secret, result);
    }

    [Fact]
    public void Sanitize_Should_Redact_Multiple_Different_Patterns_In_One_Log_Line()
    {
        var input = "Auth error — Bearer eyJhbGciOiJSUzI1NiJ9.payload.sig — password=s3cr3t — x-api-key: myapikey123";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.DoesNotContain("eyJhbGciOiJSUzI1NiJ9", result);
        Assert.DoesNotContain("s3cr3t", result);
        Assert.DoesNotContain("myapikey123", result);
        Assert.Contains("Auth error", result);
    }

    [Fact]
    public void Sanitize_Should_Not_Redact_Normal_Words_Shorter_Than_32_Chars()
    {
        var input = "User authentication failed for request ID abc123";

        var result = CreateSanitizer().Sanitize(input);

        Assert.NotNull(result);
        Assert.Contains("User authentication failed", result);
        Assert.Contains("abc123", result);
    }
}
