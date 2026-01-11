namespace choinka.Triggers.SolarTime;

internal interface ISolarCalculator
{
    DateTime GetWarsawSunrise(DateTimeOffset? date = null);
    DateTime GetWarsawSunset(DateTimeOffset? date = null);
}