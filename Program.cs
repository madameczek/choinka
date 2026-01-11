using choinka.Gpio;
using choinka.Triggers.SolarTime;
using choinka.Triggers.SolarTime.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

try
{
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