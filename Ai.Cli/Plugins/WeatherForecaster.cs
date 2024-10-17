using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Ai.Cli.Plugins;

public class WeatherForecaster
{
    [KernelFunction]
    [Description("This function retrieves weather at given destination.")]
    [return: Description("Weather at given destination.")]
    public string GetTodaysWeather([Description("The destination to retrieve the weather for.")] string destination)
    {
        // <--------- This is where you would call a fancy weather API to get the weather for the given <<destination>>.
        // We are just simulating a random weather here.
        var weatherPatterns = new[] { "Sunny", "Cloudy", "Windy", "Rainy", "Snowy" };
        
        var rand = new Random();
        return weatherPatterns[rand.Next(weatherPatterns.Length)];
    }
}