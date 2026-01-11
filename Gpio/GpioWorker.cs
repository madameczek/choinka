using choinka.Triggers.SolarTime;
using choinka.Triggers.Timed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Device.Gpio;

namespace choinka.Gpio;

internal class GpioWorker(
    ILogger<GpioWorker> logger,
    GpioControllerWithPinRestore gpioController,
    SolarNotifierService solarNotifier,
    AlarmClockService alarmClockService) : BackgroundService
{
    const int _pinChoinka = 26;
    const int _pinLevelConverter = 6;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            solarNotifier.SunsetOccurred += Choinka_OnEventOccurred;
            alarmClockService.Alarm2300Triggered += Choinka_OffEventOccurred;

            gpioController.OpenPin(_pinLevelConverter, PinMode.Output, PinValue.High, PinValue.Low);
            if (gpioController.Read(_pinLevelConverter) == PinValue.Low)
                gpioController.Write(_pinLevelConverter, PinValue.High);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("GpioWorker cancellation requested");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing GpioWorker. In case of error 13," +
                " try to elevate privileges and run the application with 'sudo'.");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("GpioWorker stopping, unsubscribing events and cleaning up pins");

        try
        {
            solarNotifier.SunsetOccurred -= Choinka_OnEventOccurred;
            alarmClockService.Alarm2300Triggered -= Choinka_OffEventOccurred;

            if (gpioController.IsPinOpen(_pinChoinka))
            {
                gpioController.Write(_pinChoinka, PinValue.Low);
                gpioController.ClosePin(_pinChoinka);
            }

            if (gpioController.IsPinOpen(_pinLevelConverter))
            {
                gpioController.Write(_pinLevelConverter, PinValue.Low);
                gpioController.ClosePin(_pinLevelConverter);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error while stopping GpioWorker");
        }

        return base.StopAsync(cancellationToken);
    }

    private void Choinka_OnEventOccurred(object? sender, EventArgs e)
    {
        if(!gpioController.IsPinOpen(_pinLevelConverter))
            gpioController.OpenPin(_pinLevelConverter, PinMode.Output, PinValue.High, PinValue.Low);

        if (gpioController.Read(_pinLevelConverter) == PinValue.Low)
            gpioController.Write(_pinLevelConverter, PinValue.High);

        if (!gpioController.IsPinOpen(_pinChoinka))
            gpioController.OpenPin(_pinChoinka, PinMode.Output, PinValue.Low, PinValue.Low);

        if (gpioController.Read(_pinChoinka) == PinValue.Low)
            gpioController.Write(_pinChoinka, PinValue.High);
    }

    private void Choinka_OffEventOccurred(object? sender, EventArgs e)
    {

        if (!gpioController.IsPinOpen(_pinChoinka))
            gpioController.OpenPin(_pinChoinka, PinMode.Output, PinValue.Low, PinValue.Low);
        else
            gpioController.Write(_pinChoinka, PinValue.Low);

        logger.LogInformation("Stan wyjścia: {StanChoinka}", gpioController.Read(_pinChoinka));
    }
}
