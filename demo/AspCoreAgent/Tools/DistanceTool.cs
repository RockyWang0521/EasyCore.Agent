using EasyCore.Agent;

namespace AspCoreAgent.Tools
{
    public class DistanceTool
    {
        [AITool("get_distance")]
        [ToolAuthorize("distance.read")]
        [ToolDescription("获取城市的距离信息")]
        public async Task<DistanceResult> GetDistanceAsync([ToolDescription("城市名称，例如：北京、上海、广州")] string city)
        {
            await Task.CompletedTask;

            return new DistanceResult
            {
                City = city,
                Vehicle = "火车",
                Distance = "500公里",
            };
        }

        public class DistanceResult
        {
            [ToolDescription("城市")]
            public string City { get; set; } = "";

            [ToolDescription("交通工具")]
            public string Distance { get; set; } = "";

            [ToolDescription("交通工具")]
            public string Vehicle { get; set; } = "";
        }
    }
}
