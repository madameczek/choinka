# choinka

## Project Summary

`choinka` is a small .NET 8 daemon designed for Raspberry Pi that controls a physical "Christmas tree" (LEDs or lights) via GPIO. It turns the lights on automatically at local sunset (uses the SolarCalculator library for Warsaw by default) and turns them off at a configured scheduled time (23:00). The application is built as an extensible background service using Microsoft.Extensions.Hosting and integrates with systemd for reliable auto-start on Linux.

### Blog

The code is an illustration for an article on my blog: [Marek Adameczek: Co w kodzie szeleœci?](https://blog.adameczek.pl/index.php/2026/01/05/bezobslugowe-oswietlenie-choinkowe/).

## Features

- Turn lights on at local sunset using precise solar calculations.
- Turn lights off on a scheduled time (Alarm service).
- Safe GPIO handling with a controller that restores pins to configured states on shutdown.
- Runs as a systemd service; logs via Serilog.

## Requirements

- Raspberry Pi or Linux device with GPIO access
- Recommended packages (already referenced): `SolarCalculator`, `System.Device.Gpio`, `Serilog`, `Microsoft.Extensions.Hosting`.

## Hardware Wiring (Default Pins)

- Level shifter / converter pin: GPIO 6 (used to power the level converter) — set HIGH to enable.
- Tree output pin: GPIO 26 (controls the tree lights).

Adjust pin numbers in code if your wiring differs.

## Configuration & Code Entry Points

- The app registers a `Places` instance and an `ISolarCalculator` to compute sunset times. See `Program.cs` for DI registration and `Triggers/SolarTime` for solar-related services.
- If the app starts after today's sunset, it triggers the "catch-up" behavior and fires the event immediately.

## Running & Deployment

- Publish the project using `dotnet publish ./choinka.csproj -c Release -r linux-arm -p:PublishSingleFile=true --self-contained true -o ./bin/Release/net8.0/publish/linux-arm/`.
- Transfer the published files to your Raspberry Pi.
- Set up a systemd service file (e.g., `/etc/systemd/system/choinka.service`)
- Detailed instructions for systemd setup can be found on my blog.

## Troubleshooting

- **Permission denied on GPIO**: Ensure the process runs with root or has proper group permissions to access GPIO.
- **Verify pin wiring**: Ensure level shifter is enabled (GPIO 6 HIGH) before enabling the tree pin.
- **Logs**: Serilog is configured; check systemd journal (`journalctl -u your-service-name`) for runtime messages.

## License

See `LICENSE.txt` in the repository for license details.

