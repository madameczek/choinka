using System.Device.Gpio;

namespace choinka.Gpio;
public class PinState(GpioPin pin, PinValue onClose, PinValue? value)
{
    public static PinState CreateState(GpioPin pin, PinValue onCloseValue, PinValue? value) =>
        new(pin, onCloseValue, value);

    public GpioPin Pin { get; set; } = pin;
    public PinValue OnCloseValue { get; set; } = onClose;
    public PinValue? Value { get; set; } = value;
}
