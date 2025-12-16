namespace AI.Workshop.ChatApp.Web.Tools;

/// <summary>
/// Tool that returns the current date and time
/// </summary>
public class CurrentTimeTool
{
    public Task<string> GetCurrentTimeAsync(CancellationToken ct = default)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        return Task.FromResult(now.ToString("dd.MM.yyyy HH:mm:ss"));
    }
}
