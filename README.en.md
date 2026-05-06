# 🚀 EasyCore.Agent

> **EasyCore.Agent** is a lightweight Agent SDK for .NET, providing conversation context management, automatic Tool Calling registration, and OpenAI-compatible model integration capabilities.  
> 本项目是一个面向 .NET 的轻量级 Agent SDK，支持上下文记忆、Tool Calling 和 OpenAI 兼容模型接入。

<p align="center">
  <img alt="dotnet" src="https://img.shields.io/badge/.NET-8+-512BD4?logo=dotnet" />
  <img alt="csharp" src="https://img.shields.io/badge/C%23-12-239120?logo=csharp" />
  <img alt="ai" src="https://img.shields.io/badge/AI-Agent-blueviolet" />
  <img alt="redis" src="https://img.shields.io/badge/Context-Redis%20%7C%20Memory-red?logo=redis" />
</p>

---

## 🌍 Language

- English (Current Document)
- 中文：[README.md](./README.md)

---

## 📚 Table of Contents

- [1. Introduction](#1-introduction)
- [2. Architecture](#2-architecture)
- [3. Core Features](#3-core-features)
- [4. Quick Start](#4-quick-start)
- [5. Configuration](#5-configuration)
- [6. Tool Development Guide](#6-tool-development-guide)
- [7. API Usage Examples](#7-api-usage-examples)
- [8. Best Practices](#8-best-practices)
- [9. FAQ](#9-faq)
- [10. Run Demo](#10-run-demo)

---

## 1. Introduction

### 🎯 What Problems Does It Solve?

When directly using large model SDKs in business applications, developers often encounter:

- Complicated multi-turn conversation context management;
- High integration costs for Tool registration and function calling;
- Inconvenient switching between different storage modes (Memory / Redis).

**EasyCore.Agent** simplifies these problems through a unified abstraction, allowing you to build production-ready Agent services faster.

---

## 2. Architecture

### 2.1 Component Relationship Diagram

![Component Relationship Diagram](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/architecture-en.svg)

### 2.2 Single Conversation Call Sequence

![Single Conversation Call Sequence](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/png/sequence-en.svg)

---

## 3. Core Features

- 🧠 **Multi-turn Context Memory**: Supports historical message management based on `sessionId`.
- 🧩 **Automatic Tool Calling Registration**: Automatically discovers callable methods through `[AITool]`.
- 🗄️ **Switchable Context Storage**: Supports both `Memory` (development) and `Redis` (production).
- 🔌 **OpenAI-Compatible Integration**: Supports custom `BaseUrl` and `Model` configuration.
- 🧱 **Clear Extension Points**: Built on `BasicAgentClient<TOptions>` for easier business encapsulation.

---

## 4. Quick Start

### 4.1 Install Package

Install `EasyCore.Agent` into your solution via NuGet.

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

### 4.4 Create Agent and Start Chat

```csharp
var tools = toolProvider.GetTools();

var agent = agentClient.CreateAgent(
    agentName: "assistant",
    instructions: "You are a professional assistant",
    tools: tools);

var answer = await agentClient.ChatRunAsync(
    sessionId: "user-001",
    agent: agent,
    message: "Help me check today's weather in Shanghai");
```

---

## 5. Configuration

### 5.1 `AgentClientOptions`

| Field | Description | Example |
|---|---|---|
| `ApiKey` | Model service API key | `sk-xxxx` |
| `BaseUrl` | Model service endpoint | `https://api.openai.com/v1` |
| `Model` | Model name | `gpt-4.1-mini` |

### 5.2 `AgentConfigOptions`

| Field | Description | Recommendation |
|---|---|---|
| `AgentContextStoreType` | Context storage type (Memory/Redis) | Use Memory for local development |
| `MaxContextCount` | Maximum context message count | 20~50 |
| `EndPoints` | Redis endpoint | `127.0.0.1:6379` |
| `Password` | Redis password | Configure per environment |
| `DistributedName` | Redis key prefix | `easycore:agent:` |

---

## 6. Tool Development Guide

### 6.1 Define a Tool Class

```csharp
public class WeatherTool
{
    [AITool("get_weather")]
    [ToolDescription("Get weather information by city")]
    public string GetWeather(string city)
    {
        return $"{city} current weather is sunny, 25℃";
    }
}
```

### 6.2 Registration Mechanism

The system scans public instance methods from assemblies in the runtime directory, identifies methods marked with `[AITool]`, and automatically registers them into `IAIToolProvider`.

---

## 7. API Usage Examples

### 7.1 Multi-turn Conversation (With Context)

```csharp
var answer = await agentClient.ChatRunAsync(sessionId, agent, userInput);
```

### 7.2 Single-turn Call (Without Context)

```csharp
var answer = await agentClient.ChatRunAsync(agent, "hello");
```

### 7.3 Clear Context

```csharp
agentClient.ClearChatContext(sessionId);
```

---

## 8. Best Practices

- ✅ Use Redis in production environments to ensure context consistency across multiple instances.
- ✅ Inject `sessionId` through gateways or middleware whenever possible.
- ✅ Validate Tool input parameters to avoid high-risk calls.
- ✅ Record request duration and Tool calling logs for troubleshooting and optimization.

---

## 9. FAQ

### ❓ Q1: Why do I get `ApiKey/BaseUrl/Model is not configured`?
Please ensure the configuration values are not empty and do not contain invisible characters (such as full-width spaces or line breaks).

### ❓ Q2: Why is my Tool not being called?
Please check:

1. Whether the method is a `public` instance method;
2. Whether `[AITool("tool_name")]` is applied;
3. Whether the assembly containing the Tool is scanned.

### ❓ Q3: Why is the context lost?
- Memory mode only works within the current process;
- Use Redis if you need persistence or multi-instance sharing.

---

## 10. Run Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

---

## 📄 License

Please add the appropriate License for your project requirements (MIT / Apache-2.0 / Private License).
