using Innovative.SolarCalculator;

namespace choinka.Triggers.SolarTime;
internal class SolarCalculator : ISolarCalculator
{
    private readonly Coordinates _warsaw;
    private readonly Places _places;

    public SolarCalculator(Places places)
    {
        _places = places;
        _warsaw = _places.Coordinates.First(c => c.Name.Equals(
            "Warsaw", StringComparison.Ordinal));
    }

    public DateTime GetWarsawSunset(DateTimeOffset? date = null)
    {
        var time = new SolarTimes(date ?? DateTimeOffset.Now, _warsaw.Latitude, _warsaw.Longitude);
        return time.Sunset;
    }

    public DateTime GetWarsawSunrise(DateTimeOffset? date = null)
    {
        var time = new SolarTimes(date ?? DateTimeOffset.Now, _warsaw.Latitude, _warsaw.Longitude);
        return time.Sunrise;
    }
}
