using MoneyTracker.Api.Auth;
using MoneyTracker.Api.Contracts;

namespace MoneyTracker.Api.Services
{
    /// <summary>
    /// Service for managing authentication operations
    /// </summary>
    public class AuthService
    {
        private readonly JwtTokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            JwtTokenService tokenService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        public async Task<IResult> LoginAsync(string username, string password, CancellationToken cancellationToken)
        {
            var result = await _tokenService.LoginAsync(username, password, cancellationToken);
            return result switch
            {
                null => Results.Unauthorized(),
                _ => Results.Ok(new { token = result })
            };
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        public async Task<IResult> RegisterAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (!bool.TryParse(_configuration["Auth:AllowRegistration"], out var allowReg) || !allowReg)
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return Results.BadRequest(new ErrorResponse { Message = "Username and password are required.", StatusCode = 400 });

            var token = await _tokenService.RegisterAsync(username, password, cancellationToken);
            return token switch
            {
                null => Results.Conflict(new ErrorResponse { Message = "Username is already taken.", StatusCode = 409 }),
                _ => Results.Ok(new { token })
            };
        }

        /// <summary>
        /// Gets authentication configuration
        /// </summary>
        public IResult GetAuthConfig()
        {
            var allowRegistration = bool.TryParse(_configuration["Auth:AllowRegistration"], out var val) && val;
            return Results.Ok(new { allowRegistration });
        }
    }
}
