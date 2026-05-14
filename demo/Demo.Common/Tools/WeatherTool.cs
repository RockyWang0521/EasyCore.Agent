using EasyCore.Agent;

namespace Demo.Common.Tools;

public class WeatherTool
{
    [AITool("get_weather")]
    [ToolDescription("Gets weather information for the specified city.")]
    public Task<WeatherResult> GetWeatherAsync([ToolDescription("City name, e.g. Beijing or Shanghai.")] string city)
    {
        return Task.FromResult(new WeatherResult
        {
            City = city,
            Weather = "Sunny",
            Temperature = "26C"
        });
    }

    public class WeatherResult
    {
        public string City { get; set; } = string.Empty;

        public string Weather { get; set; } = string.Empty;

        public string Temperature { get; set; } = string.Empty;
    }
}
