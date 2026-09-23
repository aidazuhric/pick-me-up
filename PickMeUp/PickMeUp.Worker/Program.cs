using PickMeUp.Common.Services.Configuration;
using PickMeUp.Worker;

EnvironmentConfigurationExtensions.LoadEnvFile();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddEnvironmentConfiguration();

var smtpHost = EnvironmentReader.Optional("SMTP_HOST");
var smtpUsername = EnvironmentReader.Optional("SMTP_USERNAME");
var smtpPassword = EnvironmentReader.Optional("SMTP_PASSWORD");

var smtpIsUsable = !string.IsNullOrWhiteSpace(smtpHost)
    && !string.IsNullOrWhiteSpace(smtpUsername)
    && !string.IsNullOrWhiteSpace(smtpPassword);

if (smtpIsUsable)
{
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
}
else
{
    builder.Services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
}

builder.Services.AddHostedService<NotificationConsumer>();

var host = builder.Build();
await host.RunAsync();
