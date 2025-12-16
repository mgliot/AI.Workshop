using System.ComponentModel;

namespace AI.Workshop.ConsoleApps.AgentChat.Tools;

internal class CurrentTimeTool
{
    [Description("Returns the current date and time for Central European Time Zone. This tool needs no parameters.")]
    public Task<string> InvokeAsync(IDictionary<string, object>? parameters = null, CancellationToken ct = default)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        return Task.FromResult(now.ToString("dd.MM.yyyy HH:mm:ss"));
    }
}
