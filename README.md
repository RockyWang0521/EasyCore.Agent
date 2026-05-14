# 🚀 EasyCore.Agent

> **EasyCore.Agent** 是一个面向 .NET 的轻量级 Agent SDK，提供会话上下文管理、Tool Calling 自动注册、以及 OpenAI 兼容模型接入能力。  
> This project is a lightweight .NET Agent SDK with context memory, tool calling, and OpenAI-compatible model integration.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![AI Agent](https://img.shields.io/badge/AI-Agent-blueviolet)
![Context](https://img.shields.io/badge/Context-Redis%20%7C%20Memory-red?logo=redis)

---

## 🌍 Language

- 中文（当前文档）
- English: [README.en.md](https://github.com/RockyWang0521/EasyCore.Agent/blob/master/README.en.md)

---

## 📚 目录

- [1. 项目简介](#1-项目简介)
- [2. 架构图](#2-架构图)
- [3. 核心特性](#3-核心特性)
- [4. 快速开始](#4-快速开始)
- [5. 配置说明](#5-配置说明)
- [6. Tool 开发指南](#6-tool-开发指南)
- [7. API 使用示例](#7-api-使用示例)
- [8. 最佳实践](#8-最佳实践)
- [9. FAQ](#9-faq)
- [10. EasyCore.Agent 详细介绍](#10-easycoreagent-详细介绍)
- [11. EasyCore.Agent.Workflow 详细介绍](#11-easycoreagentworkflow-详细介绍)
- [12. Demo 运行](#12-demo-运行)

---

## 1. 项目简介

### 🎯 解决什么问题？

在业务中直接使用大模型 SDK 时，通常会遇到：

- 多轮会话上下文维护繁琐；
- Tool 注册和函数调用接入成本高；
- 不同存储模式（本地内存/Redis）切换不方便。

**EasyCore.Agent** 通过统一抽象简化以上问题，让你更快构建可落地的 Agent 服务。

---

## 2. 架构图

### 2.1 组件关系图

![组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/architecture-cn.svg)

### 2.2 一次会话调用时序

![一次会话调用时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/sequence-cn.svg)  

## 3. 核心特性

- 🧠 **多轮上下文记忆**：支持按 `sessionId` 管理历史消息。
- 🧩 **Tool Calling 自动注册**：通过 `[AITool]` 自动识别可调用方法。
- 🗄️ **可切换上下文存储**：`Memory`（开发）与 `Redis`（生产）双模式。
- 🔌 **OpenAI 兼容接入**：支持自定义 `BaseUrl` 与 `Model`。
- 🧱 **清晰扩展点**：基于 `BasicAgentClient<TOptions>` 便于业务封装。
- 🎯 **按名称精确选择 Tool**：支持通过 `GetToolsByNames(...)`,`GetToolsByAuth(...)`,`GetToolsByNamesAndAuth(...)` 精确注入当前回合所需工具。
- 🧾 **响应对象直出**：支持直接返回 `AgentResponse`，满足高级编排与调试场景。

---

## 4. 快速开始

### 4.1 引用项目

通过nuget 将 `EasyCore.Agent` 引入你的解决方案。

### 4.2 注册服务

```csharp
using EasyCore.Agent;

builder.Services.EasyCoreAgent(options =>
{
    options.AgentContextStoreType = AgentContextStoreType.Memory; // or Redis
    options.MaxContextCount = 20;

    // Redis optional config
    // options.EndPoints = "127.0.0.1:6379";
    // options.Password = "";
    // options.DistributedName = "easycore:agent:";
});
```

### 4.3 定义你的 Agent Client

```csharp
public class DeepSeekAgent : BasicAgentClient<DeepSeekClientOptions>
{
    public DeepSeekAgent(
        IOptions<AgentClientOptions> options,
        IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }
}
```

### 4.4 创建 Agent 并对话

```csharp
var tools = toolProvider.GetTools();

var agent = agentClient.CreateAgent(
    agentName: "assistant",
    instructions: "你是一个专业助手",
    tools: tools);

var answer = await agentClient.ChatRunAsync(
    sessionId: "user-001",
    agent: agent,
    message: "帮我查一下今天上海天气");
```

---

## 5. 配置说明

### 5.1 `AgentClientOptions`

| 字段 | 说明 | 示例 |
|---|---|---|
| `ApiKey` | 模型服务密钥 | `sk-xxxx` |
| `BaseUrl` | 模型服务地址 | `https://api.openai.com/v1` |
| `Model` | 模型名称 | `gpt-4.1-mini` |
| `EnvName` | 当 `ApiKey` 未配置时读取的环境变量名 | `EASYCORE_AGENT_API_KEY` |

### 5.2 `AgentConfigOptions`

| 字段 | 说明 | 建议 |
|---|---|---|
| `AgentContextStoreType` | 上下文存储类型（Memory/Redis） | 本地开发用 Memory |
| `MaxContextCount` | 最大上下文条数 | 20~50 |
| `EndPoints` | Redis 地址 | `127.0.0.1:6379` |
| `Password` | Redis 密码 | 按环境配置 |
| `DistributedName` | Redis Key 前缀 | `easycore:agent:` |

---

## 6. Tool 开发指南

### 6.1 定义工具类

```csharp
public class WeatherTool
{
    [AITool("get_weather")]
    [ToolDescription("根据城市获取天气")]
    public string GetWeather(string city)
    {
        return $"{city} 当前天气晴，25℃";
    }
}
```

### 6.2 注册机制说明

系统会扫描运行目录程序集中的 `public` 实例方法，识别 `[AITool]` 并注册到 `IAIToolProvider`。

---

## 7. API 使用示例

### 7.1 多轮会话（带上下文）

```csharp
var answer = await agentClient.ChatRunAsync(sessionId, agent, userInput);
```

### 7.2 单轮调用（无上下文）

```csharp
var answer = await agentClient.ChatRunAsync(agent, "hello");
```
### 7.3 单轮调用（支持 ChatMessage / Messages）

```csharp
using Microsoft.Extensions.AI;

var msg = new ChatMessage(ChatRole.User, "请总结这段内容");
var answer1 = await agentClient.ChatRunAsync(agent, msg);

var messages = new List<ChatMessage>
{
    new(ChatRole.System, "你是技术文档助手"),
    new(ChatRole.User, "解释一下这个接口")
};
var answer2 = await agentClient.ChatRunAsync(agent, messages);
```

### 7.4 返回原始 AgentResponse（便于高级场景）

```csharp
var response1 = await agentClient.ChatRunAgentResponseAsync(agent, "hello");
var response2 = await agentClient.ChatRunAgentResponseAsync(agent, new ChatMessage(ChatRole.User, "你好"));
var response3 = await agentClient.ChatRunAgentResponseAsync(agent, messages);

var text = response1.Text;
```

### 7.5 清空上下文

```csharp
agentClient.ClearChatContext(sessionId);
```
### 7.6 创建 Agent（支持命名与匿名）

```csharp
// 具名 Agent（便于日志与多 Agent 协同场景）
var namedAgent = agentClient.CreateAgent(
    agentName: "planner",
    instructions: "你是一个计划助手",
    tools: tools);

// 匿名 Agent（简单场景）
var defaultAgent = agentClient.CreateAgent(
    instructions: "你是一个通用助手",
    tools: tools);
```

### 7.7 按名称选择工具后注入 Agent

```csharp
var tools = _toolProvider.GetToolsByNames(agentRouteDecision!.ToolName!);

var agent = agentClient.CreateAgent(
    agentName: "router-agent",
    instructions: "根据路由决策调用工具",
    tools: tools);
```

### 7.8 CreateAgent 重载（对应新增能力）

```csharp
public AIAgent CreateAgent(string agentName, string instructions, IList<AITool>? tools = null);
public AIAgent CreateAgent(string instructions, IList<AITool>? tools = null);
```

说明：

- 第一个重载适合多 Agent 协同/可观测性场景（可显式设置 `agentName`）。
- 第二个重载适合简单场景（只给系统指令和工具）。
- 两个重载内部都会读取 `ApiKey`、`BaseUrl`、`Model`，并创建可调用 Tool 的 `AIAgent`。
- `ApiKey` 未设置时，会按 `EnvName` 指定的环境变量读取（默认 `EASYCORE_AGENT_API_KEY`）。

### 7.9 ChatRunAgentResponseAsync / ChatRunAsync 新增重载（对应新增能力）

```csharp
public Task<AgentResponse> ChatRunAgentResponseAsync(AIAgent agent, string message, CancellationToken cancellationToken = default);
public Task<AgentResponse> ChatRunAgentResponseAsync(AIAgent agent, ChatMessage message, CancellationToken cancellationToken = default);
public Task<AgentResponse> ChatRunAgentResponseAsync(AIAgent agent, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);

public Task<string> ChatRunAsync(AIAgent agent, string message, CancellationToken cancellationToken = default);
public Task<string> ChatRunAsync(AIAgent agent, ChatMessage message, CancellationToken cancellationToken = default);
public Task<string> ChatRunAsync(AIAgent agent, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
```

说明：

- `ChatRunAgentResponseAsync` 返回原始 `AgentResponse`，适合需要读取更多响应信息的高级场景。
- `ChatRunAsync` 返回 `response.Text`，适合只关心文本结果的常规场景。
- 三组入参分别支持 `string`、单条 `ChatMessage`、多条 `ChatMessage`（`IEnumerable`）。


---

## 8. 最佳实践

- ✅ 生产环境优先使用 Redis，保障多实例上下文一致性。
- ✅ 建议通过网关或中间件统一注入 `sessionId`。
- ✅ 对 Tool 输入参数做业务校验，避免高风险调用。
- ✅ 记录请求耗时与工具调用日志，便于排障和优化。

---

## 9. FAQ

### ❓ Q1：`ApiKey/BaseUrl/Model is not configured` 报错？
请确认配置非空，且不要包含不可见字符（如全角空格、换行）。
补充：当 `ApiKey` 未显式配置时，SDK 会自动尝试从 `EnvName` 指定的环境变量中读取（默认 `EASYCORE_AGENT_API_KEY`）。

### ❓ Q2：工具为什么没有被调用？
请检查：
1. 是否为 `public` 实例方法；
2. 是否添加 `[AITool("tool_name")]`；
3. 工具所在程序集是否被扫描。

### ❓ Q3：上下文为何丢失？
- Memory 模式仅进程内有效；
- 若需持久化/多实例共享，请使用 Redis。

---

## 10. EasyCore.Agent 详细介绍

### 10.1 设计目标

`EasyCore.Agent` 的核心目标是：让你在 ASP.NET Core / 后端服务里，以更少样板代码完成 Agent 工程化接入。它重点解决三个问题：

1. **上下文记忆**：按会话持久化消息历史，避免业务层重复拼接上下文；
2. **Tool Calling**：通过特性自动发现工具方法，减少手工注册成本；
3. **模型接入统一化**：对 OpenAI 兼容接口做统一封装，便于切换模型与 BaseUrl。

### 10.2 核心能力拆解

- **上下文存储抽象**：支持 `Memory` 和 `Redis` 两类上下文存储策略，可按环境切换。  
- **Agent 客户端基类**：基于 `BasicAgentClient<TOptions>` 可快速派生业务 Agent 客户端。  
- **工具扫描与注册**：扫描程序集中的 `public` 实例方法，识别 `[AITool]` 自动暴露为可调用工具。  
- **统一会话入口**：通过 `ChatRunAsync` 支持单轮/多轮调用，并可按 `sessionId` 清理上下文。

### 10.3 在业务中的推荐落地方式

1. 用 DI 注册 `EasyCore.Agent`；
2. 继承 `BasicAgentClient<TOptions>` 定义你的模型客户端；
3. 把业务能力沉淀为 Tool（如查询天气、查订单、触发流程）；
4. 在 Controller / ApplicationService 中只做编排，不直接耦合模型 SDK；
5. 对每个请求统一注入 `sessionId`，保证上下文可追踪。
   
---

## 11. EasyCore.Agent.Workflow 详细介绍

`EasyCore.Agent.Workflow` 是在 `EasyCore.Agent` 基础上提供的 **流程编排层**。如果说 `EasyCore.Agent` 解决的是“单个 Agent 怎么用”，那么 Workflow 解决的是“多个步骤/多个 Agent 如何按流程协作”。

### 11.1 适用场景

- 一个用户请求要经过 **意图识别 → 分支处理 → 汇总输出**；
- 某些节点需要 **并行执行**（例如并行生成 Controller 和 DTO）；
- 需要保留流程轨迹（Trace）用于调试、审计和性能分析。

### 11.2 基于 AspCoreAgent Demo 的 Workflow 流程讲解

在 `AspCoreAgent` 的 `WorkflowService.RunAsync` 中，流程被组织为：

1. **Step1**：先做意图识别（写入 `intent`）；
2. **Branch**：根据 `intent` 进入不同分支：
   - `intent == 1`：代码生成分支（Step2 → Step3/Step4 并行 → Step5）；
   - `intent == 2`：SQL 生成分支（Step6）；
   - 其他：普通聊天分支（Step7）；
3. **Step8**：无论哪个分支，最后统一总结输出。

#### 11.2.1 流程图（Mermaid）


```
 /// <summary>
 /// 多 Agent 协同，流程执行演示
 /// </summary>
 /// <param name="input"></param>
 /// <param name="cancellationToken"></param>
 /// <returns></returns>
 [AITool("get_workflow_test")]
 [ToolDescription("执行流程")]
 public async Task<string?> RunAsync([ToolDescription("输入自然整数的数字，例如：1，2。数字的范围上下限是1到2。")] string input, CancellationToken cancellationToken = default)
 {
     var workflow = AgentWorkflow.Create()
         // Step1：意图识别 Agent
         .AddFunc(Step1Async)

         // 根据 intent 选择不同流程
         .AddBranch(branch => branch

             // intent == 1：代码生成流程
             .If(ctx => ctx.Get<string>("intent") == "1", flow => flow
                 // Step2：计划 Agent
                 .AddFunc(Step2Async)

                 // Step3 / Step4 并行执行
                 .AddParallel(parallel => parallel
                     // Step3：Controller 生成 Agent
                     .AddFunc(Step3Async)

                     // Step4：DTO 生成 Agent
                     .AddFunc(Step4Async))

                 // Step5：合并 Agent
                 .AddFunc(Step5Async))

             // intent == 2：SQL 生成流程
             .ElseIf(ctx => ctx.Get<string>("intent") == "2", flow => flow
                 // Step6：SQL Agent
                 .AddFunc(Step6Async))

             // 兜底流程
             .Else(flow => flow
                 // Step7：普通聊天 Agent
                 .AddFunc(Step7Async)))

         // Step8：最终总结 Agent
         .AddFunc(Step8Async);

     var context = new AgentWorkflowContext
     {
         Input = input
     };

     await _workflowRunner.RunAsync(workflow, context, cancellationToken);

     return context.Output;
 }

 /// <summary>
 /// Step1：意图识别 Agent
 /// 
 /// 作用：
 /// 根据用户输入判断走哪个分支。
 /// 
 /// 输入：
 /// context.Input
 /// 
 /// 输出：
 /// context.Items["intent"]
 /// </summary>
 private async Task Step1Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     var input = context.Input?.Trim();

     // 模拟 IntentAgent 的判断结果
     // 实际项目里，这里可以调用 DeepSeekAgent / OpenAI Agent
     if (input == "1")
     {
         context.Set("intent", "1");
         context.Set("intent_description", "代码生成流程");
     }
     else if (input == "2")
     {
         context.Set("intent", "2");
         context.Set("intent_description", "SQL生成流程");
     }
     else
     {
         context.Set("intent", "other");
         context.Set("intent_description", "普通聊天流程");
     }

     Console.WriteLine($"step1--意图识别结果：{context.Get<string>("intent_description")}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step2：计划 Agent
 /// 
 /// 作用：
 /// 根据用户输入生成代码生成计划。
 /// 
 /// 输入：
 /// context.Input
 /// 
 /// 输出：
 /// context.Items["plan"]
 /// context.Next(plan)
 /// </summary>
 private async Task Step2Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     // 模拟 PlanAgent
     var plan = $"""
         【PlanAgent 输出】
       
         用户输入：
         {context.Input}
       
         代码生成计划：
         1. 生成 ProductController
         2. 生成 ProductDto
         3. 最后合并 Controller 和 DTO
         """;

     context.Set("plan", plan);

     // Next 表示：把当前输出作为下一个步骤的输入
     context.Next(plan);

     Console.WriteLine($"step2--计划生成结果：{plan}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step3：Controller 生成 Agent
 /// 
 /// 作用：
 /// 根据 Step2 的计划生成 Controller。
 /// 
 /// 注意：
 /// 这是并行节点，不要调用 context.Next。
 /// 并行节点只写自己的结果到 Items。
 /// 
 /// 输入：
 /// context.Items["plan"]
 /// 
 /// 输出：
 /// context.Items["controller"]
 /// </summary>
 private async Task Step3Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     var plan = context.Get<string>("plan");

     // 模拟 ControllerAgent
     var controller = $$"""
        【ControllerAgent 输出】
        
        根据计划生成 Controller：
        
        {{plan}}
        
        public sealed class ProductController : ControllerBase
        {
            [HttpGet("{id}")]
            public IActionResult Get(Guid id)
            {
                return Ok(new ProductDto
                {
                    Id = id,
                    Name = "测试商品"
                });
            }
        }
        """;

     context.Set("controller", controller);

     Console.WriteLine($"step3--Controller 生成结果：{controller}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step4：DTO 生成 Agent
 /// 
 /// 作用：
 /// 根据 Step2 的计划生成 DTO。
 /// 
 /// 注意：
 /// 这是并行节点，不要调用 context.Next。
 /// 并行节点只写自己的结果到 Items。
 /// 
 /// 输入：
 /// context.Items["plan"]
 /// 
 /// 输出：
 /// context.Items["dto"]
 /// </summary>
 private async Task Step4Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     var plan = context.Get<string>("plan");

     // 模拟 DtoAgent
     var dto = $$"""
        【DtoAgent 输出】
        
        根据计划生成 DTO：
        
        {{plan}}
        
        public sealed class ProductDto
        {
            public Guid Id { get; set; }
        
            public string Name { get; set; } = string.Empty;
        }
        """;

     context.Set("dto", dto);

     Console.WriteLine($"step4--DTO 生成结果：{dto}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step5：合并 Agent
 /// 
 /// 作用：
 /// 等 Step3 和 Step4 都执行完之后，读取并行结果并合并。
 /// 
 /// 输入：
 /// context.Items["controller"]
 /// context.Items["dto"]
 /// 
 /// 输出：
 /// context.Next(result)
 /// </summary>
 private async Task Step5Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     var controller = context.Get<string>("controller");
     var dto = context.Get<string>("dto");

     // 模拟 MergeAgent
     var result = $"""
      【MergeAgent 输出】
   
      ===== Controller =====
   
      {controller}
   
      ===== DTO =====
   
      {dto}
   
      合并说明：
      Controller 和 DTO 已经生成完成。
      """;

     // 合并后的结果给后面的 Step8 使用
     context.Next(result);

     Console.WriteLine($"step5--合并结果：{result}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step6：SQL 生成 Agent
 /// 
 /// 作用：
 /// 当 intent == 2 时，走 SQL 分支。
 /// 
 /// 输入：
 /// context.Input
 /// 
 /// 输出：
 /// context.Next(sqlResult)
 /// </summary>
 private async Task Step6Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     // 模拟 SqlAgent
     var sqlResult = $"""
        【SqlAgent 输出】
     
        用户输入：
        {context.Input}
     
        生成 SQL：
     
        SELECT *
        FROM Products
        WHERE IsDeleted = 0
        ORDER BY CreateTime DESC;
        """;

     context.Set("sql_result", sqlResult);

     context.Next(sqlResult);

     Console.WriteLine($"step6--SQL 生成结果：{sqlResult}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step7：普通聊天 Agent
 /// 
 /// 作用：
 /// 当 intent 不是 1 或 2 时，走普通聊天分支。
 /// 
 /// 输入：
 /// context.Input
 /// 
 /// 输出：
 /// context.Next(answer)
 /// </summary>
 private async Task Step7Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     // 模拟 ChatAgent
     var answer = $"""
        【ChatAgent 输出】
     
        你输入的是：
        {context.Input}
     
        当前没有匹配到代码生成流程或 SQL 生成流程，所以进入普通聊天流程。
        """;

     context.Next(answer);

     Console.WriteLine($"step7--普通聊天结果：{answer}");

     await Task.CompletedTask;
 }

 /// <summary>
 /// Step8：最终总结 Agent
 /// 
 /// 作用：
 /// 所有分支执行完成后，统一做最终输出。
 /// 
 /// 输入：
 /// context.Output
 /// 
 /// 输出：
 /// context.Output
 /// </summary>
 private async Task Step8Async(AgentWorkflowContext context, CancellationToken cancellationToken)
 {
     var intent = context.Get<string>("intent");
     var intentDescription = context.Get<string>("intent_description");

     // 模拟 SummaryAgent
     context.Output = $"""
        【SummaryAgent 输出】
      
        流程执行完成。
      
        Intent：
        {intent}
      
        Intent说明：
        {intentDescription}
      
        最终结果：
      
        {context.Output}
        """;

     Console.WriteLine($"step8--最终总结结果：{context.Output}");

     await Task.CompletedTask;
 }
```

![流程图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/workflow-cn.svg)

#### 11.2.2 执行要点

- **分支前置判定**：Step1 只负责路由，不直接产出最终答案；
- **并行节点约束**：并行节点（Step3/Step4）建议只写 `Items`，不要相互覆盖 `Output`；
- **合并节点职责**：Step5 汇总并行结果后再 `Next` 给下游；
- **统一收口**：Step8 将不同分支输出包装成统一结构，便于前端展示。

### 11.3 EasyCore.Agent.Workflow API 说明（仅名称 + 功能）

> 以下只介绍 API 名称与用途，不绑定到具体类，方便从“能力视角”理解。

#### 11.3.1 流程构建 API

- **Create**：创建一个新的流程定义实例。  
- **AddFunc**：添加一个普通步骤（支持同步/异步委托重载）。  
- **AddBranch**：添加条件分支容器。  
- **AddParallel**：添加并行执行容器。  
- **RunAsync**：按顺序执行流程定义中的步骤。

#### 11.3.2 分支编排 API

- **If**：定义第一个条件分支。  
- **ElseIf**：定义后续条件分支。  
- **Else**：定义兜底分支。

#### 11.3.3 并行编排 API

- **AddFunc**：向并行容器中添加并行步骤。  
- **AddFlow**：向并行容器中添加一段子流程。  
- **AddBranch**：向并行容器中添加一个分支子流程。

#### 11.3.4 上下文与数据流 API

- **Set**：将步骤产出写入上下文键值区。  
- **Get**：按键读取上下文值。  
- **Get<T>**：按类型安全方式读取上下文值。  
- **Next**：把当前输出作为下一个步骤输入，同时刷新 `Output`。

#### 11.3.5 运行与接入 API

- **EasyCoreAgentWorkflow**：注册 Workflow 运行能力到依赖注入。  
- **RunAsync**：通过运行器触发流程执行（通常由应用服务调用）。

#### 11.3.6 轨迹数据字段（用于可观测性）

- **StepName**：步骤名称。  
- **StepType**：步骤类型（如 Func / Branch / Parallel）。  
- **StartTime / EndTime**：步骤开始与结束时间。  
- **ElapsedMilliseconds**：步骤耗时（毫秒）。  
- **Success**：步骤是否成功。  
- **ErrorMessage**：失败时的异常信息。
  
---

## 12. Demo 运行

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

---

## 📄 License

请根据你的项目需求补充 License（MIT / Apache-2.0 / 私有协议）。
