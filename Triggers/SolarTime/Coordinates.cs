using Innovative.Geometry;

namespace choinka.Triggers.SolarTime;
public class Coordinates
{
    public string Name { get; init; } = null!;
    public Angle Latitude { get; init; } = Angle.Empty;
    public Angle Longitude { get; init; } = Angle.Empty;
}
