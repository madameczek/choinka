using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Device.Gpio;

namespace choinka.Gpio;
public class GpioControllerWithPinRestore : GpioController, IDisposable
{
    private readonly ConcurrentDictionary<int, PinState> _pins = [];
    private readonly ILogger<GpioControllerWithPinRestore>? _logger;

    public GpioControllerWithPinRestore(ILogger<GpioControllerWithPinRestore> logger) : base()
    {
        _logger = logger;
    }

    public GpioControllerWithPinRestore() : base()
    { }

    public new GpioPin OpenPin(int pinNumber, PinMode mode, PinValue onCloseValue)
    {
        var pin = OpenPin(pinNumber, mode);
        _pins.TryAdd(pinNumber, PinState.CreateState(pin, onCloseValue, null));
        return pin;
    }

    public GpioPin OpenPin(int pinNumber, PinMode mode, PinValue initialValue, PinValue onCloseValue)
    {
        var pin = base.OpenPin(pinNumber, mode, initialValue);
        _pins.TryAdd(pinNumber, PinState.CreateState(pin, onCloseValue, initialValue));
        return pin;
    }

    public new void Write(int pinNumber, PinValue value)
    {
        var isPinExisting = _pins.TryGetValue(pinNumber, out var pinState);
        if (isPinExisting)
        {
            pinState!.Value = value;
            _pins[pinNumber] = pinState;
        }
        base.Write(pinNumber, value);
    }

    public new bool IsPinOpen(int pinNumber)
    {
        if (!_pins.TryGetValue(pinNumber, out _))
            return false;
        return base.IsPinOpen(pinNumber);
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public new void Dispose(bool disposing)
    {
        foreach (var pin in _pins)
        {
            try
            {
                Write(pin.Key, pin.Value.OnCloseValue);
                ClosePin(pin.Key);
            }
            catch (Exception)
            {
                // ignore
            }
        }
        base.Dispose(disposing);
    }
}
