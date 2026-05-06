using AspCoreAgent.Agent;
using AspCoreAgent.Route;
using EasyCore.Agent;
using Microsoft.AspNetCore.Mvc;
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

        public AgentController(DeepSeekAgent agent, IAIToolProvider toolProvider)
        {
            _agent = agent;
            _toolProvider = toolProvider;
        }

        [HttpGet]
        public async Task<string> Get(string message, string? sessionId = null)
        {
            const string agentName = "普通聊天助手";
            const string instructions = "  你是一个普通聊天助手。你只回答用户当前问题。不调用工具。不生成项目。不进入代码生成工作流。 回答要直接、清楚、可执行。";

            sessionId ??= "default";

            var agent = _agent.CreateAgent(agentName, instructions);

            return await _agent.ChatRunAsync(sessionId, agent, message);
        }

        [HttpPost]
        public async Task<string> AiTool(string message, string? sessionId = null)
        {
            const string agentName = "工具调度助手";
            const string instructions = "  你是一个工具调度助手。你的任务：1. 判断用户问题是否需要调用工具。2. 如果需要，选择最合适的工具调用。3. 如果不需要工具，直接回答用户。4. 不要生成项目。5. 不要进入代码生成工作流。6. 最终回答要直接、清楚、可执行。7.严格按照返回值的格式返回结果。";

            sessionId ??= "default";

            var tools = _toolProvider.GetTools("get_weather");

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

            const string agentName2 = "工具调度助手";
            const string instructions2 = "  你是一个工具调度助手。你的任务：1. 判断用户问题是否需要调用工具。2. 如果需要，选择最合适的工具调用。3. 如果不需要工具，直接回答用户。4. 不要生成项目。5. 不要进入代码生成工作流。6. 最终回答要直接、清楚、可执行。7.严格按照返回值的格式返回结果。";

            sessionId ??= "default";

            var tools = _toolProvider.GetTools(agentRouteDecision!.ToolName!);

            var agent2 = _agent.CreateAgent(agentName2, instructions2, tools);

            return await _agent.ChatRunAsync(sessionId, agent2, agentRouteDecision.UserQuestion);
        }
    }
}