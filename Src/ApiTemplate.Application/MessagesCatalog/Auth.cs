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

        // Validation - Login
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "Email must be a valid email address.";
        public const string PasswordRequired = "Password is required.";

        // Validation - Register
        public const string NameRequired = "Name is required.";
        public const string NameMinLength = "Name must be at least 2 characters.";
        public const string NameMaxLength = "Name must not exceed 200 characters.";
        public const string PasswordMinLength = "Password must be at least 8 characters.";
    }
}