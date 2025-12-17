using System.ComponentModel;

namespace AI.Workshop.ConsoleApps.Agents;

internal static class WeatherTools
{
    [Description("Get the weather for a given location.")]
    internal static string GetWeather([Description("The location to get the weather for.")] string location)
        => $"The weather in {location} is cloudy with a high of 15°C.";
}
