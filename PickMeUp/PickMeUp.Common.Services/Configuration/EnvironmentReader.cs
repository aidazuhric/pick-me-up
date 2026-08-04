namespace PickMeUp.Common.Services.Configuration
{
    public static class EnvironmentReader
    {
        public static string Required(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Configuration value '{key}' is missing. Add it to the .env file next to the solution, using .env.example as the template.");
            }

            return value;
        }

        public static string Optional(string key, string fallback = "") =>
            Environment.GetEnvironmentVariable(key) is { Length: > 0 } value ? value : fallback;

        public static int Int(string key, int fallback)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (!int.TryParse(value, out var parsed))
            {
                throw new InvalidOperationException($"Configuration value '{key}' must be a whole number, but was '{value}'.");
            }

            return parsed;
        }

        public static bool Bool(string key, bool fallback)
        {
            var value = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (!bool.TryParse(value, out var parsed))
            {
                throw new InvalidOperationException($"Configuration value '{key}' must be true or false, but was '{value}'.");
            }

            return parsed;
        }

        public static IReadOnlyList<string> List(string key) =>
            Environment.GetEnvironmentVariable(key)
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();
    }
}
