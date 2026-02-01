using choinka.Gpio;
using choinka.Triggers.SolarTime.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Runtime.InteropServices;
using static choinka.Infrastructure.Logging.Configuration.ConfigureAppLogging;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        Log.Error("This application is intended to run on Linux systems " +
            "with systemd and GPIO support like RaspberryPi");
        return;
    }

    Log.Information("Starting up");

    var hostBuilder = Host.CreateDefaultBuilder();
    hostBuilder.UseSystemd();
    hostBuilder.ConfigureAppConfiguration((ctx, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile($"appsettings.secrets.{ctx.HostingEnvironment.EnvironmentName}.json", false, false);
    });
    hostBuilder.UseSerilog((ctx, serilogConfig) => ConfigureSerilog(ctx, serilogConfig));
    hostBuilder.ConfigureServices((ctx, services) =>
    {
        services.AddSunTimes(ctx);
        services.AddEventServices();

        services.AddSingleton<GpioControllerWithPinRestore>();
        services.AddHostedService<GpioWorker>();
    });
    hostBuilder.Build().Run();
}
catch (Exception ex)
{
    Log.Fatal("Fatal error: {Message}", ex.Message);
}
finally
{
    Log.Information("Shutdown complete");
    Log.CloseAndFlush();
}