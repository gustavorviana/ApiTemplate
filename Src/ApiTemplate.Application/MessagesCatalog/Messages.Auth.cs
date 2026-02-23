namespace ApiTemplate.Application.MessagesCatalog;

public static partial class Messages
{
    public static class Auth
    {
        public const string UserWithEmailAlreadyExists =
            "User with this email already exists.";

        public const string PasswordMinimumRequirementsNotMet =
            "Password does not meet the minimum security requirements.";

        public const string InvalidEmailOrPassword =
            "Invalid email or password.";

        public const string InvalidAccessToken =
            "Invalid access token.";

        public const string InvalidOrExpiredRefreshToken =
            "Invalid or expired refresh token.";

        public const string UserNotFound =
            "User not found.";
    }
}