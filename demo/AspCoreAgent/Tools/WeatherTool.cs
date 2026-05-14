using EasyCore.Agent;

namespace MultiStackSolutionGenerator.Api.Tools
{
    public class WeatherTool
    {
        [AITool("get_weather")]
        [ToolAuthorize("weather.read")]
        [ToolDescription("获取天气信息")]
        public async Task<WeatherResult> GetWeatherAsync([ToolDescription("城市名称，例如：北京、上海、广州")] string city)
        {
            await Task.CompletedTask;

            return new WeatherResult
            {
                City = city,
                Weather = "下雨",
                Temperature = "25℃"
            };
        }

        public class WeatherResult
        {
            [ToolDescription("城市")]
            public string City { get; set; } = "";

            [ToolDescription("天气")]
            public string Weather { get; set; } = "";

            [ToolDescription("温度")]
            public string Temperature { get; set; } = "";
        }
    }
}
