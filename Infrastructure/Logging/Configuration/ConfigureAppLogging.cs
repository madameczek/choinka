using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Graylog;

namespace choinka.Infrastructure.Logging.Configuration;

public static class ConfigureAppLogging
{
    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureSerilog =>
        (ctx, serilogConfig) =>
        {
            var loggerConfig = ctx.Configuration
                .GetSection("Logger")
                .Get<AppLoggerConfiguration>();
            
            if (loggerConfig is null)
                throw new ArgumentException("No logger config found in \"Logger\" " +
                    "section in app configuration file", nameof(loggerConfig));

            serilogConfig.ReadFrom.Configuration(ctx.Configuration);
            serilogConfig.Enrich.FromLogContext();
            serilogConfig.Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName);
            serilogConfig.Enrich.WithProperty("AppName", loggerConfig.GraylogAppName);
            serilogConfig.WriteTo.Console(
                restrictedToMinimumLevel: Enum.Parse<Serilog.Events.LogEventLevel>(loggerConfig.GraylogMinimumLevel ?? "Information"),
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
            serilogConfig.WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = loggerConfig.GraylogAddress,
                Port = loggerConfig.GraylogPort,
                TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp,
                MinimumLogEventLevel = Enum.Parse<Serilog.Events.LogEventLevel>(loggerConfig.GraylogMinimumLevel),
                HostnameOverride = loggerConfig.HostName
            });
            Log.Information("Logger configured to use Graylog at {GraylogAddress}:{GraylogPort}",
                loggerConfig.GraylogAddress, loggerConfig.GraylogPort);
        };
}
