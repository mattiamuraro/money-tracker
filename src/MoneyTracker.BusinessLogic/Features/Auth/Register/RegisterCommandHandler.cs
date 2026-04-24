using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.Register
{
    public class RegisterCommandHandler
    {
        private readonly AuthOptions _authOptions;
        private readonly JwtOptions _jwtOptions;
        private readonly IValidator<RegisterCommand> _validator;
        private readonly MoneyTrackerDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterCommandHandler(IValidator<RegisterCommand> validator, IOptions<AuthOptions> authOptions, IOptions<JwtOptions> jwtOptions, IPasswordHasher<User> passwordHasher, MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
            _validator = validator;
            _authOptions = authOptions.Value;
            _jwtOptions = jwtOptions.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterAuthToken> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            if (!_authOptions.AllowRegistration)
                throw new UnauthorizedAccessException("Registration is currently disabled.");

            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var usernameTaken = await _dbContext.Users.AnyAsync(u => u.Username == command.Username, cancellationToken);

            if (usernameTaken)
                throw new ValidationException("Username is already taken.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = command.Username,
                PasswordHash = _passwordHasher.HashPassword(null!, command.Password),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var token = user.BuildToken(_jwtOptions);

            return new RegisterAuthToken { Token = token };

        }
    }
}
