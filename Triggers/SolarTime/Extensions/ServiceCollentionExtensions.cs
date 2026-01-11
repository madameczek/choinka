using choinka.Triggers.Timed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace choinka.Triggers.SolarTime.Extensions;
internal static class ServiceCollentionExtensions
{
    public static IServiceCollection AddSunTimes(this IServiceCollection services, HostBuilderContext ctx)
    {
        services.AddSingleton(sp => new Places());
        services.AddScoped<ISolarCalculator, SolarCalculator>();
        services.AddSingleton<SolarNotifierService>();
        services.AddHostedService(sp => sp.GetRequiredService<SolarNotifierService>());

        return services;
    }

    public static IServiceCollection AddEventServices(this IServiceCollection services)
    {
        services.AddSingleton<AlarmClockService>();
        services.AddHostedService(sp => sp.GetRequiredService<AlarmClockService>());

        return services;
    }
}
