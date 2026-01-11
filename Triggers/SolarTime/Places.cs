namespace choinka.Triggers.SolarTime;

public class Places
{
    public IEnumerable<Coordinates> Coordinates { get; init; } = [
        new Coordinates()
        {
            Name = "Warsaw", Latitude = 52.2298, Longitude = 21.0117
        },
        new Coordinates()
        {
            Name = "Gdansk", Latitude = 54.35, Longitude = 18.6667
        },
    ];
}