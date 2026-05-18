namespace MoneyTracker.Api.Endpoints.Auth.Contracts;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
