namespace choinka.Infrastructure.Logging.Configuration;

public class AppLoggerConfiguration
{
    public string GraylogAddress { get; set; } = null!;
    public int GraylogPort { get; set; }
    public string GraylogMinimumLevel { get; set; } = null!;
    public string GraylogAppName { get; set; } = null!;
    public string? HostName { get; set; } = null;
}