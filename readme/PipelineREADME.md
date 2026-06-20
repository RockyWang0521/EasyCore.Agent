# 🚀 EasyCore.Pipeline

> **EasyCore.Pipeline** 是 EasyCore.Agent 生态中的轻量级流程编排库，提供顺序步骤、条件分支、并行执行与执行轨迹（Trace）能力，适用于多 Agent 协同、意图路由、分步任务编排等场景。  
> A lightweight pipeline orchestration library for .NET with sequential steps, conditional branches, parallel execution, and execution tracing.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Pipeline](https://img.shields.io/badge/Pipeline-Orchestration-blueviolet)
![Agent](https://img.shields.io/badge/EasyCore-Agent-green)

---

## 🌍 Language

- 中文（当前文档）
- English: [PipelineREADME.us.md](PipelineREADME.us.md)

---

## 📚 目录

- [1. 项目简介](#1-项目简介)
- [2. 架构图](#2-架构图)
- [3. 核心特性](#3-核心特性)
- [4. 环境要求](#4-环境要求)
- [5. 快速开始](#5-快速开始)
- [6. 核心类型说明](#6-核心类型说明)
- [7. API 使用示例](#7-api-使用示例)
- [8. 多 Agent 协同示例](#8-多-agent-协同示例)
- [9. 数据流与上下文约定](#9-数据流与上下文约定)
- [10. 最佳实践](#10-最佳实践)
- [11. FAQ](#11-faq)
- [12. EasyCore.Pipeline 详细介绍](#12-easycorepipeline-详细介绍)
- [13. Demo 运行](#13-demo-运行)

---

## 1. 项目简介

### 🎯 解决什么问题？

在 AI Agent 应用中，一个用户请求往往需要经过多个步骤：

- 意图识别 → 按分支走不同处理逻辑；
- 某些步骤需要并行执行（如同时生成 Controller 与 DTO）；
- 各步骤之间需要共享中间结果；
- 需要记录每步耗时与成功/失败状态，便于调试与审计。

若在业务代码中用 `if/else` 和手动 `Task.WhenAll` 硬编码，流程难以维护、扩展和观测。

**EasyCore.Pipeline** 提供流式 API 构建 Pipeline，支持 **Func / Branch / Parallel** 三种步骤类型，并通过 `PipelineContext` 统一管理输入输出、共享数据与执行轨迹。

### 📦 在项目中的位置

```
EasyCore.Agent（Agent SDK / Tool Calling / 会话上下文）
    ├── EasyCore.Agent.RAG（RAG 检索链路）
    ├── EasyCore.Pipeline（本文档：流程编排）
    └── EasyCore.Vector.*（向量存储）
```

本库**不依赖** LLM 或 Agent 运行时，可与 `EasyCore.Agent` 组合使用，也可独立用于任意分步任务编排。

---

## 2. 架构图

### 2.1 组件关系图

![2-1-组件关系图](docs/svg/2-1-组件关系图-405637b9.svg)


### 2.2 一次 Pipeline 执行时序

![2-2-一次-pipeline-执行时序](docs/svg/2-2-一次-pipeline-执行时序-5789ca41.svg)


### 2.3 分支 + 并行流程图（Demo 场景）

```text
Step1 意图识别
    ↓
AddBranch
    ├── If intent==1 → Step2 计划 → AddParallel(Step3 Controller ∥ Step4 DTO) → Step5 合并
    ├── ElseIf intent==2 → Step6 SQL 生成
    └── Else → Step7 普通聊天
    ↓
Step8 最终总结
```

---

## 3. 核心特性

- 🔗 **流式构建 API**：`Pipeline.Create().AddFunc(...).AddBranch(...).AddParallel(...)` 链式编排。
- 🔀 **条件分支**：`If` / `ElseIf` / `Else`，按顺序匹配首个满足条件的分支执行。
- ⚡ **并行执行**：`AddParallel` 内多子流程通过 `Task.WhenAll` 并发运行。
- 📦 **共享上下文**：`PipelineContext` 提供 `Input`、`Output`、`Items` 在步骤间传递数据。
- 🔄 **Next 数据流**：`context.Next(output)` 将当前输出设为下一步输入。
- 📊 **执行轨迹**：每步自动记录 `StepName`、`StepType`、耗时、成功/失败与错误信息。
- 🧩 **三种 Func 重载**：支持 `Action`、`Func<Task>`、`Func<CancellationToken, Task>`。
- 🔌 **零依赖接入**：无 NuGet 外部依赖，无 DI 注册，引用程序集即可使用。

---

## 4. 环境要求

### 4.1 .NET 版本

- .NET 8.0 及以上

### 4.2 依赖

本库为**纯 .NET 类库**，不引用第三方 NuGet 包。

### 4.3 可选配合组件

| 组件 | 用途 |
|---|---|
| `EasyCore.Agent` | 在 Pipeline 步骤中调用 Agent / Tool |
| `EasyCore.Agent.RAG` | 在 Pipeline 中编排 RAG 检索链路 |

---

## 5. 快速开始

### 5.1 引用项目

```bash
dotnet add reference ../EasyCore.Pipeline/EasyCore.Pipeline.csproj
```

或安装 NuGet 包（若已发布）：

```bash
dotnet add package EasyCore.Pipeline
```

### 5.2 最简顺序流程

```csharp
using EasyCore.Pipeline;

var pipeline = Pipeline.Create()
    .AddFunc(ctx =>
    {
        ctx.Set("greeting", $"Hello, {ctx.Input}!");
    })
    .AddFunc(ctx =>
    {
        ctx.Next(ctx.Get<string>("greeting")!);
    });

var context = new PipelineContext { Input = "World" };

await PipelineRunner.RunAsync(pipeline, context);

Console.WriteLine(context.Output); // Hello, World!
```

### 5.3 带分支的流程

```csharp
var pipeline = Pipeline.Create()
    .AddFunc(ctx => ctx.Set("type", ctx.Input == "1" ? "code" : "chat"))
    .AddBranch(branch => branch
        .If(ctx => ctx.Get<string>("type") == "code", flow => flow
            .AddFunc(ctx => ctx.Next("生成代码...")))
        .Else(flow => flow
            .AddFunc(ctx => ctx.Next("普通聊天..."))));

var context = new PipelineContext { Input = "1" };
await PipelineRunner.RunAsync(pipeline, context);
```

### 5.4 带并行的流程

```csharp
var pipeline = Pipeline.Create()
    .AddParallel(parallel => parallel
        .AddFunc(async ctx => { ctx.Set("a", "result-A"); await Task.Delay(100); })
        .AddFunc(async ctx => { ctx.Set("b", "result-B"); await Task.Delay(100); }))
    .AddFunc(ctx => ctx.Next($"{ctx.Get<string>("a")} + {ctx.Get<string>("b")}"));

var context = new PipelineContext();
await PipelineRunner.RunAsync(pipeline, context);
Console.WriteLine(context.Output); // result-A + result-B
```

---

## 6. 核心类型说明

### 6.1 Pipeline

| 方法 | 说明 |
|---|---|
| `Create()` | 创建新的 Pipeline 实例 |
| `AddFunc(Action<PipelineContext>)` | 添加同步步骤 |
| `AddFunc(Func<PipelineContext, Task>)` | 添加异步步骤 |
| `AddFunc(Func<PipelineContext, CancellationToken, Task>)` | 添加支持取消的异步步骤 |
| `AddBranch(Action<BranchBuilder>)` | 添加条件分支步骤 |
| `AddParallel(Action<ParallelBuilder>)` | 添加并行步骤 |
| `RunAsync(PipelineContext, CancellationToken)` | 顺序执行所有步骤 |

每个步骤执行时自动写入 `context.Traces`。

### 6.2 PipelineContext

| 成员 | 类型 | 说明 |
|---|---|---|
| `PipelineId` | `string` | Pipeline 实例 ID（默认 GUID） |
| `ContextId` | `string` | 当前执行上下文 ID |
| `SessionId` | `string?` | 可选会话 ID |
| `Input` | `string` | 当前步骤输入 |
| `Output` | `string?` | 当前 Pipeline 输出 |
| `Items` | `Dictionary<string, object?>` | 步骤间共享数据 |
| `Traces` | `List<PipelineTrace>` | 执行轨迹列表 |

| 方法 | 说明 |
|---|---|
| `Set(key, value)` | 写入共享数据 |
| `Get(key)` | 读取共享数据 |
| `Get<T>(key)` | 强类型读取 |
| `Next(output)` | 设置 `Output` 并将 `Input` 更新为 `output` |

### 6.3 BranchBuilder

| 方法 | 说明 |
|---|---|
| `If(condition, configure)` | 第一个条件分支 |
| `ElseIf(condition, configure)` | 后续条件分支 |
| `Else(configure)` | 兜底分支（始终匹配） |

**执行规则：** 从上到下评估条件，**首个**满足条件的分支执行后返回；无匹配分支则跳过分支步骤。

分支执行时会在 `Items["__current_branch"]` 写入 `"If"` / `"ElseIf"` / `"Else"`。

### 6.4 ParallelBuilder

| 方法 | 说明 |
|---|---|
| `AddFunc(...)` | 添加单个并行 Func（三种重载） |
| `AddFlow(configure)` | 添加一段子 Pipeline |
| `AddBranch(configure)` | 添加并行分支子 Pipeline |

**执行规则：** 所有子 Pipeline 通过 `Task.WhenAll` 并发执行，共享同一个 `PipelineContext`。

### 6.5 PipelineTrace

| 字段 | 说明 |
|---|---|
| `StepName` | 步骤名称（Func 方法名或 `Branch` / `Parallel`） |
| `StepType` | 步骤类型：`Func` / `Branch` / `Parallel` |
| `StartTime` / `EndTime` | 开始 / 结束时间 |
| `ElapsedMilliseconds` | 耗时（毫秒） |
| `Success` | 是否成功 |
| `ErrorMessage` | 失败时的异常消息 |

### 6.6 PipelineRunner

| 方法 | 说明 |
|---|---|
| `RunAsync(pipeline, context, cancellationToken)` | 运行指定 Pipeline |

---

## 7. API 使用示例

### 7.1 异步步骤与 CancellationToken

```csharp
var pipeline = Pipeline.Create()
    .AddFunc(async (ctx, ct) =>
    {
        await Task.Delay(500, ct);
        ctx.Set("status", "done");
    });
```

### 7.2 嵌套 Branch

```csharp
var pipeline = Pipeline.Create()
    .AddBranch(outer => outer
        .If(ctx => ctx.Get<int>("level") > 0, flow => flow
            .AddBranch(inner => inner
                .If(ctx => ctx.Get<int>("level") > 5, f => f.AddFunc(c => c.Set("tier", "high")))
                .Else(f => f.AddFunc(c => c.Set("tier", "low"))))));
```

### 7.3 Parallel 中添加子流程

```csharp
var pipeline = Pipeline.Create()
    .AddParallel(parallel => parallel
        .AddFlow(flow => flow
            .AddFunc(ctx => ctx.Set("step1", "a"))
            .AddFunc(ctx => ctx.Set("step2", "b")))
        .AddFunc(ctx => ctx.Set("quick", "c")));
```

### 7.4 读取执行轨迹

```csharp
await PipelineRunner.RunAsync(pipeline, context);

foreach (var trace in context.Traces)
{
    Console.WriteLine(
        $"[{trace.StepType}] {trace.StepName}: " +
        $"{trace.ElapsedMilliseconds}ms, Success={trace.Success}");
}
```

### 7.5 在 Agent Tool 中封装 Pipeline

```csharp
[AITool("run_pipeline")]
public async Task<string?> RunPipelineAsync(string input, CancellationToken ct = default)
{
    var pipeline = BuildMyPipeline();

    var context = new PipelineContext
    {
        Input = input,
        SessionId = "session-001"
    };

    await PipelineRunner.RunAsync(pipeline, context, ct);
    return context.Output;
}
```

---

## 8. 多 Agent 协同示例

`AspCoreAgent` Demo 中的 `PipelineTool` 演示了完整的多 Agent 编排：

```csharp
var workflow = Pipeline.Create()
    .AddFunc(Step1Async)                    // 意图识别
    .AddBranch(branch => branch
        .If(ctx => ctx.Get<string>("intent") == "1", flow => flow
            .AddFunc(Step2Async)            // 计划
            .AddParallel(parallel => parallel
                .AddFunc(Step3Async)        // Controller（并行）
                .AddFunc(Step4Async))       // DTO（并行）
            .AddFunc(Step5Async))           // 合并
        .ElseIf(ctx => ctx.Get<string>("intent") == "2", flow => flow
            .AddFunc(Step6Async))           // SQL
        .Else(flow => flow
            .AddFunc(Step7Async)))          // 普通聊天
    .AddFunc(Step8Async);                   // 最终总结

var context = new PipelineContext { Input = input };
await PipelineRunner.RunAsync(workflow, context, cancellationToken);
return context.Output;
```

**各步骤职责：**

| 步骤 | 作用 | 写入 Items | 是否调用 Next |
|---|---|---|---|
| Step1 | 意图识别 | `intent`, `intent_description` | 否 |
| Step2 | 生成计划 | `plan` | 是 |
| Step3 | 生成 Controller | `controller` | 否（并行节点） |
| Step4 | 生成 DTO | `dto` | 否（并行节点） |
| Step5 | 合并结果 | — | 是 |
| Step6 | SQL 生成 | `sql_result` | 是 |
| Step7 | 普通聊天 | — | 是 |
| Step8 | 最终总结 | — | 更新 `Output` |

---

## 9. 数据流与上下文约定

### 9.1 Input / Output / Next

```text
初始：context.Input = 用户输入

顺序步骤：
  StepA → context.Next("result-A")
  StepB 读取 context.Input（已是 "result-A"）→ context.Next("result-B")

最终：context.Output = 最后一步的输出
```

### 9.2 Items 共享区

- 用于存放结构化中间结果（如 `intent`、`plan`、`controller`）；
- 分支判定：`ctx.Get<string>("intent") == "1"`；
- 并行步骤：各自写入**不同 Key**，避免竞争；
- 合并步骤：读取多个 Key 后 `Next` 给下游。

### 9.3 并行步骤注意事项

| 规则 | 说明 |
|---|---|
| 不要调用 `Next` | 并行节点只写 `Items`，避免覆盖 `Input`/`Output` |
| 使用不同 Key | 如 `controller`、`dto`，防止写入冲突 |
| 合并放在并行之后 | 用顺序 `AddFunc` 读取并行结果并 `Next` |
| 共享 Context | 并行步骤共享同一 `PipelineContext`，非线程安全字典需注意 |

---

## 10. 最佳实践

- ✅ **Step 职责单一**：每个 `AddFunc` 只做一件事，便于 Trace 定位问题。
- ✅ **分支前置判定**：第一个步骤负责路由（如意图识别），不直接产出最终答案。
- ✅ **并行后必须合并**：`AddParallel` 之后用顺序步骤汇总 `Items` 再 `Next`。
- ✅ **统一收口**：所有分支汇合后再做最终总结/格式化输出。
- ✅ **利用 Traces 做可观测性**：将 `context.Traces` 写入日志或返回给前端调试面板。
- ✅ **传递 CancellationToken**：长时间 Agent 调用时使用 `AddFunc(ctx, ct => ...)` 重载。
- ⚠️ **避免并行写同一 Key**：`Dictionary` 非线程安全，并发写同一键可能出错。
- ⚠️ **Items 类型转换**：`Get<T>` 仅在类型完全匹配时返回值，否则返回 `default`；建议统一约定类型或使用显式 cast。

---

## 11. FAQ

### ❓ Q1：Pipeline 与 EasyCore.Agent.Workflow 有什么区别？

`EasyCore.Pipeline` 是独立的轻量编排库，当前 Demo 通过 `PipelineTool` 直接使用。若项目中存在 `EasyCore.Agent.Workflow` 封装，二者 API 风格类似，可按项目实际引用选择。

### ❓ Q2：是否必须注册 DI？

不需要。`Pipeline`、`PipelineRunner`、`PipelineContext` 均可直接 new / 静态调用。

### ❓ Q3：分支都不匹配会怎样？

`AddBranch` 步骤静默跳过，不执行任何子流程，Pipeline 继续下一步。

### ❓ Q4：步骤抛出异常会怎样？

异常向上传播，Pipeline 中断；该步骤的 Trace 记录 `Success = false` 和 `ErrorMessage`。

### ❓ Q5：Parallel 中一个子步骤失败会怎样？

`Task.WhenAll` 会传播第一个异常，整个 Parallel 步骤失败。

### ❓ Q6：能否动态修改已构建的 Pipeline？

`Pipeline` 构建后步骤列表不可变。需要动态流程时请每次 `Pipeline.Create()` 重新构建，或封装 Factory 方法。

### ❓ Q7：Func 步骤名称如何确定？

Trace 中的 `StepName` 取自委托方法名（`func.Method.Name`）。匿名 lambda 会显示编译器生成名，建议用具名方法便于排查。

---

## 12. EasyCore.Pipeline 详细介绍

### 12.1 设计目标

1. **轻量**：零外部依赖，API  surface 小，学习成本低；
2. **可组合**：Func / Branch / Parallel 可任意嵌套；
3. **可观测**：内置 Trace，无需额外 AOP；
4. **Agent 友好**：与 `EasyCore.Agent` Tool 自然结合，一步一个 Agent 调用。

### 12.2 类型结构

```
EasyCore.Pipeline
├── Pipeline.cs           # 流程定义与执行
├── PipelineContext.cs    # 运行时上下文
├── PipelineRunner.cs     # 运行入口
├── PipelineTrace.cs      # 步骤轨迹
├── BranchBuilder.cs      # 条件分支构建
├── BranchItem.cs         # 分支项（internal）
└── ParallelBuilder.cs    # 并行构建
```

### 12.3 典型落地步骤

1. 引用 `EasyCore.Pipeline`；
2. 定义各步骤方法（或 inline lambda）；
3. `Pipeline.Create()` 链式组装 Func / Branch / Parallel；
4. 创建 `PipelineContext`，设置 `Input`；
5. `PipelineRunner.RunAsync` 执行；
6. 读取 `context.Output` 与 `context.Traces`；
7. 可选：封装为 Agent `[AITool]` 供 LLM 调用。

---

## 13. Demo 运行

### 13.1 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 13.2 通过 Agent Tool 触发

`PipelineTool` 注册了 `[AITool("get_workflow_test")]`，可通过 Agent 对话调用：

- 输入 `1`：走代码生成流程（计划 → 并行生成 Controller/DTO → 合并 → 总结）
- 输入 `2`：走 SQL 生成流程
- 其他输入：走普通聊天流程

所有分支最终经 Step8 统一总结输出。

---

## 📄 License

MIT OR Apache-2.0（与 EasyCore.Agent 主仓库保持一致）
