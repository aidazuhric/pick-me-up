using PickMeUp.Common.Services.Configuration;
using PickMeUp.Worker;

EnvironmentConfigurationExtensions.LoadEnvFile();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddEnvironmentConfiguration();

if (string.IsNullOrWhiteSpace(EnvironmentReader.Optional("SMTP_HOST")))
{
    builder.Services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
}
else
{
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
}

builder.Services.AddHostedService<NotificationConsumer>();

var host = builder.Build();
await host.RunAsync();
