using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MoneyTracker.BusinessLogic.Common.Exceptions;
using MoneyTracker.BusinessLogic.Common.Options;
using MoneyTracker.BusinessLogic.Features.Auth.ExtensionMethods;
using MoneyTracker.Data;
using MoneyTracker.Data.EntityFramework;

namespace MoneyTracker.BusinessLogic.Features.Auth.Login
{
    public class LoginCommandHandler
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IValidator<LoginCommand> _validator;
        private readonly MoneyTrackerDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginCommandHandler(IValidator<LoginCommand> validator, IOptions<JwtOptions> jwtOptions, IPasswordHasher<User> passwordHasher, MoneyTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
            _validator = validator;
            _jwtOptions = jwtOptions.Value;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginAuthToken> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == command.Username, cancellationToken);

            if (user is null)
                throw new UnauthorizedAccessException($"Username or password is incorrect");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException($"Username or password is incorrect");

            var token = user.BuildToken(_jwtOptions);

            return new LoginAuthToken { Token = token };

        }
    }
}
