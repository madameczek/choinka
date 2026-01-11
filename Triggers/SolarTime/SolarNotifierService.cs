using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace choinka.Triggers.SolarTime;
internal class SolarNotifierService(
    ILogger<SolarNotifierService> logger,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    public event EventHandler? SunsetOccurred;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunEventLoopAsync(
            "Zachód słońca",
            (calc, date) => calc.GetWarsawSunset(date),
            reason => InvokeSunsetEvent(reason),
            stoppingToken);
    }

    private async Task RunEventLoopAsync(
        string eventName,
        Func<ISolarCalculator, DateTimeOffset?, DateTime> getEventTime,
        Action<string> invokeEvent,
        CancellationToken token)
    {
        try
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var calculator = scope.ServiceProvider.GetRequiredService<ISolarCalculator>();
                var todayTime = getEventTime(calculator, null);
                var now = DateTimeOffset.Now;

                if (now > todayTime)
                {
                    logger.LogWarning("Aplikacja uruchomiona po {EventName}. Wywołuję event natychmiast (catch-up).", eventName);
                    invokeEvent($"Zaległy {eventName} (start aplikacji po czasie)");
                }
                else
                {
                    logger.LogInformation("Czekam na dzisiejszy {EventName}: {EventTime}", eventName, todayTime);
                    await WaitUntil(todayTime, token);

                    if (!token.IsCancellationRequested)
                        invokeEvent($"Dzisiejszy {eventName}");
                }
            }

            // Daily loop
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var tomorrow = DateTimeOffset.Now.AddDays(1);
                    using var scope = scopeFactory.CreateScope();
                    var calculator = scope.ServiceProvider.GetRequiredService<ISolarCalculator>();
                    var nextTime = getEventTime(calculator, tomorrow);

                    logger.LogInformation("Następny {EventName} zaplanowany na: {EventTime}", eventName, nextTime);

                    await WaitUntil(nextTime, token);

                    if (!token.IsCancellationRequested)
                        invokeEvent($"Planowy {eventName}");
                }
                catch (OperationCanceledException)
                {
                    // cancellation requested - exit loop
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error in {EventName} loop", eventName);
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error while starting {EventName} loop", eventName);
        }
    }

    private async Task WaitUntil(DateTime targetTime, CancellationToken token)
    {
        var delay = targetTime - DateTime.Now;
        if (delay.TotalMilliseconds > 0)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("Task cancelled");
                // ignore
            }
        }
    }

    private void InvokeSunsetEvent(string reason)
    {
        logger.LogInformation("EVENT: ZACHÓD SŁOŃCA ({reason})", reason);
        SafeInvoke(SunsetOccurred, EventArgs.Empty, "SunsetOccurred");
    }

    private void SafeInvoke(EventHandler? handler, EventArgs args, string eventName)
    {
        if (handler == null)
            return;

        var invocationList = handler.GetInvocationList();
        foreach (var @delegate in invocationList)
        {
            try
            {
                if (@delegate is EventHandler eventHandler)
                    eventHandler(this, args);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception thrown by handler for {EventName}; continuing with other handlers", eventName);
            }
        }
    }
}
