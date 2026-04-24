using MoneyTracker.Api.Endpoints.Auth.Contracts;
using MoneyTracker.BusinessLogic.Features.Auth.Login;
using MoneyTracker.BusinessLogic.Features.Auth.Register;

namespace MoneyTracker.Api.Endpoints.Auth.ExtensionMethods
{
    internal static class MappingHelper
    {
        public static LoginCommand ToLoginCommand(this LoginRequest entity)
        {
            return new LoginCommand
            {
                Username = entity.Username,
                Password = entity.Password
            };
        }

        public static AuthTokenResponse ToLoginAuthTokenResponse(this LoginAuthToken entity)
        {
            return new AuthTokenResponse
            {
                Token = entity.Token
            };
        }

        public static AuthTokenResponse ToRegisterAuthTokenResponse(this RegisterAuthToken entity)
        {
            return new AuthTokenResponse
            {
                Token = entity.Token
            };
        }

        public static RegisterCommand ToRegisterCommand(this RegisterRequest entity)
        {
            return new RegisterCommand
            {
                Username = entity.Username,
                Password = entity.Password
            };
        }
    }
}
