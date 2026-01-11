using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace choinka.Triggers.Timed;
internal class AlarmClockService(ILogger<AlarmClockService> logger) : BackgroundService
{
    public event EventHandler? Alarm2300Triggered;
    public event EventHandler? Alarm730Triggered;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var alarm2300Task = RunEventLoopAsync(
            "Minęła 23:00",
            new TimeOnly(23, 00),
            reason => InvokeAlarmEvent(reason, Alarm2300Triggered),
            stoppingToken);


        var alarm730Task = RunEventLoopAsync(
            "Minęła 7:30",
            new TimeOnly(7, 30),
            reason => InvokeAlarmEvent(reason, Alarm730Triggered),
            stoppingToken);

        await Task.WhenAll(alarm2300Task, alarm730Task);
        //await alarm2300Task;
    }


    private async Task RunEventLoopAsync(
       string eventName,
       TimeOnly alarmTime,
       Action<string> invokeEvent,
       CancellationToken token)
    {
        try
        {
            // Initial check + possible catch-up
            var alarmTimeSpan = alarmTime.ToTimeSpan();
                var todayTime = DateTime.Now.Date.Add(alarmTimeSpan);
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

            // Daily loop
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var tomorrow = DateTimeOffset.Now.Date.AddDays(1);
                    var nextTime = tomorrow.Add(alarmTimeSpan);

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
        { }
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
                // ignore
            }
        }
    }

    private void InvokeAlarmEvent(string reason, EventHandler? handler)
    {
        logger.LogInformation("EVENT: BUDZIK ({reason})", reason);
        SafeInvoke(handler, EventArgs.Empty, "AlarmClock");
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
