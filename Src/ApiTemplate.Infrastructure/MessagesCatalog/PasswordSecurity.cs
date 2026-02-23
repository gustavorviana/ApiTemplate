namespace ApiTemplate.Infrastructure.MessagesCatalog;

public static partial class Messages
{
    public static class PasswordSecurityMessages
    {
        public const string PasswordCannotBeEmpty = "Password cannot be empty.";

        public const string PasswordAtLeast8Chars =
            "Password must be at least 8 characters long.";

        public const string PasswordAtLeast10Chars =
            "Password must be at least 10 characters long.";

        public const string PasswordAtLeast12Chars =
            "Password must be at least 12 characters long.";

        public const string PasswordAtLeast14Chars =
            "Password must be at least 14 characters long.";

        public const string PasswordMustContainDigit =
            "Password must contain at least one digit.";

        public const string PasswordMustContainLowerAndUpper =
            "Password must contain both lowercase and uppercase letters.";

        public const string PasswordMustContainSymbol =
            "Password must contain at least one non-alphanumeric character.";

        public const string PasswordMustContainThreeCategories =
            "Password must contain characters from at least three categories: lowercase, uppercase, digits, symbols.";

        public const string PasswordMustContainAllCategories =
            "Password must contain lowercase, uppercase, digits and symbols.";

        public const string PasswordBelowMinimum =
            "Password strength is below the required minimum.";
    }
}