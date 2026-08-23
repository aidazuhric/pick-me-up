using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PickMeUp.Common.Services.Configuration
{
    public static class EnvironmentConfigurationExtensions
    {
        public static StartupOptions AddEnvironmentConfiguration(this IServiceCollection services)
        {
            var database = new DatabaseOptions
            {
                ConnectionString = EnvironmentReader.Required("DATABASE_CONNECTION_STRING")
            };

            var jwt = new JwtOptions
            {
                SecretKey = EnvironmentReader.Required("JWT_SECRET_KEY"),
                Issuer = EnvironmentReader.Required("JWT_ISSUER"),
                Audience = EnvironmentReader.Required("JWT_AUDIENCE"),
                DurationInMinutes = EnvironmentReader.Int("JWT_DURATION_IN_MINUTES", 30)
            };

            var api = new ApiOptions
            {
                BasePath = EnvironmentReader.Optional("API_BASE_PATH"),
                CorsAllowedOrigins = EnvironmentReader.List("CORS_ALLOWED_ORIGINS")
            };

            services.AddSingleton(Options.Create(database));
            services.AddSingleton(Options.Create(jwt));
            services.AddSingleton(Options.Create(api));

            services.AddSingleton(Options.Create(new RabbitMqOptions
            {
                Host = EnvironmentReader.Optional("RABBITMQ_HOST"),
                Port = EnvironmentReader.Int("RABBITMQ_PORT", 5672),
                Username = EnvironmentReader.Optional("RABBITMQ_USERNAME"),
                Password = EnvironmentReader.Optional("RABBITMQ_PASSWORD"),
                Sender = EnvironmentReader.Optional("RABBITMQ_SENDER")
            }));

            services.AddSingleton(Options.Create(new SmtpOptions
            {
                Host = EnvironmentReader.Optional("SMTP_HOST"),
                Port = EnvironmentReader.Int("SMTP_PORT", 587),
                Username = EnvironmentReader.Optional("SMTP_USERNAME"),
                Password = EnvironmentReader.Optional("SMTP_PASSWORD"),
                UseSsl = EnvironmentReader.Bool("SMTP_USE_SSL", true)
            }));

            services.AddSingleton(Options.Create(new TwoFactorOptions
            {
                CodeLength = EnvironmentReader.Int("TWO_FACTOR_CODE_LENGTH", 6),
                ExpiryMinutes = EnvironmentReader.Int("TWO_FACTOR_EXPIRY_MINUTES", 5),
                MaxAttempts = EnvironmentReader.Int("TWO_FACTOR_MAX_ATTEMPTS", 5),
                ResendCooldownSeconds = EnvironmentReader.Int("TWO_FACTOR_RESEND_COOLDOWN_SECONDS", 30)
            }));

            services.AddSingleton(Options.Create(new PasswordResetOptions
            {
                TokenLength = EnvironmentReader.Int("PASSWORD_RESET_TOKEN_LENGTH", 8),
                ExpiryMinutes = EnvironmentReader.Int("PASSWORD_RESET_EXPIRY_MINUTES", 20),
                MaxAttempts = EnvironmentReader.Int("PASSWORD_RESET_MAX_ATTEMPTS", 5)
            }));

            services.AddSingleton(Options.Create(new StripeOptions
            {
                SecretKey = EnvironmentReader.Optional("STRIPE_SECRET_KEY"),
                PublishableKey = EnvironmentReader.Optional("STRIPE_PUBLISHABLE_KEY"),
                WebhookSecret = EnvironmentReader.Optional("STRIPE_WEBHOOK_SECRET")
            }));

            services.AddSingleton(Options.Create(new MapsOptions
            {
                ApiKey = EnvironmentReader.Optional("MAPS_API_KEY")
            }));

            services.AddSingleton(Options.Create(new FileStorageOptions
            {
                RootPath = EnvironmentReader.Optional("FILE_STORAGE_ROOT", "storage")
            }));

            return new StartupOptions(database, jwt, api);
        }

        public static void LoadEnvFile()
        {
            var envFile = FindEnvFile(AppContext.BaseDirectory);

            if (envFile != null)
            {
                DotNetEnv.Env.Load(envFile);
            }
        }

        private static string? FindEnvFile(string startDirectory)
        {
            var directory = new DirectoryInfo(startDirectory);

            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, ".env");

                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
