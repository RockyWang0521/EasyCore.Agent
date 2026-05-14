using AspCoreAgent.Agent;
using AspCoreAgent.Route;
using AspCoreAgent.Tools;
using EasyCore.Agent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspCoreAgent.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly DeepSeekAgent _agent;
        private readonly IAIToolProvider _toolProvider;
        private readonly PipelineTool _pipelineTool;

        public AgentController(DeepSeekAgent agent, IAIToolProvider toolProvider, PipelineTool pipelineTool)
        {
            _agent = agent;
            _toolProvider = toolProvider;
            _pipelineTool = pipelineTool;
        }

        [HttpGet("Get")]
        public async Task<string> Get(string message, string? sessionId = null)
        {
            const string agentName = "普通聊天助手";
            const string instructions = "  你是一个普通聊天助手。你只回答用户当前问题。不调用工具。不生成项目。不进入代码生成工作流。 回答要直接、清楚、可执行。";

            sessionId ??= "default";

            var agent = _agent.CreateAgent(agentName, instructions);

            return await _agent.ChatRunAsync(sessionId, agent, message);
        }

        [HttpPost("Post")]
        public async Task<string> AiTool(string message, string? sessionId = null)
        {
            const string agentName = "工具调度助手";
            const string instructions = "  你是一个工具调度助手。你的任务：1. 判断用户问题是否需要调用工具。2. 如果需要，选择最合适的工具调用。3. 如果不需要工具，直接回答用户。4. 不要生成项目。5. 不要进入代码生成工作流。6. 最终回答要直接、清楚、可执行。7.严格按照返回值的格式返回结果。";

            sessionId ??= "default";

            var tools = _toolProvider.GetToolsByNames("get_weather");

            var agent = _agent.CreateAgent(agentName, instructions, tools);

            return await _agent.ChatRunAsync(sessionId, agent, message);
        }

        [HttpPost("ReAct")]
        public async Task<string> ReAct(string message, string? sessionId = null)
        {
            const string agentName = "工具调度助手和普通聊天助手";
            const string instructions = "  你是一个工具调度助手和普通聊天助手。你的任务：1. 判断用户问题是否需要调用工具。2. 如果需要，选择最合适的工具调用。3. 如果不需要工具，直接回答用户。4. 不要生成项目。5. 不要进入代码生成工作流。6. 最终回答要直接、清楚、可执行。7.严格按照返回值的格式返回结果。";

            sessionId ??= "default";

            var tools = _toolProvider.GetTools();

            var agent = _agent.CreateAgent(agentName, instructions, tools);

            return await _agent.ChatRunAsync(sessionId, agent, message);
        }

        [HttpPost]
        public async Task<string> AiToolAuth(string message, string? sessionId = null)
        {
            const string agentName = "工具调度助手";
            const string instructions = "  你是一个工具调度助手。你的任务：1. 判断用户问题是否需要调用工具。2. 如果需要，选择最合适的工具调用。3. 如果不需要工具，直接回答用户。4. 不要生成项目。5. 不要进入代码生成工作流。6. 最终回答要直接、清楚、可执行。7.严格按照返回值的格式返回结果。";

            sessionId ??= "default";

            // 读取数据库或者本地，从用户权限表中或者角色表中查询用户或角色具有的权限，例如 weather.read、work.read、work.insert、knowledge.read 等等。
            var auth = new[] { "weather.read" };

            var tools = _toolProvider.GetToolsByNamesAndAuth(auth, "get_weather");

            var agent = _agent.CreateAgent(agentName, instructions, tools);

            return await _agent.ChatRunAsync(sessionId, agent, message);
        }

        [HttpPost("RouterAgent")]
        public async Task<string> RouterAgent(string message, string? sessionId = null)
        {
            const string agentName = "路由调度助手";
            const string instructions = """
                你是一个 Router Agent，只负责判断用户问题应该交给哪个能力处理。
                
                你不能回答用户问题。
                你不能调用工具。
                你只能返回 JSON。
                
                可选 RouteType：
                
                1. DirectAnswer
                普通聊天、概念解释、简单代码解释。
                
                2. Tool
                需要调用具体工具，例如天气、工单、库存、设备状态。
                如果选择 Tool，必须返回 ToolName。
                
                3. Knowledge
                需要查询知识库、文档、SOP、历史资料、故障库。
                
                4. CodeGeneration
                需要生成完整代码、项目文件、控制器、服务、实体、前端页面。
                
                5. SqlQuery
                需要查询数据库、统计数据、报表数据。
                
                6. Workflow
                需要多步骤流程，例如：
                - 生成完整后端项目
                - 分析故障原因
                - 查询知识库后再生成报告
                - 多 Agent 协作任务
                
                返回格式必须是严格 JSON，不要 Markdown，不要解释。
                
                JSON 格式：
                
                {
                  "routeType": "Tool",
                  "toolName": "get_weather",
                  "workflowName": null,
                  "reason": "用户要查询天气，需要调用天气工具",
                  "userQuestion": "北京天气怎么样"
                }
                
                路由规则：
                
                - 问天气：Tool，toolName = get_weather
                - 问工单：Tool，toolName = get_work
                - 问知识库、文档、资料、SOP、故障说明：Knowledge
                - 要生成完整代码或项目：CodeGeneration
                - 要查询数据库、统计、报表：SqlQuery
                - 需要多步骤、多工具、多 Agent：Workflow
                - 其他普通问题：DirectAnswer
                """;

            var agent = _agent.CreateAgent(agentName, instructions);

            var response = await _agent.ChatRunAsync(agent, message);

            var json = response.Trim();

            var agentRouteDecision = new AgentRouteDecision()
            {
                RouteType = AgentRouteType.DirectAnswer,
                Reason = "Router Agent 返回为空，默认直接回答",
                UserQuestion = message
            };

            if (!string.IsNullOrWhiteSpace(json))
            {
                json = json.Replace("```json", "").Replace("```", "").Trim();

                agentRouteDecision = JsonSerializer.Deserialize<AgentRouteDecision>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } });
            }

            const string agentNameTool = "工具调度助手";
            const string instructionsTool = "  你是一个工具调度助手。你的任务：1. 判断用户问题是否需要调用工具。2. 如果需要，选择最合适的工具调用。3. 如果不需要工具，直接回答用户。4. 不要生成项目。5. 不要进入代码生成工作流。6. 最终回答要直接、清楚、可执行。7.严格按照返回值的格式返回结果。";

            sessionId ??= "default";

            var tools = _toolProvider.GetToolsByNames(agentRouteDecision!.ToolName!);

            var agentTool = _agent.CreateAgent(agentNameTool, instructionsTool, tools);

            return await _agent.ChatRunAsync(sessionId, agentTool, agentRouteDecision.UserQuestion);
        }

        [HttpPost("AgentPipeline")]
        public async Task<string?> AgentPipeline(string message, string? sessionId = null)
        {
            return await _pipelineTool.RunAsync(message);
        }

        [HttpPost("ChatWithImage")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ChatWithImage(
            [FromForm] ChatWithImageRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.Image == null || request.Image.Length == 0)
                return BadRequest("请上传图片");

            await using var stream = request.Image.OpenReadStream();

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);

            var imageBytes = memoryStream.ToArray();

            using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);

            if (mat.Empty())
                return BadRequest("图片解析失败");

            var width = mat.Width;
            var height = mat.Height;

            var instructions = BuildTireDamagePrompt(width, height);

            var agent = _agent.CreateAgent("图片聊天助手", instructions);

            var chatMessage = new ChatMessage(ChatRole.User,
            [
                new TextContent(request.Message),
                new DataContent(imageBytes, GetImageMimeType(request.Image.FileName))
            ]);

            var json = await _agent.ChatRunAsync(agent, chatMessage, cancellationToken: cancellationToken);

            var result = JsonSerializer.Deserialize<TireDamageResult>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null)
                return BadRequest($"模型返回 JSON 格式错误：{json}");

            if (result.HasDamage && result.Damages.Count > 0)
            {
                foreach (var damage in result.Damages)
                {
                    var box = damage.Box;

                    var x1 = Math.Clamp(box.X1, 0, width - 1);
                    var y1 = Math.Clamp(box.Y1, 0, height - 1);
                    var x2 = Math.Clamp(box.X2, 0, width - 1);
                    var y2 = Math.Clamp(box.Y2, 0, height - 1);

                    Console.WriteLine($"Image: {mat.Width}x{mat.Height}");
                    Console.WriteLine($"Box: x1={x1}, y1={y1}, x2={x2}, y2={y2}");

                    if (x2 <= x1 || y2 <= y1)
                        continue;

                    Cv2.Rectangle(
                        mat,
                        new Rect(x1, y1, x2 - x1, y2 - y1),
                        new Scalar(0, 0, 255),
                        thickness: 3);

                    var label = $"{damage.Type} {damage.Confidence:0.00}";

                    Cv2.PutText(
                        mat,
                        label,
                        new Point(x1, Math.Max(y1 - 8, 20)),
                        HersheyFonts.HersheySimplex,
                        0.7,
                        new Scalar(0, 0, 255),
                        thickness: 2);
                }
            }

            Cv2.ImEncode(".jpg", mat, out var outputBytes);

            return File(
                outputBytes,
                "image/jpeg",
                $"marked_{DateTime.Now:yyyyMMddHHmmss}.jpg");
        }

        private static string GetImageMimeType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        private static string BuildTireDamagePrompt(int width, int height)
        {
            return $$"""
你是专业的轮胎破损检测助手。

输入图像尺寸：
- width = {{width}}
- height = {{height}}

你的任务是：
仅检测轮胎表面的“真实物理破损”。

允许检测的破损类型仅包括：
- crack（裂纹）
- scratch（划伤）
- hole（孔洞）
- unknown（无法明确分类）

必须严格忽略以下内容：
- 轮胎正常花纹沟
- 胎面纹理
- 胎侧橡胶纹路
- 阴影
- 灰尘
- 污渍
- 反光
- 橡胶老化痕迹
- 普通磨损
- 图像噪点
- 拍摄伪影

重要规则：

1. 只有当区域明显区别于正常花纹结构时，才允许判定为破损。

2. 若无法确定是真实破损，必须返回：
{
  "hasDamage": false,
  "damages": []
}

3. 不允许猜测性检测。

4. 不允许把连续规则花纹沟识别为 crack。

5. bounding box 必须紧贴真实破损区域，不得过大。

6. box 坐标必须满足：
- x 范围：0~{{width}}
- y 范围：0~{{height}}
- x1 < x2
- y1 < y2

7. confidence 规则：
- 真实明显破损：0.90~1.00
- 不允许输出 confidence < 0.90 的破损

8. 输出必须是纯 JSON：
- 不允许 markdown
- 不允许解释
- 不允许代码块
- 不允许额外文字

输出格式：
{
  "hasDamage": true,
  "damages": [
    {
      "type": "crack",
      "confidence": 0.95,
      "box": {
        "x1": 120,
        "y1": 80,
        "x2": 168,
        "y2": 122
      }
    }
  ]
}
""";
        }
    }
}