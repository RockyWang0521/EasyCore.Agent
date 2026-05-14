# 🚀 EasyCore.Agent

> **EasyCore.Agent** is a lightweight Agent SDK for .NET. It provides conversation context management, automatic Tool Calling registration, and OpenAI-compatible model integration.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![AI Agent](https://img.shields.io/badge/AI-Agent-blueviolet)
![Context](https://img.shields.io/badge/Context-Redis%20%7C%20Memory-red?logo=redis)

---

## 🌍 Language

- Chinese: [README.md](https://github.com/RockyWang0521/EasyCore.Agent/blob/master/README.md)
- English (current document)

---

## 📚 Table of Contents

- [1. Introduction](#1-introduction)
- [2. Architecture Diagrams](#2-architecture-diagrams)
- [3. Core Features](#3-core-features)
- [4. Quick Start](#4-quick-start)
- [5. Configuration](#5-configuration)
- [6. Tool Development Guide](#6-tool-development-guide)
- [7. API Usage Examples](#7-api-usage-examples)
- [8. Best Practices](#8-best-practices)
- [9. FAQ](#9-faq)
- [10. Detailed Introduction to EasyCore.Agent](#10-detailed-introduction-to-easycoreagent)
- [11. Detailed Introduction to EasyCore.Agent.Workflow](#11-detailed-introduction-to-easycoreagentworkflow)
- [12. Running the Demo](#12-running-the-demo)

---

## 1. Introduction

### 🎯 What problem does it solve?

When using large model SDKs directly in business systems, you usually run into these issues:

- Maintaining multi-turn conversation context is tedious;
- Tool registration and function calling integration are costly;
- Switching between different context storage modes, such as local memory and Redis, is inconvenient.

**EasyCore.Agent** simplifies these problems through unified abstractions, helping you build production-ready Agent services faster.

---

## 2. Architecture Diagrams

### 2.1 Component Relationship Diagram

![Component Relationship Diagram](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/architecture-cn.svg)

### 2.2 Single Conversation Call Sequence

![Single Conversation Call Sequence](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/sequence-cn.svg)

---

## 3. Core Features

- 🧠 **Multi-turn context memory**: Manage conversation history by `sessionId`.
- 🧩 **Automatic Tool Calling registration**: Automatically identify callable methods through `[AITool]`.
- 🗄️ **Switchable context storage**: Supports both `Memory` for development and `Redis` for production.
- 🔌 **OpenAI-compatible integration**: Supports custom `BaseUrl` and `Model`.
- 🧱 **Clear extension points**: Built on `BasicAgentClient<TOptions>` for easy business-level encapsulation.
- 🎯 **Precise tool selection** via `GetToolsByNames(...)`, `GetToolsByAuth(...)`, and `GetToolsByNamesAndAuth(...)`.
- 🧾 **Raw response output** with `AgentResponse` for advanced orchestration/debugging.

---

## 4. Quick Start

### 4.1 Add the Package

Add `EasyCore.Agent` to your solution through NuGet.

### 4.2 Register Services

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

### 4.3 Define Your Agent Client

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

### 4.4 Create an Agent and Start a Conversation

```csharp
var tools = toolProvider.GetTools();

var agent = agentClient.CreateAgent(
    agentName: "assistant",
    instructions: "You are a professional assistant.",
    tools: tools);

var answer = await agentClient.ChatRunAsync(
    sessionId: "user-001",
    agent: agent,
    message: "Help me check today's weather in Shanghai.");
```

---

## 5. Configuration

### 5.1 `AgentClientOptions`

| Field | Description | Example |
|---|---|---|
| `ApiKey` | Model service API key | `sk-xxxx` |
| `BaseUrl` | Model service endpoint | `https://api.openai.com/v1` |
| `Model` | Model name | `gpt-4.1-mini` |
| `EnvName` | Env var name used when `ApiKey` is empty | `EASYCORE_AGENT_API_KEY` |

### 5.2 `AgentConfigOptions`

| Field | Description | Recommendation |
|---|---|---|
| `AgentContextStoreType` | Context storage type: Memory or Redis | Use Memory for local development |
| `MaxContextCount` | Maximum number of context messages | 20~50 |
| `EndPoints` | Redis endpoint | `127.0.0.1:6379` |
| `Password` | Redis password | Configure by environment |
| `DistributedName` | Redis key prefix | `easycore:agent:` |

---

## 6. Tool Development Guide

### 6.1 Define a Tool Class

```csharp
public class WeatherTool
{
    [AITool("get_weather")]
    [ToolDescription("Get weather by city")]
    public string GetWeather(string city)
    {
        return $"The current weather in {city} is sunny, 25°C.";
    }
}
```

### 6.2 Registration Mechanism

The system scans public instance methods in assemblies under the runtime directory, identifies methods decorated with `[AITool]`, and registers them into `IAIToolProvider`.

### 6.3 `IAIToolProvider` API surface (complete)

- `GetTool(string name, string[]? auth = null)`
- `GetTools()`
- `GetToolsByNames(params string[] names)`
- `GetToolsByAuth(string[]? auth = null)`
- `GetToolsByNamesAndAuth(string[]? auth = null, params string[] names)`

Example:

```csharp
// 1) All tools
var allTools = _toolProvider.GetTools();

// 2) Filter by names (commonly used in routing scenarios)
var namedTools = _toolProvider.GetToolsByNames("get_weather", "get_workflow_test");

// 3) Filter by authorization
var authTools = _toolProvider.GetToolsByAuth(new[] { "order.read", "order.*" });

// 4) Combined filter by names and authorization
var finalTools = _toolProvider.GetToolsByNamesAndAuth(
    auth: new[] { "order.read" },
    names: new[] { "get_order", "cancel_order" });
```

### 6.4 Permission Wildcard Rules (GetToolsByAuth / GetToolsByNamesAndAuth)

The Tool permission matching rules are as follows:

1. **Tool without configured permissions**: Access is allowed by default.
2. **Tool with permissions configured, but user auth is empty**: Access is denied.
3. **No wildcard**: Case-insensitive exact match (e.g., order.read only matches order.read).
4. **Global wildcard `*`**: Matches any permission.
5. **Segmented wildcard: Matches segment by segment using `.` as the delimiter; `*` matches only a single segment.
   - `order.*` matches `order.read`, `order.write`
   - `order.*` does NOT match `order.center.read` (different number of segments)
   - `*.read` matches `order.read`, `user.read`
6. **Any match grants access: If any item in the user's permission set matches any required permission of the Tool, access is allowed.

---

## 7. API Usage Examples

### 7.1 Multi-turn conversation (with context)

```csharp
var answer = await agentClient.ChatRunAsync(sessionId, agent, userInput);
```

### 7.2 Single-turn call (no context)

```csharp
var answer = await agentClient.ChatRunAsync(agent, "hello");
```

### 7.3 Single-turn call with `ChatMessage` / collection

```csharp
using Microsoft.Extensions.AI;

var msg = new ChatMessage(ChatRole.User, "Please summarize this content.");
var answer1 = await agentClient.ChatRunAsync(agent, msg);

var messages = new List<ChatMessage>
{
    new(ChatRole.System, "You are a technical documentation assistant."),
    new(ChatRole.User, "Explain this API.")
};
var answer2 = await agentClient.ChatRunAsync(agent, messages);
```

### 7.4 Return raw `AgentResponse`

```csharp
var response1 = await agentClient.ChatRunAgentResponseAsync(agent, "hello");
var response2 = await agentClient.ChatRunAgentResponseAsync(agent, new ChatMessage(ChatRole.User, "hello"));
var response3 = await agentClient.ChatRunAgentResponseAsync(agent, messages);
```

### 7.5 Clear context

```csharp
agentClient.ClearChatContext(sessionId);
```

### 7.6 Create named/unnamed agents

```csharp
var namedAgent = agentClient.CreateAgent("planner", "You are a planning assistant.", tools);
var defaultAgent = agentClient.CreateAgent("You are a general assistant.", tools);
```

### 7.7 Inject tools selected by route

```csharp
var tools = _toolProvider.GetToolsByNames(agentRouteDecision!.ToolName!);
```

### 7.8 `CreateAgent` overloads

```csharp
public AIAgent CreateAgent(string agentName, string instructions, IList<AITool>? tools = null);
public AIAgent CreateAgent(string instructions, IList<AITool>? tools = null);
```

Description:
- The first overload is designed for multi-Agent collaboration and observability scenarios (supports explicit configuration of `agentName`).
- The second overload is intended for simple scenarios (only requires system prompts and tools as input).
- Both overloads internally read `ApiKey`, `BaseUrl`, and `Model`, then instantiate an executable `AIAgent` with tool capabilities.
- When `ApiKey` is not configured, it will be retrieved from the environment variable specified by `EnvName` (default: `EASYCORE_AGENT_API_KEY`).

### 7.9 New overloads for `ChatRunAgentResponseAsync` / `ChatRunAsync`

```csharp
public Task<AgentResponse> ChatRunAgentResponseAsync(AIAgent agent, string message, CancellationToken cancellationToken = default);
public Task<AgentResponse> ChatRunAgentResponseAsync(AIAgent agent, ChatMessage message, CancellationToken cancellationToken = default);
public Task<AgentResponse> ChatRunAgentResponseAsync(AIAgent agent, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);

public Task<string> ChatRunAsync(AIAgent agent, string message, CancellationToken cancellationToken = default);
public Task<string> ChatRunAsync(AIAgent agent, ChatMessage message, CancellationToken cancellationToken = default);
public Task<string> ChatRunAsync(AIAgent agent, IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default);
```
Description:

- `ChatRunAgentResponseAsync` returns the raw `AgentResponse`, suitable for advanced scenarios that require accessing more response details.
- `ChatRunAsync` returns `response.Text`, ideal for common scenarios where only the text result is needed.
- The three sets of input parameters respectively support `string`, single `ChatMessage`, and multiple `ChatMessage` (`IEnumerable`).

---

## 8. Best Practices

- ✅ Prefer Redis in production to keep context consistent across multiple instances.
- ✅ Use a gateway or middleware to inject `sessionId` uniformly.
- ✅ Validate Tool input parameters at the business layer to avoid high-risk calls.
- ✅ Record request duration and tool invocation logs for troubleshooting and optimization.

---

## 9. FAQ

### ❓ Q1: Why do I get the error `ApiKey/BaseUrl/Model is not configured`?

Make sure the configuration values are not empty and do not contain invisible characters, such as full-width spaces or line breaks.

> If `ApiKey` is not explicitly configured, the SDK reads it from the environment variable specified by `EnvName`.

### ❓ Q2: Why is my tool not called?

Please check:

1. Whether the method is a `public` instance method;
2. Whether `[AITool("tool_name")]` is added;
3. Whether the assembly containing the tool is scanned.

### ❓ Q3: Why is the conversation context lost?

- Memory mode is only valid inside the current process;
- Use Redis if you need persistence or context sharing across multiple instances.

---

## 10. Detailed Introduction to EasyCore.Agent

### 10.1 Design Goals

The core goal of `EasyCore.Agent` is to let you integrate Agent capabilities into ASP.NET Core and backend services with less boilerplate code. It focuses on solving three problems:

1. **Context memory**: Persist message history by conversation to avoid repeatedly assembling context in the business layer;
2. **Tool Calling**: Automatically discover tool methods through attributes and reduce manual registration cost;
3. **Unified model integration**: Encapsulate OpenAI-compatible APIs uniformly, making it easier to switch models and BaseUrl.

### 10.2 Core Capabilities

- **Context storage abstraction**: Supports both `Memory` and `Redis` context storage strategies, switchable by environment.
- **Agent client base class**: Quickly derive business Agent clients from `BasicAgentClient<TOptions>`.
- **Tool scanning and registration**: Scan public instance methods in assemblies and automatically expose methods decorated with `[AITool]` as callable tools.
- **Unified conversation entry point**: Use `ChatRunAsync` for both single-turn and multi-turn calls, and clear context by `sessionId`.

### 10.3 Recommended Business Implementation

1. Register `EasyCore.Agent` with DI;
2. Inherit from `BasicAgentClient<TOptions>` to define your model client;
3. Encapsulate business capabilities as Tools, such as checking weather, querying orders, or triggering workflows;
4. Keep Controllers and Application Services focused on orchestration instead of coupling them directly to model SDKs;
5. Inject `sessionId` uniformly for every request to make context traceable.

---

## 11. Detailed Introduction to EasyCore.Agent.Workflow

`EasyCore.Agent.Workflow` is a **workflow orchestration layer** built on top of `EasyCore.Agent`. If `EasyCore.Agent` answers “how to use a single Agent,” then Workflow answers “how multiple steps or multiple Agents collaborate in a process.”

### 11.1 Use Cases

- A user request needs to go through **intent recognition → branch handling → summarized output**;
- Some nodes need to be executed **in parallel**, such as generating a Controller and DTO at the same time;
- Workflow traces need to be preserved for debugging, auditing, and performance analysis.

### 11.2 Workflow Explanation Based on the AspCoreAgent Demo

In `WorkflowService.RunAsync` of `AspCoreAgent`, the workflow is organized as follows:

1. **Step1**: Perform intent recognition first and write `intent`;
2. **Branch**: Enter different branches based on `intent`:
   - `intent == 1`: Code generation branch: Step2 → Step3/Step4 in parallel → Step5;
   - `intent == 2`: SQL generation branch: Step6;
   - Other values: Normal chat branch: Step7;
3. **Step8**: No matter which branch is executed, the final output is summarized uniformly.

#### 11.2.1 Flow Diagram (Mermaid)

```csharp
/// <summary>
/// Multi-Agent collaboration workflow execution demo
/// </summary>
/// <param name="input"></param>
/// <param name="cancellationToken"></param>
/// <returns></returns>
[AITool("get_workflow_test")]
[ToolDescription("Execute workflow")]
public async Task<string?> RunAsync([ToolDescription("Input a natural integer, for example: 1 or 2. The valid range is 1 to 2.")] string input, CancellationToken cancellationToken = default)
{
    var workflow = AgentWorkflow.Create()
        // Step1: Intent recognition Agent
        .AddFunc(Step1Async)

        // Select different flows based on intent
        .AddBranch(branch => branch

            // intent == 1: Code generation flow
            .If(ctx => ctx.Get<string>("intent") == "1", flow => flow
                // Step2: Plan Agent
                .AddFunc(Step2Async)

                // Step3 / Step4 execute in parallel
                .AddParallel(parallel => parallel
                    // Step3: Controller generation Agent
                    .AddFunc(Step3Async)

                    // Step4: DTO generation Agent
                    .AddFunc(Step4Async))

                // Step5: Merge Agent
                .AddFunc(Step5Async))

            // intent == 2: SQL generation flow
            .ElseIf(ctx => ctx.Get<string>("intent") == "2", flow => flow
                // Step6: SQL Agent
                .AddFunc(Step6Async))

            // Fallback flow
            .Else(flow => flow
                // Step7: Normal chat Agent
                .AddFunc(Step7Async)))

        // Step8: Final summary Agent
        .AddFunc(Step8Async);

    var context = new AgentWorkflowContext
    {
        Input = input
    };

    await _workflowRunner.RunAsync(workflow, context, cancellationToken);

    return context.Output;
}

/// <summary>
/// Step1: Intent recognition Agent
/// 
/// Purpose:
/// Determine which branch to execute based on user input.
/// 
/// Input:
/// context.Input
/// 
/// Output:
/// context.Items["intent"]
/// </summary>
private async Task Step1Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    var input = context.Input?.Trim();

    // Simulate the result of IntentAgent
    // In a real project, this can call DeepSeekAgent / OpenAI Agent
    if (input == "1")
    {
        context.Set("intent", "1");
        context.Set("intent_description", "Code generation flow");
    }
    else if (input == "2")
    {
        context.Set("intent", "2");
        context.Set("intent_description", "SQL generation flow");
    }
    else
    {
        context.Set("intent", "other");
        context.Set("intent_description", "Normal chat flow");
    }

    Console.WriteLine($"step1--Intent recognition result: {context.Get<string>("intent_description")}");

    await Task.CompletedTask;
}

/// <summary>
/// Step2: Plan Agent
/// 
/// Purpose:
/// Generate a code generation plan based on user input.
/// 
/// Input:
/// context.Input
/// 
/// Output:
/// context.Items["plan"]
/// context.Next(plan)
/// </summary>
private async Task Step2Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    // Simulate PlanAgent
    var plan = $"""
        [PlanAgent Output]
      
        User input:
        {context.Input}
      
        Code generation plan:
        1. Generate ProductController
        2. Generate ProductDto
        3. Finally merge Controller and DTO
        """;

    context.Set("plan", plan);

    // Next means: pass the current output as the input to the next step
    context.Next(plan);

    Console.WriteLine($"step2--Plan generation result: {plan}");

    await Task.CompletedTask;
}

/// <summary>
/// Step3: Controller generation Agent
/// 
/// Purpose:
/// Generate Controller based on the plan from Step2.
/// 
/// Note:
/// This is a parallel node. Do not call context.Next.
/// Parallel nodes should only write their own results into Items.
/// 
/// Input:
/// context.Items["plan"]
/// 
/// Output:
/// context.Items["controller"]
/// </summary>
private async Task Step3Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    var plan = context.Get<string>("plan");

    // Simulate ControllerAgent
    var controller = $$"""
       [ControllerAgent Output]
       
       Generate Controller based on the plan:
       
       {{plan}}
       
       public sealed class ProductController : ControllerBase
       {
           [HttpGet("{id}")]
           public IActionResult Get(Guid id)
           {
               return Ok(new ProductDto
               {
                   Id = id,
                   Name = "Test Product"
               });
           }
       }
       """;

    context.Set("controller", controller);

    Console.WriteLine($"step3--Controller generation result: {controller}");

    await Task.CompletedTask;
}

/// <summary>
/// Step4: DTO generation Agent
/// 
/// Purpose:
/// Generate DTO based on the plan from Step2.
/// 
/// Note:
/// This is a parallel node. Do not call context.Next.
/// Parallel nodes should only write their own results into Items.
/// 
/// Input:
/// context.Items["plan"]
/// 
/// Output:
/// context.Items["dto"]
/// </summary>
private async Task Step4Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    var plan = context.Get<string>("plan");

    // Simulate DtoAgent
    var dto = $$"""
       [DtoAgent Output]
       
       Generate DTO based on the plan:
       
       {{plan}}
       
       public sealed class ProductDto
       {
           public Guid Id { get; set; }
       
           public string Name { get; set; } = string.Empty;
       }
       """;

    context.Set("dto", dto);

    Console.WriteLine($"step4--DTO generation result: {dto}");

    await Task.CompletedTask;
}

/// <summary>
/// Step5: Merge Agent
/// 
/// Purpose:
/// After Step3 and Step4 are both completed, read the parallel results and merge them.
/// 
/// Input:
/// context.Items["controller"]
/// context.Items["dto"]
/// 
/// Output:
/// context.Next(result)
/// </summary>
private async Task Step5Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    var controller = context.Get<string>("controller");
    var dto = context.Get<string>("dto");

    // Simulate MergeAgent
    var result = $"""
     [MergeAgent Output]
  
     ===== Controller =====
  
     {controller}
  
     ===== DTO =====
  
     {dto}
  
     Merge note:
     Controller and DTO have been generated.
     """;

    // Pass the merged result to Step8
    context.Next(result);

    Console.WriteLine($"step5--Merge result: {result}");

    await Task.CompletedTask;
}

/// <summary>
/// Step6: SQL generation Agent
/// 
/// Purpose:
/// Execute the SQL branch when intent == 2.
/// 
/// Input:
/// context.Input
/// 
/// Output:
/// context.Next(sqlResult)
/// </summary>
private async Task Step6Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    // Simulate SqlAgent
    var sqlResult = $"""
       [SqlAgent Output]
    
       User input:
       {context.Input}
    
       Generated SQL:
    
       SELECT *
       FROM Products
       WHERE IsDeleted = 0
       ORDER BY CreateTime DESC;
       """;

    context.Set("sql_result", sqlResult);

    context.Next(sqlResult);

    Console.WriteLine($"step6--SQL generation result: {sqlResult}");

    await Task.CompletedTask;
}

/// <summary>
/// Step7: Normal chat Agent
/// 
/// Purpose:
/// Execute the normal chat branch when intent is neither 1 nor 2.
/// 
/// Input:
/// context.Input
/// 
/// Output:
/// context.Next(answer)
/// </summary>
private async Task Step7Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    // Simulate ChatAgent
    var answer = $"""
       [ChatAgent Output]
    
       Your input is:
       {context.Input}
    
       No code generation flow or SQL generation flow was matched, so the normal chat flow is used.
       """;

    context.Next(answer);

    Console.WriteLine($"step7--Normal chat result: {answer}");

    await Task.CompletedTask;
}

/// <summary>
/// Step8: Final summary Agent
/// 
/// Purpose:
/// After all branches are completed, produce the final output uniformly.
/// 
/// Input:
/// context.Output
/// 
/// Output:
/// context.Output
/// </summary>
private async Task Step8Async(AgentWorkflowContext context, CancellationToken cancellationToken)
{
    var intent = context.Get<string>("intent");
    var intentDescription = context.Get<string>("intent_description");

    // Simulate SummaryAgent
    context.Output = $"""
       [SummaryAgent Output]
     
       Workflow execution completed.
     
       Intent:
       {intent}
     
       Intent description:
       {intentDescription}
     
       Final result:
     
       {context.Output}
       """;

    Console.WriteLine($"step8--Final summary result: {context.Output}");

    await Task.CompletedTask;
}
```

![Workflow Diagram](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/workflow-en.svg)

#### 11.2.2 Execution Notes

- **Pre-branch decision**: Step1 is only responsible for routing and does not directly produce the final answer.
- **Parallel node constraint**: Parallel nodes, such as Step3 and Step4, should only write to `Items` and should not overwrite each other's `Output`.
- **Merge node responsibility**: Step5 summarizes the parallel results and then calls `Next` to pass the result downstream.
- **Unified finalization**: Step8 wraps outputs from different branches into a unified structure, making frontend display easier.

### 11.3 EasyCore.Agent.Workflow API Description: Names and Purposes Only

> The following only explains API names and purposes. It is not bound to a specific class, making it easier to understand from a capability perspective.

#### 11.3.1 Workflow Building APIs

- **Create**: Create a new workflow definition instance.
- **AddFunc**: Add a normal step. Supports synchronous and asynchronous delegate overloads.
- **AddBranch**: Add a conditional branch container.
- **AddParallel**: Add a parallel execution container.
- **RunAsync**: Execute the steps in the workflow definition in order.

#### 11.3.2 Branch Orchestration APIs

- **If**: Define the first conditional branch.
- **ElseIf**: Define subsequent conditional branches.
- **Else**: Define the fallback branch.

#### 11.3.3 Parallel Orchestration APIs

- **AddFunc**: Add a parallel step to the parallel container.
- **AddFlow**: Add a sub-flow to the parallel container.
- **AddBranch**: Add a branch sub-flow to the parallel container.

#### 11.3.4 Context and Data Flow APIs

- **Set**: Write step output into the context key-value area.
- **Get**: Read a context value by key.
- **Get<T>**: Read a context value in a type-safe way.
- **Next**: Use the current output as the next step input and refresh `Output`.

#### 11.3.5 Runtime and Integration APIs

- **EasyCoreAgentWorkflow**: Register Workflow runtime capabilities into dependency injection.
- **RunAsync**: Trigger workflow execution through the runner, usually called by an application service.

#### 11.3.6 Trace Data Fields for Observability

- **StepName**: Step name.
- **StepType**: Step type, such as Func / Branch / Parallel.
- **StartTime / EndTime**: Step start and end time.
- **ElapsedMilliseconds**: Step duration in milliseconds.
- **Success**: Whether the step succeeded.
- **ErrorMessage**: Exception message when failed.

---

## 12. Running the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

---

## 📄 License

Please add a License according to your project needs, such as MIT, Apache-2.0, or a private license.
