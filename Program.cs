using choinka.Gpio;
using choinka.Triggers.SolarTime;
using choinka.Triggers.SolarTime.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Runtime.InteropServices;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .WriteTo.Console()
    .CreateBootstrapLogger();
Log.Information("Starting up");

try
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        Log.Error("This application is intended to run on Linux systems " +
            "with systemd and GPIO support like RaspberryPi");
        return;
    }

    var hostBuilder = Host.CreateDefaultBuilder();
    hostBuilder.UseSystemd();
    hostBuilder.ConfigureServices((ctx, services) =>
    {
        services.AddSingleton(sp => new Places());

        services.AddSunTimes(ctx);
        services.AddEventServices();

        services.AddSingleton<GpioControllerWithPinRestore>();
        services.AddHostedService<GpioWorker>();
    });
    hostBuilder.Build().Run();
}
catch (Exception ex)
{
    Log.Fatal("Fatar error: {Message}", ex.Message);
}
finally
{
    Log.Information("Shutdown complete");
    Log.CloseAndFlush();
}