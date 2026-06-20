# 🚀 EasyCore.Pipeline

> **EasyCore.Pipeline** is a lightweight pipeline orchestration library in the EasyCore.Agent ecosystem. It provides sequential steps, conditional branches, parallel execution, and execution tracing — ideal for multi-Agent workflows, intent routing, and step-by-step task orchestration.  
> A lightweight pipeline orchestration library for .NET with sequential steps, conditional branches, parallel execution, and execution tracing.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Pipeline](https://img.shields.io/badge/Pipeline-Orchestration-blueviolet)
![Agent](https://img.shields.io/badge/EasyCore-Agent-green)

---

## 🌍 Language

- [中文](PipelineREADME.md)
- English (current document)

---

## 📚 Table of Contents

- [1. Introduction](#1-introduction)
- [2. Architecture](#2-architecture)
- [3. Core Features](#3-core-features)
- [4. Requirements](#4-requirements)
- [5. Quick Start](#5-quick-start)
- [6. Core Types](#6-core-types)
- [7. API Examples](#7-api-examples)
- [8. Multi-Agent Workflow Example](#8-multi-agent-workflow-example)
- [9. Data Flow & Context Conventions](#9-data-flow--context-conventions)
- [10. Best Practices](#10-best-practices)
- [11. FAQ](#11-faq)
- [12. EasyCore.Pipeline in Depth](#12-easycorepipeline-in-depth)
- [13. Running the Demo](#13-running-the-demo)

---

## 1. Introduction

### 🎯 What Problem Does It Solve?

In AI Agent applications, a single user request often goes through multiple steps:

- Intent recognition → route to different handlers;
- Some steps run in parallel (e.g. generate Controller and DTO at the same time);
- Steps need to share intermediate results;
- Each step's duration and success/failure should be recorded for debugging and auditing.

Hard-coding with `if/else` and manual `Task.WhenAll` makes flows hard to maintain, extend, and observe.

**EasyCore.Pipeline** offers a fluent API to build pipelines with **Func**, **Branch**, and **Parallel** step types, unified through `PipelineContext` for I/O, shared data, and execution traces.

### 📦 Where It Fits in the Project

```
EasyCore.Agent (Agent SDK / Tool Calling / session context)
    ├── EasyCore.Agent.RAG (RAG retrieval pipeline)
    ├── EasyCore.Pipeline (this doc: workflow orchestration)
    └── EasyCore.Vector.* (vector storage)
```

This library does **not** depend on LLM or Agent runtime. Use it with `EasyCore.Agent` or standalone for any step-based orchestration.

---

## 2. Architecture

### 2.1 Component Diagram

![2-1-component-diagram](docs/svg/2-1-component-diagram-6f2f0d68.svg)


### 2.2 Pipeline Execution Sequence

![2-2-pipeline-execution-sequence](docs/svg/2-2-pipeline-execution-sequence-20db5d37.svg)


### 2.3 Branch + Parallel Flow (Demo)

```text
Step1 Intent recognition
    ↓
AddBranch
    ├── If intent==1 → Step2 Plan → AddParallel(Step3 Controller ∥ Step4 DTO) → Step5 Merge
    ├── ElseIf intent==2 → Step6 SQL generation
    └── Else → Step7 General chat
    ↓
Step8 Final summary
```

---

## 3. Core Features

- 🔗 **Fluent builder API**: `Pipeline.Create().AddFunc(...).AddBranch(...).AddParallel(...)` chainable orchestration.
- 🔀 **Conditional branches**: `If` / `ElseIf` / `Else` — first matching branch runs.
- ⚡ **Parallel execution**: Multiple sub-pipelines inside `AddParallel` run concurrently via `Task.WhenAll`.
- 📦 **Shared context**: `PipelineContext` provides `Input`, `Output`, and `Items` for cross-step data.
- 🔄 **Next data flow**: `context.Next(output)` sets output and passes it as the next step's input.
- 📊 **Execution traces**: Each step records `StepName`, `StepType`, duration, success/failure, and errors.
- 🧩 **Three Func overloads**: `Action`, `Func<Task>`, and `Func<CancellationToken, Task>`.
- 🔌 **Zero-dependency**: No external NuGet packages, no DI registration required.

---

## 4. Requirements

### 4.1 .NET Version

- .NET 8.0 or later

### 4.2 Dependencies

Pure .NET class library — **no third-party NuGet packages**.

### 4.3 Optional Companion Components

| Component | Purpose |
|---|---|
| `EasyCore.Agent` | Call Agents / Tools inside pipeline steps |
| `EasyCore.Agent.RAG` | Orchestrate RAG retrieval in a pipeline |

---

## 5. Quick Start

### 5.1 Reference the Project

```bash
dotnet add reference ../EasyCore.Pipeline/EasyCore.Pipeline.csproj
```

Or install the NuGet package (when published):

```bash
dotnet add package EasyCore.Pipeline
```

### 5.2 Minimal Sequential Pipeline

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

### 5.3 Pipeline with Branches

```csharp
var pipeline = Pipeline.Create()
    .AddFunc(ctx => ctx.Set("type", ctx.Input == "1" ? "code" : "chat"))
    .AddBranch(branch => branch
        .If(ctx => ctx.Get<string>("type") == "code", flow => flow
            .AddFunc(ctx => ctx.Next("Generating code...")))
        .Else(flow => flow
            .AddFunc(ctx => ctx.Next("General chat..."))));

var context = new PipelineContext { Input = "1" };
await PipelineRunner.RunAsync(pipeline, context);
```

### 5.4 Pipeline with Parallel Steps

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

## 6. Core Types

### 6.1 Pipeline

| Method | Description |
|---|---|
| `Create()` | Create a new pipeline instance |
| `AddFunc(Action<PipelineContext>)` | Add a synchronous step |
| `AddFunc(Func<PipelineContext, Task>)` | Add an async step |
| `AddFunc(Func<PipelineContext, CancellationToken, Task>)` | Add a cancellable async step |
| `AddBranch(Action<BranchBuilder>)` | Add a conditional branch step |
| `AddParallel(Action<ParallelBuilder>)` | Add a parallel step |
| `RunAsync(PipelineContext, CancellationToken)` | Run all steps sequentially |

Each step automatically appends to `context.Traces`.

### 6.2 PipelineContext

| Member | Type | Description |
|---|---|---|
| `PipelineId` | `string` | Pipeline instance ID (GUID by default) |
| `ContextId` | `string` | Current execution context ID |
| `SessionId` | `string?` | Optional session ID |
| `Input` | `string` | Current step input |
| `Output` | `string?` | Current pipeline output |
| `Items` | `Dictionary<string, object?>` | Shared data between steps |
| `Traces` | `List<PipelineTrace>` | Execution trace list |

| Method | Description |
|---|---|
| `Set(key, value)` | Write shared data |
| `Get(key)` | Read shared data |
| `Get<T>(key)` | Strongly typed read |
| `Next(output)` | Set `Output` and update `Input` to `output` |

### 6.3 BranchBuilder

| Method | Description |
|---|---|
| `If(condition, configure)` | First conditional branch |
| `ElseIf(condition, configure)` | Subsequent conditional branch |
| `Else(configure)` | Fallback branch (always matches) |

**Execution rule:** Conditions are evaluated top to bottom; the **first** match runs and returns. If none match, the branch step is skipped.

Sets `Items["__current_branch"]` to `"If"` / `"ElseIf"` / `"Else"` when a branch runs.

### 6.4 ParallelBuilder

| Method | Description |
|---|---|
| `AddFunc(...)` | Add a single parallel func (three overloads) |
| `AddFlow(configure)` | Add a sub-pipeline |
| `AddBranch(configure)` | Add a parallel branch sub-pipeline |

**Execution rule:** All sub-pipelines run concurrently via `Task.WhenAll`, sharing the same `PipelineContext`.

### 6.5 PipelineTrace

| Field | Description |
|---|---|
| `StepName` | Step name (func method name or `Branch` / `Parallel`) |
| `StepType` | Step type: `Func` / `Branch` / `Parallel` |
| `StartTime` / `EndTime` | Start / end timestamp |
| `ElapsedMilliseconds` | Duration in milliseconds |
| `Success` | Whether the step succeeded |
| `ErrorMessage` | Exception message on failure |

### 6.6 PipelineRunner

| Method | Description |
|---|---|
| `RunAsync(pipeline, context, cancellationToken)` | Run the specified pipeline |

---

## 7. API Examples

### 7.1 Async Step with CancellationToken

```csharp
var pipeline = Pipeline.Create()
    .AddFunc(async (ctx, ct) =>
    {
        await Task.Delay(500, ct);
        ctx.Set("status", "done");
    });
```

### 7.2 Nested Branches

```csharp
var pipeline = Pipeline.Create()
    .AddBranch(outer => outer
        .If(ctx => ctx.Get<int>("level") > 0, flow => flow
            .AddBranch(inner => inner
                .If(ctx => ctx.Get<int>("level") > 5, f => f.AddFunc(c => c.Set("tier", "high")))
                .Else(f => f.AddFunc(c => c.Set("tier", "low"))))));
```

### 7.3 Sub-flow Inside Parallel

```csharp
var pipeline = Pipeline.Create()
    .AddParallel(parallel => parallel
        .AddFlow(flow => flow
            .AddFunc(ctx => ctx.Set("step1", "a"))
            .AddFunc(ctx => ctx.Set("step2", "b")))
        .AddFunc(ctx => ctx.Set("quick", "c")));
```

### 7.4 Reading Execution Traces

```csharp
await PipelineRunner.RunAsync(pipeline, context);

foreach (var trace in context.Traces)
{
    Console.WriteLine(
        $"[{trace.StepType}] {trace.StepName}: " +
        $"{trace.ElapsedMilliseconds}ms, Success={trace.Success}");
}
```

### 7.5 Wrapping Pipeline as an Agent Tool

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

## 8. Multi-Agent Workflow Example

The `AspCoreAgent` demo's `PipelineTool` shows a full multi-Agent orchestration:

```csharp
var workflow = Pipeline.Create()
    .AddFunc(Step1Async)                    // Intent recognition
    .AddBranch(branch => branch
        .If(ctx => ctx.Get<string>("intent") == "1", flow => flow
            .AddFunc(Step2Async)            // Plan
            .AddParallel(parallel => parallel
                .AddFunc(Step3Async)        // Controller (parallel)
                .AddFunc(Step4Async))       // DTO (parallel)
            .AddFunc(Step5Async))           // Merge
        .ElseIf(ctx => ctx.Get<string>("intent") == "2", flow => flow
            .AddFunc(Step6Async))           // SQL
        .Else(flow => flow
            .AddFunc(Step7Async)))          // General chat
    .AddFunc(Step8Async);                   // Final summary

var context = new PipelineContext { Input = input };
await PipelineRunner.RunAsync(workflow, context, cancellationToken);
return context.Output;
```

**Step responsibilities:**

| Step | Role | Writes to Items | Calls Next |
|---|---|---|---|
| Step1 | Intent recognition | `intent`, `intent_description` | No |
| Step2 | Generate plan | `plan` | Yes |
| Step3 | Generate Controller | `controller` | No (parallel) |
| Step4 | Generate DTO | `dto` | No (parallel) |
| Step5 | Merge results | — | Yes |
| Step6 | SQL generation | `sql_result` | Yes |
| Step7 | General chat | — | Yes |
| Step8 | Final summary | — | Updates `Output` |

---

## 9. Data Flow & Context Conventions

### 9.1 Input / Output / Next

```text
Initial: context.Input = user input

Sequential steps:
  StepA → context.Next("result-A")
  StepB reads context.Input (now "result-A") → context.Next("result-B")

Final: context.Output = last step output
```

### 9.2 Items Shared Store

- Holds structured intermediate results (e.g. `intent`, `plan`, `controller`);
- Branch conditions: `ctx.Get<string>("intent") == "1"`;
- Parallel steps: write to **different keys** to avoid contention;
- Merge steps: read multiple keys then `Next` downstream.

### 9.3 Parallel Step Guidelines

| Rule | Description |
|---|---|
| Do not call `Next` | Parallel nodes only write to `Items`; avoid overwriting `Input`/`Output` |
| Use distinct keys | e.g. `controller`, `dto` — prevent write conflicts |
| Merge after parallel | Use a sequential `AddFunc` to aggregate parallel results and `Next` |
| Shared context | Parallel steps share one `PipelineContext`; `Dictionary` is not thread-safe |

---

## 10. Best Practices

- ✅ **Single responsibility per step**: One concern per `AddFunc` for easier trace debugging.
- ✅ **Route in an early step**: First step handles routing (e.g. intent); don't produce final answers there.
- ✅ **Always merge after parallel**: Aggregate `Items` in a sequential step after `AddParallel`, then `Next`.
- ✅ **Unified final output**: Summarize/format after all branches converge.
- ✅ **Use Traces for observability**: Log `context.Traces` or return to a debug UI.
- ✅ **Pass CancellationToken**: Use `AddFunc(ctx, ct => ...)` for long-running Agent calls.
- ⚠️ **Avoid parallel writes to the same key**: Concurrent writes to one dictionary key are unsafe.
- ⚠️ **Items type casting**: `Get<T>` returns `default` unless the stored type matches exactly; agree on types or cast explicitly.

---

## 11. FAQ

### ❓ Q1: How is Pipeline different from EasyCore.Agent.Workflow?

`EasyCore.Pipeline` is a standalone lightweight orchestration library; the demo uses it directly via `PipelineTool`. If your project wraps it as `EasyCore.Agent.Workflow`, the API style is similar — use whichever your project references.

### ❓ Q2: Do I need DI registration?

No. `Pipeline`, `PipelineRunner`, and `PipelineContext` can be used directly without DI.

### ❓ Q3: What if no branch matches?

The `AddBranch` step is skipped silently; the pipeline continues to the next step.

### ❓ Q4: What happens when a step throws?

The exception propagates and the pipeline stops; that step's trace records `Success = false` and `ErrorMessage`.

### ❓ Q5: What if one parallel sub-step fails?

`Task.WhenAll` propagates the first exception; the entire Parallel step fails.

### ❓ Q6: Can I modify a pipeline after building?

The step list is immutable after build. Rebuild with `Pipeline.Create()` or use a factory for dynamic flows.

### ❓ Q7: How is the Func step name determined?

Trace `StepName` comes from `func.Method.Name`. Anonymous lambdas get compiler-generated names — prefer named methods for debugging.

---

## 12. EasyCore.Pipeline in Depth

### 12.1 Design Goals

1. **Lightweight**: zero external dependencies, small API surface, low learning curve;
2. **Composable**: Func / Branch / Parallel nest freely;
3. **Observable**: built-in traces without extra AOP;
4. **Agent-friendly**: natural fit with `EasyCore.Agent` Tools — one Agent call per step.

### 12.2 Type Structure

```
EasyCore.Pipeline
├── Pipeline.cs           # Pipeline definition & execution
├── PipelineContext.cs    # Runtime context
├── PipelineRunner.cs     # Entry point
├── PipelineTrace.cs      # Step trace
├── BranchBuilder.cs      # Conditional branches
├── BranchItem.cs         # Branch item (internal)
└── ParallelBuilder.cs    # Parallel builder
```

### 12.3 Typical Rollout Steps

1. Reference `EasyCore.Pipeline`;
2. Define step methods (or inline lambdas);
3. Assemble with `Pipeline.Create()` — Func / Branch / Parallel;
4. Create `PipelineContext` and set `Input`;
5. Run via `PipelineRunner.RunAsync`;
6. Read `context.Output` and `context.Traces`;
7. Optional: expose as Agent `[AITool]` for LLM invocation.

---

## 13. Running the Demo

### 13.1 Start the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 13.2 Trigger via Agent Tool

`PipelineTool` registers `[AITool("get_workflow_test")]` — invoke through Agent chat:

- Input `1`: code generation flow (plan → parallel Controller/DTO → merge → summary)
- Input `2`: SQL generation flow
- Other input: general chat flow

All branches finish through Step8 for a unified summary output.

---

## 📄 License

MIT OR Apache-2.0 (same as the EasyCore.Agent main repository)
