# 🚀 EasyCore.Agent

> **EasyCore.Agent** 是面向 .NET 8+ 的企业级 AI Agent 开发框架。生态包含 Agent SDK、RAG 检索工具、Pipeline 流程编排，以及 Redis / Qdrant / Milvus / PostgreSQL / Elasticsearch 五套向量存储实现，可按需组合构建完整的 RAG 与 Agent 应用。

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![AI Agent](https://img.shields.io/badge/AI-Agent-blueviolet)
![RAG](https://img.shields.io/badge/RAG-Retrieval-green)
![Vector](https://img.shields.io/badge/Vector-5%20Backends-orange)

---

## 🌍 Language

- 中文（当前文档）
- English: [README.en.md](README.en.md)

---

## 📚 目录

### 第一部分：生态与架构
- [1. 生态总览](#1-生态总览)
- [2. 架构与模块关系](#2-架构与模块关系)
- [3. NuGet / 项目清单](#3-nuget--项目清单)
- [4. 向量后端选型对比](#4-向量后端选型对比)

### 第二部分：EasyCore.Agent（Agent SDK）
- [5. Agent SDK 完整指南](#5-easycoreagent-agent-sdk-完整指南)

### 第三部分：EasyCore.Agent.RAG
- [6. RAG 工具库](#6-easycoreagentrag)

### 第四部分：EasyCore.Pipeline
- [7. Pipeline 流程编排](#7-easycorepipeline)

### 第五部分：向量存储
- [8. EasyCore.Vector.Redis](#8-easycorevectorredis)
- [9. EasyCore.Vector.Qdrant](#9-easycorevectorqdrant)
- [10. EasyCore.Vector.Milvus](#10-easycorevectormilvus)
- [11. EasyCore.Vector.PostgreSQL](#11-easycorevectorpostgresql)
- [12. EasyCore.Vector.Elasticsearch](#12-easycorevectorelasticsearch)

### 第六部分：实践与 Demo
- [13. 完整 RAG 技术栈示例](#13-完整-rag-技术栈示例)
- [14. Demo 项目详解](#14-demo-项目详解)
- [15. Tool 开发指南](#15-tool-开发指南)
- [16. 配置参考](#16-配置参考)
- [17. 最佳实践](#17-最佳实践)
- [18. FAQ](#18-faq)
- [19. License](#19-license)

---

## 1. 生态总览

EasyCore.Agent 不是单一 NuGet 包，而是一组**可独立引用、可组合使用**的 .NET 库：

| 层次 | 项目 | 职责 |
|---|---|---|
| **Agent 层** | `EasyCore.Agent` | 多轮会话、Tool Calling、OpenAI 兼容模型接入、Embedding |
| **RAG 层** | `EasyCore.Agent.RAG` | 文档切块、Query Rewrite、Multi Query、MMR 去重 |
| **编排层** | `EasyCore.Pipeline` | 轻量流程编排：顺序 / 分支 / 并行 + Trace |
| **存储层** | `EasyCore.Vector.*` | 向量 Collection 管理、相似度检索、过滤、混合检索 |

### 1.1 典型企业 RAG 数据流

```
文档入库：DocumentChunker → EmbedAsync → VectorStore.UpsertAsync
用户问答：用户提问 → QueryRewrite → MultiQuery → VectorSearch → MmrSelector → Agent 生成答案
Agent 增强：Tool Calling + 会话 Memory + Pipeline 编排
```

### 1.2 设计原则

| 原则 | 说明 |
|---|---|
| **可组合** | 按需引用包；RAG 无需 DI；向量库 API 统一 |
| **OpenAI 兼容** | `BaseUrl` + `Model` 切换 DeepSeek、Qwen、OpenAI 等 |
| **生产级上下文** | Memory（开发）/ Redis（多实例生产） |
| **可观测编排** | Pipeline 内置 `Traces` 记录每步耗时与成败 |
| **存储无关** | 五套向量后端可互换，业务层改动最小 |

### 1.3 解决方案目录结构

```
EasyCore.Agent/
├── src/
│   ├── EasyCore.Agent/EasyCore.Agent/     # Agent SDK
│   ├── EasyCore.Agent.RAG/                # RAG 工具
│   ├── EasyCore.Pipeline/                 # Pipeline 编排
│   └── EasyCore.Vector.*/                 # 向量存储（5 后端）
└── demo/
    ├── Demo.Common/                       # 共享 Agent、Tool、DTO
    ├── Demo.EasyCore.Agent/               # Agent 独立 Demo（5230）
    ├── Demo.EasyCore.Agent.RAG/           # RAG 独立 Demo（5231）
    ├── Demo.EasyCore.Pipeline/            # Pipeline 独立 Demo（5232）
    ├── Demo.EasyCore.Vector.*/            # 各向量库 Demo（5233-5237）
    └── AspCoreAgent/                      # 综合 Demo（5229）
```

---

## 2. 架构与模块关系

### 2.1 组件关系图

![diagram](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/diagram-01-9a7a0347.svg)


### 2.2 RAG 问答时序

![diagram](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/diagram-02-178b751a.svg)


### 2.3 架构 SVG（历史图示）

![architecture-cn](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/architecture-cn.svg)

![sequence-cn](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/sequence-cn.svg)

---

## 3. NuGet / 项目清单

| 项目 | 路径 | 中文文档 | 英文文档 |
|---|---|---|---|
| EasyCore.Agent | `src/EasyCore.Agent/EasyCore.Agent` | 本文档 §5 | [README.en.md](README.en.md) |
| EasyCore.Agent.RAG | `src/EasyCore.Agent.RAG` | [RagREADME.md](readme/RagREADME.md) | [RagREADME.us.md](readme/RagREADME.us.md) |
| EasyCore.Pipeline | `src/EasyCore.Pipeline` | [PipelineREADME.md](readme/PipelineREADME.md) | [PipelineREADME.us.md](readme/PipelineREADME.us.md) |
| EasyCore.Vector.Redis | `src/EasyCore.Vector.Redis` | [RedisREADME.md](readme/RedisREADME.md) | [RedisREADME.us.md](readme/RedisREADME.us.md) |
| EasyCore.Vector.Qdrant | `src/EasyCore.Vector.Qdrant` | [QdrantREADME.md](readme/QdrantREADME.md) | [QdrantREADME.us.md](readme/QdrantREADME.us.md) |
| EasyCore.Vector.Milvus | `src/EasyCore.Vector.Milvus` | [MilvusREADME.md](readme/MilvusREADME.md) | [MilvusREADME.us.md](readme/MilvusREADME.us.md) |
| EasyCore.Vector.PostgreSQL | `src/EasyCore.Vector.PostgreSQL` | [PostgreSQLREADME.md](readme/PostgreSQLREADME.md) | [PostgreSQLREADME.us.md](readme/PostgreSQLREADME.us.md) |
| EasyCore.Vector.Elasticsearch | `src/EasyCore.Vector.Elasticsearch` | [ElasticsearchREADME.md](readme/ElasticsearchREADME.md) | [ElasticsearchREADME.us.md](readme/ElasticsearchREADME.us.md) |

---

## 4. 向量后端选型对比

| 能力 | Redis | Qdrant | Milvus | PostgreSQL | Elasticsearch |
|---|---|---|---|---|---|
| 底层引擎 | Redis Stack + RediSearch | Qdrant gRPC | Milvus 2.x | pgvector | dense_vector + KNN |
| 稠密向量检索 | ✅ | ✅ | ✅ | ✅ | ✅ |
| 稀疏向量检索 | ❌ | ✅ `SparseSearchAsync` | ❌ | ❌ | ❌ |
| 原生 Dense+Sparse 混合 | BM25 候选融合 | ✅ 加权融合 | BM25 候选融合 | BM25 候选融合 | BM25 候选融合 |
| 标量过滤 | ✅ | ✅ | ✅ | ✅ | ✅ |
| 混合检索 Hybrid | ✅ | ✅ | ✅ | ✅ | ✅ |
| MMR / RAG 集成 | 与 RAG 库组合 | 与 RAG 库组合 | 与 RAG 库组合 | 与 RAG 库组合 | 与 RAG 库组合 |
| 典型场景 | 低延迟、已有 Redis | 稀疏+语义双路召回 | 大规模向量 | 已有 PG、事务一致 | 已有 ES、全文+向量 |

> 五套向量库 API 风格统一（`CreateCollectionAsync` / `UpsertAsync` / `VectorSearchAsync` / `HybridSearchAsync`），切换后端时业务层改动最小。

### 4.1 选型决策树

```text
是否已有基础设施？
├── 已有 Redis Stack（含 RediSearch）→ EasyCore.Vector.Redis
├── 已有 PostgreSQL → EasyCore.Vector.PostgreSQL（pgvector）
├── 已有 Elasticsearch → EasyCore.Vector.Elasticsearch
├── 需要稀疏向量 / 原生 Dense+Sparse 混合 → EasyCore.Vector.Qdrant
└── 超大规模专用向量库 → EasyCore.Vector.Milvus
```

---


## 5. EasyCore.Agent（Agent SDK）完整指南

### 5.1 项目定位

**EasyCore.Agent** 是生态的核心 SDK，负责：

- 与 OpenAI 兼容 API 通信（Chat + Embedding）；
- 按 `sessionId` 管理多轮对话上下文；
- 扫描并注册 `[AITool]` 业务工具；
- 提供 `BasicAgentClient<TOptions>` 基类供业务扩展。

### 5.2 环境要求

| 项 | 要求 |
|---|---|
| .NET | 8.0+ |
| LLM API | OpenAI 兼容端点（DeepSeek、Qwen、OpenAI、vLLM 等） |
| Redis | 可选，生产多实例会话推荐 |

### 5.3 快速开始（ASP.NET Core）

**Step 1 — 注册服务：**

```csharp
builder.Services.EasyCoreAgent(options =>
{
    options.AgentContextStoreType = AgentContextStoreType.Memory;
    options.MaxContextCount = 20;
});
```

**Step 2 — 定义 Client Options 与 Agent：**

```csharp
public class DeepSeekClientOptions : AgentClientOptions { }

public class DeepSeekAgent : BasicAgentClient<DeepSeekClientOptions>
{
    public DeepSeekAgent(IOptions<DeepSeekClientOptions> options, IServiceProvider sp)
        : base(options, sp) { }
}
```

**Step 3 — appsettings.json：**

```json
{
  "DeepSeekClientOptions": {
    "ApiKey": "your-api-key",
    "BaseUrl": "https://api.deepseek.com",
    "Model": "deepseek-chat",
    "EmbeddingModel": "text-embedding-v3",
    "EnvName": "EASYCORE_AGENT_API_KEY"
  }
}
```

**Step 4 — 注册 Agent 到 DI：**

```csharp
builder.Services.Configure<DeepSeekClientOptions>(
    builder.Configuration.GetSection(nameof(DeepSeekClientOptions)));
builder.Services.AddSingleton<DeepSeekAgent>();
```

**Step 5 — 对话：**

```csharp
var tools = toolProvider.GetTools();
var agent = agentClient.CreateAgent("assistant", "你是专业助手", tools);
var answer = await agentClient.ChatRunAsync("session-001", agent, "你好");
```

### 5.4 EasyCoreAgent 扩展方法

`EasyCoreAgentExtensions.EasyCoreAgent(IServiceCollection, Action<AgentConfigOptions>?)` 完成：

1. 注册 `AgentConfigOptions` 单例；
2. 按 `AgentContextStoreType` 注册 `IAgentContextStore`（Memory 或 Redis）；
3. Redis 模式下联动 `EasyCoreDistributedCache`；
4. 扫描应用目录下非 `Microsoft.*` / `System.*` 的 DLL，调用 `AddAITools` 注册工具。

### 5.5 AgentConfigOptions 配置详解

| 字段 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `MaxContextCount` | `int` | `20` | 每个 session 保留的最大消息条数，超出从头部删除 |
| `AgentContextStoreType` | 枚举 | `Memory` | `Memory` 或 `Redis` |
| `EndPoints` | `List<string>` | 空 | Redis 地址，如 `127.0.0.1:6379` |
| `ConnectTimeout` | `int` | `10` | Redis 连接超时（毫秒） |
| `SyncTimeout` | `int` | `10` | Redis 同步操作超时（毫秒） |
| `DistributedName` | `string` | `agent:context:` | 分布式缓存键前缀 |
| `User` | `string?` | null | Redis 用户名 |
| `Password` | `string?` | null | Redis 密码 |

**Redis 生产配置示例：**

```csharp
builder.Services.EasyCoreAgent(options =>
{
    options.AgentContextStoreType = AgentContextStoreType.Redis;
    options.MaxContextCount = 50;
    options.EndPoints = new List<string> { "redis.internal:6379" };
    options.Password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");
    options.DistributedName = "myapp:agent:context:";
});
```

### 5.6 AgentClientOptions 配置详解

| 字段 | 说明 |
|---|---|
| `BaseUrl` | API 根地址，如 `https://api.deepseek.com`、`https://dashscope.aliyuncs.com/compatible-mode/v1` |
| `Model` | 对话模型名 |
| `EmbeddingModel` | Embedding 模型名 |
| `ApiKey` | API 密钥；为空时读取 `EnvName` 环境变量 |
| `EnvName` | 环境变量名，默认逻辑下常用 `EASYCORE_AGENT_API_KEY` |

**ApiKey 解析规则：**

- 优先使用 `options.ApiKey`；
- 为空则读 `Environment.GetEnvironmentVariable(EnvName)`；
- 自动去除 `Bearer ` 前缀；
- 拒绝非 ASCII 及控制字符，避免复制粘贴引入不可见字符导致鉴权失败。

### 5.7 BasicAgentClient API 完整参考

#### CreateAgent

| 重载 | 说明 |
|---|---|
| `CreateAgent()` | 默认 Agent，无自定义 instructions |
| `CreateAgent(instructions, tools?)` | 指定系统提示与工具 |
| `CreateAgent(name, instructions, tools?)` | 指定名称、提示与工具 |

#### ChatRunAsync（带 session，多轮）

```csharp
Task<string> ChatRunAsync(
    string sessionId,
    AIAgent agent,
    string message,
    EasyAgentRunOptions? runOptions = null,
    CancellationToken cancellationToken = default)
```

**行为：**

1. 从 `IAgentContextStore` 加载历史消息；
2. 追加 User 消息；
3. 按 `MaxContextCount` 裁剪；
4. 调用 `agent.RunAsync`；
5. 将 Assistant / Tool 消息写回上下文；
6. 再次裁剪并 Save；
7. 返回 `response.Text`。

#### ChatRunAsync（无 session，单轮）

支持 `string`、`ChatMessage`、`IEnumerable<ChatMessage>` 三种入参，不读写上下文存储。

#### ChatRunAgentResponseAsync

与 ChatRunAsync 对应，返回完整 `AgentResponse`（含 Tool Call、多消息等），便于业务解析结构化结果。

#### EmbedAsync / EmbedBatchAsync

```csharp
Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
Task<List<float[]>> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default);
```

#### 上下文管理

```csharp
IList<ChatMessage> GetChatContext(string sessionId);
void ClearChatContext(string sessionId);
```

#### CreateEmbeddingClient

暴露底层 `EmbeddingClient`，供高级场景直接使用 OpenAI SDK。

### 5.8 上下文存储实现

| 实现 | 类型 | 特点 |
|---|---|---|
| `MemoryAgentContextStore` | 进程内 | 重启丢失；单实例开发 |
| `RedisAgentContextStore` | Redis | 多实例共享；持久化 |

两者均实现 `IAgentContextStore`：`Get` / `Save` / `Clear` / `GetMaxContextCount`。

### 5.9 Tool 系统概览

工具通过 `[AITool("name")]` 标记，由 `AIToolProvider` 扫描注册。详见本文档 [§15 Tool 开发指南](#15-tool-开发指南)。

**IAIToolProvider 方法：**

| 方法 | 说明 |
|---|---|
| `GetTools()` | 全部工具 |
| `GetTool(name, auth?)` | 单个工具 + 权限校验 |
| `GetToolsByNames(params names)` | 按名称白名单 |
| `GetToolsByAuth(auth?)` | 按权限过滤 |
| `GetToolsByNamesAndAuth(auth?, params names)` | 名称 + 权限联合 |

权限支持 `*` 全通配与 `order.*` 分段通配（见 `ToolAuthorizeAttribute`）。

### 5.10 多模型并存示例

```csharp
// DeepSeek 对话 + Qwen Embedding
builder.Services.Configure<DeepSeekClientOptions>(config.GetSection("DeepSeekClientOptions"));
builder.Services.Configure<QianwenClientOptions>(config.GetSection("QianwenClientOptions"));
builder.Services.AddSingleton<DeepSeekAgent>();
builder.Services.AddSingleton<QianwenAgent>();

// RAG 入库用 Qwen Embedding，问答用 DeepSeek
var vec = await qianwenAgent.EmbedAsync(chunk.Content);
var answer = await deepSeekAgent.ChatRunAsync(sessionId, agent, prompt);
```

### 5.11 Agent SDK FAQ

| 问题 | 解答 |
|---|---|
| ApiKey 报错 | 检查 config / 环境变量 / 无 BOM 与不可见字符 |
| Tool 未扫描 | 确认 DLL 在运行目录且非 Microsoft/System 前缀 |
| 上下文丢失 | Memory 模式重启即失；生产用 Redis |
| Embedding 维度 | 与向量库 Collection 定义一致 |

---


---

## 6. EasyCore.Agent.RAG

### 6.1 项目简介

### 🎯 解决什么问题？

企业知识库问答（RAG）通常包含多个独立步骤：

- 长文档需要切块后再 Embedding；
- 用户多轮对话中的指代、省略需要 Query Rewrite；
- 单次检索召回不足时需要 Multi Query 扩展；
- 向量 Top-K 结果高度重复时需要 MMR 提升多样性。

若在每个业务项目中重复实现上述逻辑，成本高且难以统一调优。

**EasyCore.Agent.RAG** 将这些能力封装为轻量、无状态的静态工具类，与 `EasyCore.Agent`（Agent / Embedding）和 `EasyCore.Vector.*`（向量存储）解耦，可按需组合。

### 📦 在项目中的位置

```
EasyCore.Agent（Agent SDK / Embedding / 会话上下文）
    └── EasyCore.Agent.RAG（本文档：切块 / Rewrite / Multi Query / MMR）
            └── EasyCore.Vector.*（向量入库与检索）
                    ├── EasyCore.Vector.Redis
                    ├── EasyCore.Vector.Qdrant
                    ├── EasyCore.Vector.Milvus
                    ├── EasyCore.Vector.PostgreSQL
                    └── EasyCore.Vector.Elasticsearch
```

本库**不绑定**具体向量数据库，也不强制 DI 注册；引用 NuGet 包或项目后直接调用静态方法即可。

---

## 6.2 架构图

### 6.2.1 RAG 链路总览

![2-1-rag-链路总览](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-rag-链路总览-7d4e369a.svg)


### 6.2.2 各模块职责

| 模块 | 类型 | 是否依赖 LLM | 说明 |
|---|---|---|---|
| `DocumentChunker` | 静态工具 | 否 | 固定窗口 + 重叠切块 |
| `QueryRewrite` | 静态工具 | 是 | 结合会话历史改写检索 Query |
| `MultiQueryGenerator` | 静态工具 | 是 | 从一个问题生成多条检索 Query |
| `MmrSelector` | 静态工具 | 否 | 在相关性与多样性间做 MMR 平衡 |

### 6.2.3 Query Rewrite 时序

![2-3-query-rewrite-时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-3-query-rewrite-时序-db16607d.svg)


---

## 6.3 核心特性

- 📄 **DocumentChunker**：按字符窗口切块，支持可配置 `chunkSize` 与 `overlapSize`，保留 `StartIndex` / `EndIndex` 便于溯源。
- 🔄 **QueryRewrite**：利用 `AIAgent` 将会话中的模糊问题改写为独立、可检索的 Query；自动检测语言并与用户问题保持一致。
- 🔀 **MultiQueryGenerator**：从一个用户问题生成 N 条不同角度的检索 Query，提升召回覆盖率。
- 🎯 **MmrSelector**：Maximum Marginal Relevance 算法，在保持相关性的同时降低结果重复度。
- 🧩 **Prompt 可扩展**：`QueryRewritePromptBuilder`、`MultiQueryPromptBuilder` 暴露 System / User Prompt 构建方法，便于业务定制。
- ⚡ **同步 / 异步**：`QueryRewrite`、`MultiQueryGenerator` 均提供同步与异步 API。
- 🔌 **零配置接入**：无 ServiceCollection 扩展，引用程序集即可使用。

---

## 6.4 环境要求

### 6.4.1 .NET 版本

- .NET 8.0 及以上

### 6.4.2 NuGet 依赖

| 包 | 用途 |
|---|---|
| `Microsoft.Agents.AI` | `AIAgent`、`ChatMessage` 等 Agent 运行时 |
| `Microsoft.Agents.AI.OpenAI` | OpenAI 兼容模型接入（通过 EasyCore.Agent 间接使用） |

### 6.4.3 配合使用的组件

| 组件 | 用途 |
|---|---|
| `EasyCore.Agent` | 创建 `AIAgent`、Embedding、会话上下文 |
| `EasyCore.Vector.*` | 向量入库与相似度检索 |

---

## 6.5 快速开始

### 6.5.1 安装包

```bash
dotnet add package EasyCore.Agent.RAG
```

### 6.5.2 文档切块

```csharp
using EasyCore.Agent.RAG;

var content = File.ReadAllText("manual.md");

var chunks = DocumentChunker.Chunk(
    content: content,
    documentId: "manual-001",
    chunkSize: 800,
    overlapSize: 100);

foreach (var chunk in chunks)
{
    Console.WriteLine($"[{chunk.Index}] {chunk.StartIndex}-{chunk.EndIndex}: {chunk.Content[..Math.Min(50, chunk.Content.Length)]}...");
}
```

### 6.5.3 Query Rewrite

```csharp
using EasyCore.Agent.RAG;
using Microsoft.Extensions.AI;

// 假设已通过 EasyCore.Agent 创建 agent，并有多轮会话 history
var history = agentClient.GetChatContext(sessionId);

var rewritten = await QueryRewrite.RewriteAsync(
    query: "它支持哪些功能？",
    agent: agent,
    history: history);

// 可能输出："EasyCore.Agent 支持哪些功能？"
```

### 6.5.4 Multi Query

```csharp
var queries = await MultiQueryGenerator.GenerateAsync(
    query: "如何申请年假？",
    agent: agent,
    count: 3);

// 可能输出：
// - 如何申请年假？
// - 年假申请流程是什么？
// - 员工休假制度有哪些规定？
```

### 6.5.5 MMR 去重

```csharp
var candidates = searchResults.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector("contentVector")
}).ToList();

var diversified = MmrSelector.Select(
    candidates: candidates,
    topK: 3,
    lambda: 0.7);
```

---

## 6.6 模块说明

### 6.6.1 DocumentChunker

| 成员 | 说明 |
|---|---|
| `Chunk(content, documentId, chunkSize, overlapSize)` | 将文本切分为 `List<DocumentChunk>` |

**参数约束：**

| 参数 | 默认值 | 约束 |
|---|---|---|
| `chunkSize` | `800` | 必须 > 0 |
| `overlapSize` | `100` | 必须 ≥ 0 且 < `chunkSize` |

**行为说明：**

- 自动归一化换行符（`\r\n` → `\n`）并 Trim；
- 空内容返回空列表；
- 每个 chunk 自动生成唯一 `Id`（GUID N 格式）；
- 空白 chunk 会被跳过。

### 6.6.2 DocumentChunk

| 属性 | 类型 | 说明 |
|---|---|---|
| `Id` | `string` | Chunk 唯一标识 |
| `DocumentId` | `string` | 来源文档 ID |
| `Index` | `int` | 在文档中的序号（从 0 开始） |
| `Content` | `string` | 切块文本 |
| `StartIndex` | `int` | 在原文中的起始字符位置 |
| `EndIndex` | `int` | 在原文中的结束字符位置 |

### 6.6.3 QueryRewrite

| 方法 | 说明 |
|---|---|
| `RewriteAsync(query, agent, history, cancellationToken)` | 异步改写 |
| `Rewrite(query, agent, history)` | 同步改写 |

**降级策略：** 若 LLM 返回空文本，则原样返回用户 `query`。

**Prompt 规则（摘要）：**

1. 检测用户最新问题的语言；
2. 改写为独立、清晰、适合检索的 Query；
3. 保持与原问题相同语言；
4. 不回答问题、不解释、不臆造历史中不存在的信息；
5. 若问题已足够清晰则原样返回；
6. 仅输出纯文本 Query。

### 6.6.4 MultiQueryGenerator

| 方法 | 说明 |
|---|---|
| `GenerateAsync(query, agent, count, cancellationToken)` | 异步生成多条 Query |
| `Generate(query, agent, count)` | 同步生成 |

**输出解析：**

- 按行拆分 LLM 输出；
- 自动去除 `1. `、`1、`、`- ` 等序号前缀；
- 去重（大小写不敏感）；
- 若结果中不包含原问题，则将其插入首位；
- 最终返回不超过 `count` 条。

### 6.6.5 MmrSelector

| 方法 | 说明 |
|---|---|
| `Select(candidates, topK, lambda)` | MMR 选取 Top-K |

**算法：**

```
MMR = λ × relevanceScore − (1 − λ) × maxSimilarity(selected)
```

- `relevanceScore`：向量检索原始 Score；
- `maxSimilarity`：候选与已选集合的最大余弦相似度；
- `lambda`：默认 `0.7`，越大越偏向相关性，越小越偏向多样性。

**过滤规则：** 无向量（`Vector.Length == 0`）的候选会被排除。

### 6.6.6 MmrCandidate

| 属性 | 类型 | 说明 |
|---|---|---|
| `Id` | `string` | 候选 ID |
| `Content` | `string` | 文本内容 |
| `Score` | `float` | 原始相关性分数 |
| `Vector` | `float[]` | 用于多样性计算的向量 |

---

## 6.7 API 使用示例

### 6.7.1 入库：切块 + Embedding + 向量写入

```csharp
using EasyCore.Agent.RAG;
using EasyCore.Vector.Redis;

const string collectionName = "knowledge_base";
const string vectorField = "contentVector";

var chunks = DocumentChunker.Chunk(documentText, documentId, 800, 100);

foreach (var chunk in chunks)
{
    var embedding = await agentClient.EmbedAsync(chunk.Content);

    var record = new RedisTextVector
    {
        Id = chunk.Id,
        DocumentId = chunk.DocumentId,
        Index = chunk.Index,
        StartIndex = chunk.StartIndex,
        EndIndex = chunk.EndIndex,
        Content = chunk.Content
    };

    record.SetVector(vectorField, embedding);
    await vectorStore.UpsertAsync(collectionName, record);
}
```

### 6.7.2 检索：Rewrite → Embed → Search

```csharp
var history = deepSeekAgent.GetChatContext(sessionId);
var standaloneQuery = await QueryRewrite.RewriteAsync(userMessage, agent, history);

var queryVector = await agentClient.EmbedAsync(standaloneQuery);

var results = await vectorStore.VectorSearchAsync<RedisTextVector>(
    collectionName,
    vectorField,
    queryVector,
    new RedisVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.75f
    });
```

### 6.7.3 Multi Query 多路检索

```csharp
var queries = await MultiQueryGenerator.GenerateAsync(userMessage, agent, count: 5);

var merged = new Dictionary<string, RedisVectorSearchResult<RedisTextVector>>();

foreach (var q in queries)
{
    var vector = await agentClient.EmbedAsync(q);
    var hits = await vectorStore.VectorSearchAsync<RedisTextVector>(
        collectionName, vectorField, vector,
        new RedisVectorSearchOptions { Limit = 5 });

    foreach (var hit in hits)
    {
        if (!merged.ContainsKey(hit.Record.Id) || merged[hit.Record.Id].Score < hit.Score)
            merged[hit.Record.Id] = hit;
    }
}

var topResults = merged.Values.OrderByDescending(x => x.Score).Take(10).ToList();
```

### 6.7.4 MMR + Agent 回答

```csharp
var mmrCandidates = topResults.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector(vectorField)
}).ToList();

var contextChunks = MmrSelector.Select(mmrCandidates, topK: 3, lambda: 0.7);

var context = string.Join("\n\n", contextChunks.Select(c => c.Content));

var answer = await agentClient.ChatRunAsync(
    sessionId,
    agent,
    $"参考以下资料回答问题：\n\n{context}\n\n问题：{userMessage}");
```

### 6.7.5 自定义 Prompt（QueryRewrite）

```csharp
// 直接使用 PromptBuilder 构建消息，再自行调用 Agent
var messages = QueryRewritePromptBuilder.Build(query, history);

// 或替换 System Prompt
var customSystem = QueryRewritePromptBuilder.GetSystemPrompt();
// 基于 customSystem 自行组装 messages...
```

### 6.7.6 自定义 Prompt（MultiQuery）

```csharp
var messages = MultiQueryPromptBuilder.Build(query, count: 5);

var systemPrompt = MultiQueryPromptBuilder.BuildSystemPrompt(count: 5);
var userPrompt = MultiQueryPromptBuilder.BuildUserPrompt(query, count: 5);
```

---

## 6.8 完整 RAG 流水线

![8-完整-rag-流水线](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/8-完整-rag-流水线-d3fe635b.svg)


**推荐组合：**

| 场景 | 建议启用的模块 |
|---|---|
| 单轮 FAQ | DocumentChunker + VectorSearch |
| 多轮对话知识库 | + QueryRewrite |
| 召回率不足 | + MultiQueryGenerator |
| 结果重复度高 | + MmrSelector |
| 高精度要求 | + 外部 Reranker（业务自行接入） |

---

## 6.9 最佳实践

- ✅ **`chunkSize` 与 Embedding 模型匹配**：中文建议 500~1000 字符，英文可按 token 估算；`overlapSize` 通常取 `chunkSize` 的 10%~20%。
- ✅ **Rewrite 前先积累会话历史**：通过 `EasyCore.Agent` 的 `GetChatContext(sessionId)` 获取完整 `ChatMessage` 列表。
- ✅ **Multi Query 后做结果合并去重**：按 `Record.Id` 保留最高分，避免重复 chunk 进入上下文。
- ✅ **MMR 需要向量数据**：检索时设置 `IncludeVector = true`，或将向量一并映射到 `MmrCandidate.Vector`。
- ✅ **`lambda` 调参**：知识库重复内容多时可降至 `0.5~0.6`；追求精确匹配时可提高至 `0.8~0.9`。
- ✅ **ScoreThreshold 与 MMR 配合**：先用向量库阈值过滤低分结果，再 MMR 精选。
- ⚠️ **QueryRewrite / MultiQuery 依赖 LLM**：注意 API 成本与延迟，可对简单问题跳过 Rewrite。
- ⚠️ **DocumentChunker 为字符级切块**：不感知 Markdown 标题或段落边界，长文档可考虑先按段落预分割。

---

## 6.10 FAQ

### ❓ Q1：本库是否包含向量存储？

不包含。向量入库与检索请使用 `EasyCore.Vector.Redis`、`EasyCore.Vector.Qdrant` 等配套包。

### ❓ Q2：是否必须注册 DI？

不需要。所有 API 均为静态方法，引用程序集后直接调用。

### ❓ Q3：QueryRewrite 需要什么类型的 Agent？

需要支持 `RunAsync(IEnumerable<ChatMessage>)` 的 `AIAgent`，通常由 `EasyCore.Agent` 的 `CreateAgent(...)` 创建。

### ❓ Q4：Rewrite 返回空或异常怎么办？

`RewriteAsync` 在 LLM 返回空时会降级为原始 `query`；建议在业务层对异常做 try/catch 并同样降级。

### ❓ Q5：MMR 选不出足够条数？

若候选本身不足 `topK`，或大量候选缺少有效向量，返回数量会少于 `topK`。请确保检索阶段返回足够候选且 `IncludeVector = true`。

### ❓ Q6：是否支持 Reranker？

当前版本未内置 Cross-Encoder Reranker。可在 `MmrSelector` 之后自行接入第三方 Rerank 服务。

### ❓ Q7：Multi Query 生成语言不对？

Prompt 已要求「与用户问题同语言」。若模型仍偏离，可修改 `MultiQueryPromptBuilder.BuildSystemPrompt` 或在业务层过滤。

---

## 6.11 EasyCore.Agent.RAG 详细介绍

### 6.11.1 设计目标

`EasyCore.Agent.RAG` 聚焦 **RAG 检索链路中的可复用算法与 Prompt 封装**，而非重复实现 Agent 或向量库能力。设计原则：

1. **轻量无状态**：静态工具类，无全局配置，便于测试与组合；
2. **与存储解耦**：不引用任何 `EasyCore.Vector.*` 程序集；
3. **与 Agent 协作**：Rewrite / MultiQuery 通过标准 `AIAgent` 接口调用 LLM；
4. **企业可扩展**：Prompt Builder 公开，允许业务覆盖 System Prompt。

### 6.11.2 类型一览

```
EasyCore.Agent.RAG
├── DocumentChunker/
│   ├── DocumentChunker          # 文档切块
│   └── DocumentChunk            # 切块模型
├── QueryRewrite/
│   ├── QueryRewrite             # Query 改写
│   └── QueryRewritePromptBuilder
├── MultiQueryGenerator/
│   ├── MultiQueryGenerator      # 多 Query 生成
│   └── MultiQueryPromptBuilder
└── MmrSelector/
    ├── MmrSelector              # MMR 选取
    └── MmrCandidate             # MMR 候选模型
```

### 6.11.3 典型落地步骤

1. 引用 `EasyCore.Agent.RAG` 与目标 `EasyCore.Vector.*`；
2. 注册 `EasyCore.Agent` 与向量库 DI；
3. 入库：`DocumentChunker` → `EmbedAsync` → `UpsertAsync`；
4. 检索：`QueryRewrite`（可选）→ `MultiQueryGenerator`（可选）→ `VectorSearchAsync`；
5. 后处理：`MmrSelector.Select` → 拼接上下文 → `ChatRunAsync` 生成答案。

---

## 6.12 Demo 运行

`AspCoreAgent` Demo 的 `EmbeddingController` 提供了 RAG 相关 API 示例。

### 6.12.1 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 6.12.2 RAG 相关端点

| 端点 | 说明 |
|---|---|
| `GET /api/Embedding/RagDocumentChunker` | 文档切块示例 |
| `GET /api/Embedding/RagQueryRewrite?message=...&sessionId=...` | Query Rewrite（含多轮上下文） |
| `GET /api/Embedding/RagMultiQueryRetrieval?message=...` | Multi Query 生成 |

各向量库 Controller（Redis / Qdrant / Milvus 等）中的 `*MmrSelector` 端点演示了 **向量检索 + MMR** 的组合用法。

---

---

## 7. EasyCore.Pipeline

### 7.1 项目简介

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

## 7.2 架构图

### 7.2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-405637b9.svg)


### 7.2.2 一次 Pipeline 执行时序

![2-2-一次-pipeline-执行时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-一次-pipeline-执行时序-5789ca41.svg)


### 7.2.3 分支 + 并行流程图（Demo 场景）

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

## 7.3 核心特性

- 🔗 **流式构建 API**：`Pipeline.Create().AddFunc(...).AddBranch(...).AddParallel(...)` 链式编排。
- 🔀 **条件分支**：`If` / `ElseIf` / `Else`，按顺序匹配首个满足条件的分支执行。
- ⚡ **并行执行**：`AddParallel` 内多子流程通过 `Task.WhenAll` 并发运行。
- 📦 **共享上下文**：`PipelineContext` 提供 `Input`、`Output`、`Items` 在步骤间传递数据。
- 🔄 **Next 数据流**：`context.Next(output)` 将当前输出设为下一步输入。
- 📊 **执行轨迹**：每步自动记录 `StepName`、`StepType`、耗时、成功/失败与错误信息。
- 🧩 **三种 Func 重载**：支持 `Action`、`Func<Task>`、`Func<CancellationToken, Task>`。
- 🔌 **零依赖接入**：无 NuGet 外部依赖，无 DI 注册，引用程序集即可使用。

---

## 7.4 环境要求

### 7.4.1 .NET 版本

- .NET 8.0 及以上

### 7.4.2 依赖

本库为**纯 .NET 类库**，不引用第三方 NuGet 包。

### 7.4.3 可选配合组件

| 组件 | 用途 |
|---|---|
| `EasyCore.Agent` | 在 Pipeline 步骤中调用 Agent / Tool |
| `EasyCore.Agent.RAG` | 在 Pipeline 中编排 RAG 检索链路 |

---

## 7.5 快速开始

### 7.5.1 安装包

```bash
dotnet add package EasyCore.Pipeline
```

### 7.5.2 最简顺序流程

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

### 7.5.3 带分支的流程

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

### 7.5.4 带并行的流程

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

## 7.6 核心类型说明

### 7.6.1 Pipeline

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

### 7.6.2 PipelineContext

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

### 7.6.3 BranchBuilder

| 方法 | 说明 |
|---|---|
| `If(condition, configure)` | 第一个条件分支 |
| `ElseIf(condition, configure)` | 后续条件分支 |
| `Else(configure)` | 兜底分支（始终匹配） |

**执行规则：** 从上到下评估条件，**首个**满足条件的分支执行后返回；无匹配分支则跳过分支步骤。

分支执行时会在 `Items["__current_branch"]` 写入 `"If"` / `"ElseIf"` / `"Else"`。

### 7.6.4 ParallelBuilder

| 方法 | 说明 |
|---|---|
| `AddFunc(...)` | 添加单个并行 Func（三种重载） |
| `AddFlow(configure)` | 添加一段子 Pipeline |
| `AddBranch(configure)` | 添加并行分支子 Pipeline |

**执行规则：** 所有子 Pipeline 通过 `Task.WhenAll` 并发执行，共享同一个 `PipelineContext`。

### 7.6.5 PipelineTrace

| 字段 | 说明 |
|---|---|
| `StepName` | 步骤名称（Func 方法名或 `Branch` / `Parallel`） |
| `StepType` | 步骤类型：`Func` / `Branch` / `Parallel` |
| `StartTime` / `EndTime` | 开始 / 结束时间 |
| `ElapsedMilliseconds` | 耗时（毫秒） |
| `Success` | 是否成功 |
| `ErrorMessage` | 失败时的异常消息 |

### 7.6.6 PipelineRunner

| 方法 | 说明 |
|---|---|
| `RunAsync(pipeline, context, cancellationToken)` | 运行指定 Pipeline |

---

## 7.7 API 使用示例

### 7.7.1 异步步骤与 CancellationToken

```csharp
var pipeline = Pipeline.Create()
    .AddFunc(async (ctx, ct) =>
    {
        await Task.Delay(500, ct);
        ctx.Set("status", "done");
    });
```

### 7.7.2 嵌套 Branch

```csharp
var pipeline = Pipeline.Create()
    .AddBranch(outer => outer
        .If(ctx => ctx.Get<int>("level") > 0, flow => flow
            .AddBranch(inner => inner
                .If(ctx => ctx.Get<int>("level") > 5, f => f.AddFunc(c => c.Set("tier", "high")))
                .Else(f => f.AddFunc(c => c.Set("tier", "low"))))));
```

### 7.7.3 Parallel 中添加子流程

```csharp
var pipeline = Pipeline.Create()
    .AddParallel(parallel => parallel
        .AddFlow(flow => flow
            .AddFunc(ctx => ctx.Set("step1", "a"))
            .AddFunc(ctx => ctx.Set("step2", "b")))
        .AddFunc(ctx => ctx.Set("quick", "c")));
```

### 7.7.4 读取执行轨迹

```csharp
await PipelineRunner.RunAsync(pipeline, context);

foreach (var trace in context.Traces)
{
    Console.WriteLine(
        $"[{trace.StepType}] {trace.StepName}: " +
        $"{trace.ElapsedMilliseconds}ms, Success={trace.Success}");
}
```

### 7.7.5 在 Agent Tool 中封装 Pipeline

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

## 7.8 多 Agent 协同示例

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

## 7.9 数据流与上下文约定

### 7.9.1 Input / Output / Next

```text
初始：context.Input = 用户输入

顺序步骤：
  StepA → context.Next("result-A")
  StepB 读取 context.Input（已是 "result-A"）→ context.Next("result-B")

最终：context.Output = 最后一步的输出
```

### 7.9.2 Items 共享区

- 用于存放结构化中间结果（如 `intent`、`plan`、`controller`）；
- 分支判定：`ctx.Get<string>("intent") == "1"`；
- 并行步骤：各自写入**不同 Key**，避免竞争；
- 合并步骤：读取多个 Key 后 `Next` 给下游。

### 7.9.3 并行步骤注意事项

| 规则 | 说明 |
|---|---|
| 不要调用 `Next` | 并行节点只写 `Items`，避免覆盖 `Input`/`Output` |
| 使用不同 Key | 如 `controller`、`dto`，防止写入冲突 |
| 合并放在并行之后 | 用顺序 `AddFunc` 读取并行结果并 `Next` |
| 共享 Context | 并行步骤共享同一 `PipelineContext`，非线程安全字典需注意 |

---

## 7.10 最佳实践

- ✅ **Step 职责单一**：每个 `AddFunc` 只做一件事，便于 Trace 定位问题。
- ✅ **分支前置判定**：第一个步骤负责路由（如意图识别），不直接产出最终答案。
- ✅ **并行后必须合并**：`AddParallel` 之后用顺序步骤汇总 `Items` 再 `Next`。
- ✅ **统一收口**：所有分支汇合后再做最终总结/格式化输出。
- ✅ **利用 Traces 做可观测性**：将 `context.Traces` 写入日志或返回给前端调试面板。
- ✅ **传递 CancellationToken**：长时间 Agent 调用时使用 `AddFunc(ctx, ct => ...)` 重载。
- ⚠️ **避免并行写同一 Key**：`Dictionary` 非线程安全，并发写同一键可能出错。
- ⚠️ **Items 类型转换**：`Get<T>` 仅在类型完全匹配时返回值，否则返回 `default`；建议统一约定类型或使用显式 cast。

---

## 7.11 FAQ

### ❓ Q1：Pipeline 适合什么场景？

`EasyCore.Pipeline` 是独立的轻量编排库，适用于单次请求内完成的多步骤流程：意图路由、分支处理、并行生成与 Trace 观测。

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

## 7.12 EasyCore.Pipeline 详细介绍

### 7.12.1 设计目标

1. **轻量**：零外部依赖，API  surface 小，学习成本低；
2. **可组合**：Func / Branch / Parallel 可任意嵌套；
3. **可观测**：内置 Trace，无需额外 AOP；
4. **Agent 友好**：与 `EasyCore.Agent` Tool 自然结合，一步一个 Agent 调用。

### 7.12.2 类型结构

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

### 7.12.3 典型落地步骤

1. 引用 `EasyCore.Pipeline`；
2. 定义各步骤方法（或 inline lambda）；
3. `Pipeline.Create()` 链式组装 Func / Branch / Parallel；
4. 创建 `PipelineContext`，设置 `Input`；
5. `PipelineRunner.RunAsync` 执行；
6. 读取 `context.Output` 与 `context.Traces`；
7. 可选：封装为 Agent `[AITool]` 供 LLM 调用。

---

## 7.13 Demo 运行

### 7.13.1 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 7.13.2 通过 Agent Tool 触发

`PipelineTool` 注册了 `[AITool("get_workflow_test")]`，可通过 Agent 对话调用：

- 输入 `1`：走代码生成流程（计划 → 并行生成 Controller/DTO → 合并 → 总结）
- 输入 `2`：走 SQL 生成流程
- 其他输入：走普通聊天流程

所有分支最终经 Step8 统一总结输出。

---

---

## 8. EasyCore.Vector.Redis

### 8.1 项目简介

### 🎯 解决什么问题？

在构建 RAG（检索增强生成）或语义搜索系统时，通常需要：

- 将文档切块并向量化后持久化存储；
- 按相似度快速召回 Top-K 相关片段；
- 结合业务字段（文档 ID、分块序号、租户 ID 等）做过滤；
- 在关键词检索与向量检索之间做融合（Hybrid Search）；
- 与 ASP.NET Core 依赖注入体系无缝集成。

直接使用 Redis 原生 API 或 RediSearch 命令时，往往需要处理 Index Schema 构建、Hash 序列化、KNN 查询语法、Filter 表达式拼接等细节，接入成本较高。

**EasyCore.Vector.Redis** 通过统一的 `IVectorStore` / `IRedisVectorStore` 抽象，封装上述底层细节，让你用强类型 C# 模型完成向量库的创建、写入、检索与删除。

### 📦 在项目中的位置

```
EasyCore.Agent（Agent SDK）
    └── EasyCore.Agent.RAG（RAG 切块 / MMR / Rerank 等）
            └── EasyCore.Vector.*（向量存储抽象与多后端实现）
                    └── EasyCore.Vector.Redis（本文档）
```

与其他向量后端（Qdrant、Milvus、PostgreSQL、Elasticsearch）保持一致的 API 风格，便于按环境切换存储引擎而无需改动业务代码。

---

## 8.2 架构图

### 8.2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-b4df3e64.svg)


### 8.2.2 一次向量检索时序

![2-2-一次向量检索时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-一次向量检索时序-2124a1eb.svg)


### 8.2.3 存储模型

每个 Collection 在 Redis 中的组织方式：

| 层级 | 命名规则 | 说明 |
|---|---|---|
| Index | `{collectionName}:idx` | RediSearch 索引名 |
| Key 前缀 | `{collectionName}:` | 所有文档 Hash 的统一前缀 |
| 文档 Key | `{collectionName}:{id}` | 单条记录的 Redis Hash Key |

每条记录以 **Redis Hash** 形式存储，内置字段 `Id`、`Content`，以及自定义标量字段与向量字段（二进制 FLOAT32 数组）。

---

## 8.3 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查，删除 Collection 时同步清理 Index 与文档 Key。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 Hash 覆盖写入。
- 🔍 **KNN 向量检索**：基于 RediSearch Dialect 2 的 `[KNN]` 语法，支持 Cosine / L2 / Inner Product 三种距离度量。
- 🧮 **标量过滤**：向量检索与纯标量 Query 均支持 Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🔀 **混合检索（Hybrid Search）**：将向量检索结果与 BM25/关键词候选按权重融合，提升召回质量。
- 🧱 **强类型 Record 映射**：继承 `RedisVectorRecord` 即可自动映射标量字段；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCoreRedis(...)` 扩展方法注册连接、Options 与 `IRedisVectorStore`。

---

## 8.4 环境要求

### 8.4.1 Redis 版本

需要 **Redis Stack**（包含 RediSearch 与 Vector 模块），而非普通 Redis 单机版。

推荐部署方式：

```bash
# Docker 快速启动 Redis Stack
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack:latest
```

### 8.4.2 .NET 版本

- .NET 8.0 及以上

### 8.4.3 NuGet 依赖

| 包 | 用途 |
|---|---|
| `StackExchange.Redis` | Redis 连接与 Hash 操作 |
| `NRedisStack` | RediSearch / Vector 命令封装 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI 扩展 |

---

## 8.5 快速开始

### 8.5.1 安装包

```bash
dotnet add package EasyCore.Vector.Redis
```

### 8.5.2 注册服务

```csharp
using EasyCore.Vector.Redis;

builder.Services.EasyCoreRedis(options =>
{
    options.ConnectionString = "localhost:6379";
    // options.DefaultDatabase = 0; // 可选，指定 DB 索引
});
```

### 8.5.3 定义向量实体

```csharp
using EasyCore.Vector.Redis;

public sealed class RedisTextVector : RedisVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

> `RedisVectorRecord` 已内置 `Id`、`Content`、`Vectors`，子类只需声明业务标量字段。

### 8.5.4 创建 Collection 并写入数据

```csharp
public class KnowledgeService
{
    private readonly IRedisVectorStore _vectorStore;
    private const string CollectionName = "knowledge_base";
    private const string VectorField = "contentVector";

    public KnowledgeService(IRedisVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }

    public async Task EnsureCollectionAsync(CancellationToken cancellationToken = default)
    {
        var definition = new RedisVectorCollectionDefinition
        {
            ScalarFields =
            {
                new RedisScalarFieldDefinition
                {
                    Name = "DocumentId",
                    FieldType = ScalarFieldType.VarChar,
                    MaxLength = 128
                },
                new RedisScalarFieldDefinition
                {
                    Name = "Index",
                    FieldType = ScalarFieldType.Int64
                }
            },
            VectorFields =
            {
                new RedisVectorFieldDefinition
                {
                    Name = VectorField,
                    Dimension = 1024,
                    MetricType = RedisSimilarityMetricType.Cosine,
                    IndexType = RedisVectorIndexType.Hnsw
                }
            }
        };

        await _vectorStore.CreateCollectionAsync(CollectionName, definition, cancellationToken);
    }

    public async Task UpsertAsync(
        RedisTextVector record,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        record.SetVector(VectorField, embedding);
        await _vectorStore.UpsertAsync(CollectionName, record, cancellationToken);
    }
}
```

### 8.5.5 向量检索

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("EasyCore.Agent 支持哪些功能？");

var results = await _vectorStore.VectorSearchAsync<RedisTextVector>(
    collectionName: CollectionName,
    vectorName: VectorField,
    vector: queryEmbedding,
    options: new RedisVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.75f,
        IncludeMetadata = true
    });

foreach (var item in results)
{
    Console.WriteLine($"Score={item.Score:F4}, Content={item.Record.Content}");
}
```

---

## 8.6 配置说明

### 8.6.1 `RedisOptions`

| 字段 | 类型 | 说明 | 示例 |
|---|---|---|---|
| `ConnectionString` | `string` | Redis 连接字符串（必填） | `localhost:6379` |
| `DefaultDatabase` | `int?` | 默认 DB 索引，未设置时使用连接字符串或 `-1` | `0` |

连接字符串支持 StackExchange.Redis 标准格式，例如：

```
localhost:6379
localhost:6379,password=your_password
redis.example.com:6379,ssl=true,abortConnect=false
```

### 8.6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `RedisOptions` | Singleton | 配置快照 |
| `IConnectionMultiplexer` | Singleton | Redis 连接复用 |
| `IRedisVectorStore` | Scoped | 向量存储操作入口 |

---

## 8.7 数据模型与 Collection 设计

### 8.7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `RedisVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors` |
| `RedisVectorCollectionDefinition` | Collection Schema 定义 |
| `RedisVectorFieldDefinition` | 向量字段（维度、度量、索引类型） |
| `RedisScalarFieldDefinition` | 标量字段（类型、是否建索引） |
| `RedisVectorSearchOptions` | 向量检索参数 |
| `RedisVectorFilter` | 过滤条件容器 |
| `RedisVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 8.7.2 内置字段

创建 Collection 时，SDK 会自动追加以下字段，**无需**在业务定义中重复声明：

| 字段 | 类型 | 说明 |
|---|---|---|
| `Id` | `VarChar(128)` | 主键，对应 Redis Hash Key 后缀 |
| `Content` | `VarChar(65535)` | 文本内容，可用于关键词过滤 |

### 8.7.3 向量字段配置

```csharp
new RedisVectorFieldDefinition
{
    Name = "contentVector",           // 向量字段名
    Dimension = 1024,                 // 必须与 Embedding 模型输出维度一致
    MetricType = RedisSimilarityMetricType.Cosine,  // Cosine / L2 / InnerProduct
    IndexType = RedisVectorIndexType.Hnsw,          // Hnsw / Ivfflat
    CreateIndex = true,               // 是否创建向量索引
    Lists = 100                       // IVF 参数（HNSW 场景下保留默认值即可）
}
```

#### 相似度度量说明

| 枚举值 | RediSearch 度量 | Score 转换方式 |
|---|---|---|
| `Cosine` | `COSINE` | `1 - distance`（越大越相似） |
| `L2` | `L2` | `1 / (1 + distance)` |
| `InnerProduct` | `IP` | `-distance` |

### 8.7.4 标量字段类型

| `ScalarFieldType` | RediSearch 映射 |
|---|---|
| `Bool` | Tag Field |
| `String` / `VarChar` / `Json` | Text Field |
| `Int8` ~ `Int64` / `Float` / `Double` | Numeric Field |

### 8.7.5 命名约束

Collection 名与字段名必须符合标识符规则：

```
^[A-Za-z_][A-Za-z0-9_]*$
```

例如：`test_collection`、`DocumentId` ✅；`test-collection`、`123abc` ❌。

---

## 8.8 API 使用示例

以下示例均基于 `IRedisVectorStore`，接口继承关系为：

```
IRedisVectorStore
  └── IVectorStore
        └── IRedisVectorSearch
              └── IRedisHybridSearch
```

### 8.8.1 Collection 管理

```csharp
// 检查 Collection 是否存在
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection（删除 Index + 所有文档 Key）
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.8.2 写入与删除

```csharp
// 单条 Upsert
await _vectorStore.UpsertAsync("test_collection", record);

// 批量 Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// 按 Id 删除
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.8.3 按 Id 获取

```csharp
var record = await _vectorStore.GetAsync<RedisTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.8.4 标量 Query（不含向量相似度）

```csharp
var records = await _vectorStore.QueryAsync<RedisTextVector>(
    collectionName: "test_collection",
    filter: new RedisVectorFilter
    {
        Conditions =
        {
            new RedisVectorFilterCondition
            {
                Field = "DocumentId",
                Operator = RedisVectorFilterOperator.Equal,
                Value = "doc-001"
            }
        }
    },
    limit: 20,
    offset: 0,
    includeMetadata: true);
```

### 8.8.5 向量检索（带 Filter）

```csharp
var options = new RedisVectorSearchOptions
{
    Limit = 10,
    ScoreThreshold = 0.8f,
    MetricType = RedisSimilarityMetricType.Cosine,
    IncludeVector = false,
    IncludeMetadata = true,
    Filter = new RedisVectorFilter
    {
        Conditions =
        {
            new RedisVectorFilterCondition
            {
                Field = "Index",
                Operator = RedisVectorFilterOperator.In,
                Value = new[] { 1, 2, 3 }
            }
        }
    }
};

var results = await _vectorStore.VectorSearchAsync<RedisTextVector>(
    "test_collection",
    "contentVector",
    queryVector,
    options);
```

### 8.8.6 混合检索（Hybrid Search）

Hybrid Search 适用于「语义相似 + 关键词命中」Combined Ranking 场景。BM25 候选可由 `QueryAsync` + `Contains` 等方式获得，再与向量结果融合：

```csharp
// 1) 关键词候选（示例：Content 包含 "RAG"）
var keywordRecords = await _vectorStore.QueryAsync<RedisTextVector>(
    "test_collection",
    new RedisVectorFilter
    {
        Conditions =
        {
            new RedisVectorFilterCondition
            {
                Field = "Content",
                Operator = RedisVectorFilterOperator.Contains,
                Value = "RAG"
            }
        }
    },
    limit: 10,
    includeMetadata: true);

// 2) 构造 BM25 候选分数（生产环境可替换为真实 BM25 分数）
var bm25Results = keywordRecords
    .Select((record, index) => new RedisVectorSearchResult<RedisTextVector>
    {
        Record = record,
        Score = Math.Max(0.1f, 1.0f - index * 0.08f)
    })
    .ToList();

// 3) 混合融合
var hybridResults = await _vectorStore.HybridSearchAsync(
    collectionName: "test_collection",
    vectorName: "contentVector",
    vector: queryVector,
    bm25Results: bm25Results,
    options: new RedisVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f,
    bm25Weight: 0.3f);
```

融合算法会对向量分与 BM25 分分别归一化后加权求和，返回 Top-K 结果。

### 8.8.7 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<RedisTextVector>("test_collection", "contentVector", vector);
```

> 建议在 ASP.NET Core 业务代码中优先使用异步 API，避免阻塞线程池。

---

## 8.9 过滤与检索能力详解

### 8.9.1 支持的 Filter 运算符

| 运算符 | 说明 | 适用字段类型 | 示例 |
|---|---|---|---|
| `Equal` | 等于 | 数值 / 文本 / 布尔 | `DocumentId = "doc-001"` |
| `NotEqual` | 不等于 | 数值 / 文本 / 布尔 | `Index != 0` |
| `GreaterThan` | 大于 | 数值 | `Index > 5` |
| `GreaterThanOrEqual` | 大于等于 | 数值 | `Index >= 1` |
| `LessThan` | 小于 | 数值 | `Index < 10` |
| `LessThanOrEqual` | 小于等于 | 数值 | `Index <= 100` |
| `Contains` | 文本包含 | 文本 | `Content` 包含 `"RAG"` |
| `In` | 多值匹配（OR） | 数值 / 文本 / 布尔 | `Index in (1,2,3)` |

多个 Condition 之间为 **AND** 关系（空格连接）。`In` 运算符内部为 OR。

### 8.9.2 `RedisVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限 |
| `ScoreThreshold` | `null` | 相似度阈值，低于此分数的结果被过滤 |
| `Filter` | `null` | 检索前过滤条件 |
| `MetricType` | `Cosine` | 分数转换使用的度量类型 |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

### 8.9.3 向量检索执行流程

1. 根据 `Filter` 构建 RediSearch 过滤表达式；
2. 拼接 KNN 子句：`(filter)=>[KNN {Limit} @{vectorName} $queryVector AS score]`；
3. 使用 Dialect 2 执行 Search；
4. 将 distance 转换为统一 Score；
5. 应用 `ScoreThreshold` 过滤；
6. 按 Score 降序排序并截取 `Limit` 条。

---

## 8.10 与 EasyCore.Agent.RAG 集成

在 `AspCoreAgent` Demo 中，Redis 向量库与 RAG 切块、Embedding 完整串联：

```csharp
// 1) 文档切块
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) 向量化并写入 Redis
var embeddingClient = _agent.CreateEmbeddingClient();

foreach (var chunk in chunks)
{
    var embedding = await _agent.EmbedAsync(chunk.Content);

    var record = new RedisTextVector
    {
        Id = Guid.NewGuid().ToString("N"),
        DocumentId = chunk.DocumentId,
        Index = chunk.Index,
        StartIndex = chunk.StartIndex,
        EndIndex = chunk.EndIndex,
        Content = chunk.Content
    };

    record.SetVector("contentVector", embedding);
    await _redisVectorStore.UpsertAsync("test_collection", record);
}

// 3) 检索 + MMR 去重（EasyCore.Agent.RAG）
var candidates = await _redisVectorStore.VectorSearchAsync<RedisTextVector>(...);

var mmrCandidates = candidates.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector("contentVector")
}).ToList();

var finalResults = MmrSelector.Select(mmrCandidates, topK: 2, lambda: 0.7);
```

典型 RAG 流水线：

```text
原始文档
  ↓ DocumentChunker 切块
文本 Chunk
  ↓ Embedding 模型
向量 + 元数据
  ↓ UpsertAsync
Redis Vector Store
  ↓ VectorSearchAsync / HybridSearchAsync
召回候选
  ↓ MmrSelector / Reranker（EasyCore.Agent.RAG）
精炼上下文
  ↓ Agent ChatRunAsync
最终回答
```

---

## 8.11 最佳实践

- ✅ **Embedding 维度与 Schema 严格一致**：`RedisVectorFieldDefinition.Dimension` 必须等于模型输出维度，否则写入或检索会失败。
- ✅ **Collection 只创建一次**：`CreateCollectionAsync` 在 Index 已存在时会直接返回，建议在应用启动或首次导入前调用。
- ✅ **生产环境使用 Redis Stack 集群或云托管**：确保 RediSearch Vector 模块可用，并配置持久化（AOF/RDB）。
- ✅ **合理设置 `ScoreThreshold`**：过滤低质量召回，减少 LLM 上下文噪声。
- ✅ **大批量写入使用 `UpsertBatchAsync`**：减少往返次数；超大批量建议自行分批。
- ✅ **Hybrid Search 中 BM25 分数需归一化语义**：SDK 内部会按最大值归一化，但上游 BM25 分数应具有可比性。
- ✅ **敏感数据不要写入 `Content` 明文**：必要时在入库前加密或脱敏。
- ⚠️ **避免频繁 DeleteCollection**：`DeleteCollectionAsync` 会扫描并删除所有 `{collection}:*` Key，大数据量下可能耗时较长。

---

## 8.12 FAQ

### ❓ Q1：`Unknown Index` 或 `no such index` 报错？

说明 Collection 尚未创建或 Index 已被删除。请先调用 `CreateCollectionAsync`，并确认 `collectionName` 与写入/检索时一致。

### ❓ Q2：向量检索无结果或 Score 很低？

请检查：

1. Embedding 模型是否与入库时使用同一模型；
2. `Dimension`、`MetricType` 是否与 Collection 定义一致；
3. 是否设置了过高的 `ScoreThreshold`；
4. `Filter` 条件是否过于严格。

### ❓ Q3：`Invalid identifier` 报错？

Collection 名、字段名必须符合 `^[A-Za-z_][A-Za-z0-9_]*$`，请勿使用连字符或中文。

### ❓ Q4：为什么 `includeVector = true` 时必须传 `vectorName`？

一条记录可能包含多个向量字段，SDK 需要明确读取哪个字段的二进制向量数据。

### ❓ Q5：能否与普通 Redis 客户端共用同一连接？

可以。`EasyCoreRedis` 注册的 `IConnectionMultiplexer` 是 Singleton，可在其他服务中一并注入复用；注意 DB 索引与 Key 前缀隔离。

### ❓ Q6：Ivfflat 与 HNSW 如何选择？

- **HNSW**（默认）：查询延迟低，适合在线检索；
- **Ivfflat**：构建成本与参数调优相对灵活，适合对 recall 与内存有特殊权衡的场景。

---

## 8.13 EasyCore.Vector.Redis 详细介绍

### 8.13.1 设计目标

`EasyCore.Vector.Redis` 的核心目标是：在 .NET 应用中提供**生产可用**的 Redis 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：自动补全 `Id` / `Content` 字段，校验主键与字段重名；
2. **类型映射**：通过反射读写 Hash 字段，支持常见标量类型与枚举；
3. **检索表达**：屏蔽 RediSearch KNN + Filter 语法细节；
4. **可组合性**：向量检索、标量 Query、Hybrid 融合分层接口，便于扩展。

### 8.13.2 接口分层

```
IRedisHybridSearch
  ├── HybridSearchAsync / HybridSearch

IRedisVectorSearch : IRedisHybridSearch
  ├── VectorSearchAsync / VectorSearch

IVectorStore : IRedisVectorSearch
  ├── CreateCollectionAsync / DeleteCollectionAsync / CollectionExistsAsync
  ├── UpsertAsync / UpsertBatchAsync
  ├── GetAsync / QueryAsync / DeleteAsync

IRedisVectorStore : IVectorStore
  └── （标记接口，DI 注入使用）
```

### 8.13.3 典型落地步骤

1. 部署 Redis Stack，配置 `ConnectionString`；
2. 调用 `EasyCoreRedis` 注册 DI；
3. 定义 `RedisVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync` 确保 Index 存在；
5. 文档切块 → Embedding → `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 8.13.4 与其他向量后端对比（选型参考）

| 维度 | Redis | 说明 |
|---|---|---|
| 部署复杂度 | 低 | 若已有 Redis Stack，可直接复用 |
| 向量规模 | 中小型 | 适合百万级以内 Chunk |
| 混合检索 | 支持 | 需自行提供 BM25 候选分数 |
| 事务/多模 | 强 | Hash + Search + Cache 一体 |
| 生态一致性 | 高 | 与 EasyCore 其他 `IVectorStore` 用法一致 |

---

## 8.14 Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 Redis 向量库 API 示例。

### 8.14.1 启动 Redis Stack

```bash
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack:latest
```

### 8.14.2 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 8.14.3 相关 API 端点

| 端点 | 说明 |
|---|---|
| `GET /api/Redis/RedisVectorStoreUpsert` | 创建 Collection 并导入切块向量 |
| `GET /api/Redis/RedisVectorStoreSearch` | 向量检索 + Filter + Score 过滤 |
| `GET /api/Redis/RedisVectorStoreMmrSelector` | 向量检索 + MMR 去重 |
| `GET /api/Redis/RedisVectorStoreGet` | 按 Id 获取记录 |
| `GET /api/Redis/RedisVectorStoreQuery` | 标量 Query |
| `GET /api/Redis/RedisVectorStoreHybridSearch` | 混合检索示例 |
| `GET /api/Redis/RedisVectorStoreDelete` | 删除单条记录 |
| `GET /api/Redis/RedisVectorStoreCollectionExists` | 检查 Collection |
| `GET /api/Redis/RedisVectorStoreDeleteCollection` | 删除整个 Collection |

---

---

## 9. EasyCore.Vector.Qdrant

### 9.1 项目简介

### 🎯 解决什么问题？

在构建 RAG（检索增强生成）或语义搜索系统时，通常需要：

- 将文档切块并向量化后持久化存储；
- 按相似度快速召回 Top-K 相关片段；
- 结合业务字段（文档 ID、分块序号、租户 ID 等）做过滤；
- 在**语义向量检索**与**稀疏向量（关键词/BM25 风格）检索**之间做融合；
- 与 ASP.NET Core 依赖注入体系无缝集成。

直接使用 Qdrant gRPC API 时，往往需要处理 Collection Schema 构建、Named Vector / Sparse Vector 配置、Payload 序列化、Filter 表达式拼接、混合检索权重融合等细节，接入成本较高。

**EasyCore.Vector.Qdrant** 通过统一的 `IQdrantVectorStore` 抽象，封装上述底层细节，让你用强类型 C# 模型完成向量库的创建、写入、检索与删除。

### ⭐ 与其他后端的差异化能力

| 能力 | EasyCore.Vector.Qdrant | EasyCore.Vector.Redis 等 |
|---|---|---|
| 稀疏向量检索 | ✅ `SparseSearchAsync` + `SparseVectorValue` | ❌ |
| 混合检索 | ✅ Dense + Sparse 向量加权融合 | BM25 候选 + 向量分融合 |
| 距离度量 | Collection 创建时指定 `Distance` | 检索时可传 `MetricType` |

> **稀疏向量 + 原生混合检索** 是本库的核心差异化能力，适合「Embedding 语义召回 + SPLADE/BM42 等稀疏向量关键词增强」的生产场景。

### 📦 在项目中的位置

```
EasyCore.Agent（Agent SDK）
    └── EasyCore.Agent.RAG（RAG 切块 / MMR / Rerank 等）
            └── EasyCore.Vector.*（向量存储抽象与多后端实现）
                    └── EasyCore.Vector.Qdrant（本文档）
```

与其他向量后端（Redis、Milvus、PostgreSQL、Elasticsearch）保持一致的 API 风格，便于按环境切换存储引擎而无需改动业务代码。

---

## 9.2 架构图

### 9.2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-33cf79de.svg)


### 9.2.2 混合检索时序（Dense + Sparse）

![2-2-混合检索时序-dense-sparse](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-混合检索时序-dense-sparse-840ad150.svg)


### 9.2.3 存储模型

每个 Collection 在 Qdrant 中的组织方式：

| 层级 | 说明 |
|---|---|
| Collection | 向量集合，包含一个或多个 Named Dense Vector 及可选 Sparse Vector |
| Point | 单条记录，UUID 作为 Point Id |
| Named Vectors | 稠密向量，如 `documentVector` |
| Sparse Vectors | 稀疏向量，命名规则 `{Name}_sparse`，如 `documentVector_sparse` |
| Payload | 业务元数据：`content`、`metadata`（JSON）、`record`（完整记录 JSON）及反射出的标量字段 |

---

## 9.3 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查；支持 Named Dense Vector 与 Sparse Vector 联合建表。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 Point UUID 覆盖写入。
- 🔍 **稠密向量检索**：`VectorSearchAsync`，距离度量由 Collection 创建时的 `Distance` 决定。
- 🧩 **稀疏向量检索（差异化）**：`SparseSearchAsync`，传入 `SparseVectorValue`（`Indices` + `Values` 列表），适用于 SPLADE、BM42 等稀疏 Embedding 或手工关键词向量。
- 🔀 **原生混合检索（差异化）**：`HybridSearchAsync` 同时执行稠密与稀疏检索，按 `denseWeight` / `sparseWeight` 加权融合——**不是** Redis 后端的 BM25 候选合并模式。
- 🧮 **标量过滤**：向量检索均支持 Payload Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🧱 **强类型 Record 映射**：继承 `QdrantVectorRecord` 即可自动映射标量字段到 Payload；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCoreQdrant(...)` 扩展方法注册 Options、`QdrantClient` 与 `IQdrantVectorStore`。

---

## 9.4 环境要求

### 9.4.1 Qdrant 版本

需要运行 **Qdrant Server**（支持 Sparse Vector 的版本，推荐 1.7+）。

推荐部署方式：

```bash
# Docker 快速启动 Qdrant（HTTP 6333 / gRPC 6334）
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

> SDK 默认通过 **gRPC 端口 6334** 通信（非 HTTP 6333）。

### 9.4.2 .NET 版本

- .NET 8.0 及以上

### 9.4.3 NuGet 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| `Qdrant.Client` | 1.18.1 | Qdrant gRPC 客户端 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.0 | DI 扩展 |

---

## 9.5 快速开始

### 9.5.1 安装包

```bash
dotnet add package EasyCore.Vector.Qdrant
```

### 9.5.2 注册服务

```csharp
using EasyCore.Vector.Qdrant;

builder.Services.EasyCoreQdrant(options =>
{
    options.Host = "localhost";
    options.GrpcPort = 6334;       // gRPC 默认端口
    options.ApiKey = null;         // Qdrant Cloud 等场景可选
    options.UseHttps = false;      // 是否使用 HTTPS
});
```

### 9.5.3 定义向量实体

```csharp
using EasyCore.Vector.Qdrant;

public sealed class QdrantTextVector : QdrantVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

> `QdrantVectorRecord` 已内置 `Id`、`Content`、`Vectors`、`Metadata`，子类只需声明业务标量字段。标量属性会在 Upsert 时自动反射写入 Payload，供 Filter 使用。

### 9.5.4 创建 Collection 并写入数据

```csharp
using Qdrant.Client.Grpc;

public class KnowledgeService
{
    private readonly IQdrantVectorStore _vectorStore;
    private const string CollectionName = "knowledge_base";
    private const string VectorField = "contentVector";

    public KnowledgeService(IQdrantVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }

    public async Task EnsureCollectionAsync(CancellationToken cancellationToken = default)
    {
        var definition = new QdrantVectorCollectionDefinition
        {
            VectorFields =
            {
                new QdrantVectorFieldDefinition
                {
                    Name = VectorField,
                    Dimension = 1024,
                    Distance = Distance.Cosine,
                    EnableSparseVector = true   // 同时创建 contentVector_sparse 稀疏向量槽位
                }
            }
        };

        await _vectorStore.CreateCollectionAsync(CollectionName, definition, cancellationToken);
    }

    public async Task UpsertAsync(
        QdrantTextVector record,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        record.SetVector(VectorField, embedding);
        await _vectorStore.UpsertAsync(CollectionName, record, cancellationToken);
    }
}
```

### 9.5.5 稠密向量检索

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("EasyCore.Agent 支持哪些功能？");

var results = await _vectorStore.VectorSearchAsync<QdrantTextVector>(
    collectionName: CollectionName,
    vectorName: VectorField,
    vector: queryEmbedding,
    options: new QdrantVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.75f,
        IncludeMetadata = true
    });

foreach (var item in results)
{
    Console.WriteLine($"Score={item.Score:F4}, Content={item.Record.Content}");
}
```

### 9.5.6 稀疏向量检索

```csharp
var sparseQuery = new SparseVectorValue
{
    Indices = new List<uint> { 12, 88, 391 },
    Values = new List<float> { 1.2f, 0.7f, 2.4f }
};

var sparseResults = await _vectorStore.SparseSearchAsync<QdrantTextVector>(
    collectionName: CollectionName,
    vectorName: "contentVector_sparse",
    sparseVector: sparseQuery,
    options: new QdrantVectorSearchOptions { Limit = 10 });
```

### 9.5.7 混合检索（Dense + Sparse）

```csharp
var hybridResults = await _vectorStore.HybridSearchAsync<QdrantTextVector>(
    collectionName: CollectionName,
    denseVectorName: "contentVector",
    denseVector: queryEmbedding,
    sparseVectorName: "contentVector_sparse",
    sparseVector: sparseQuery,
    options: new QdrantVectorSearchOptions { Limit = 5 },
    denseWeight: 0.7f,
    sparseWeight: 0.3f);
```

---

## 9.6 配置说明

### 9.6.1 `QdrantOptions`

| 字段 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Host` | `string` | `localhost` | Qdrant 服务器主机名或 IP |
| `GrpcPort` | `int` | `6334` | Qdrant gRPC 端口 |
| `ApiKey` | `string?` | `null` | API Key（Qdrant Cloud 等认证场景） |
| `UseHttps` | `bool` | `false` | 是否使用 HTTPS 连接 |

### 9.6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `QdrantOptions` | Singleton | 配置快照 |
| `QdrantClient` | Singleton | gRPC 客户端连接复用 |
| `IQdrantVectorStore` | Scoped | 向量存储操作入口 |

---

## 9.7 数据模型与 Collection 设计

### 9.7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `QdrantVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors`、`Metadata` |
| `QdrantVectorCollectionDefinition` | Collection Schema 定义 |
| `QdrantVectorFieldDefinition` | 向量字段（维度、距离、是否启用稀疏向量） |
| `SparseVectorValue` | 稀疏向量值（`Indices` + `Values` 列表） |
| `QdrantVectorSearchOptions` | 检索参数 |
| `QdrantVectorFilter` | 过滤条件容器 |
| `QdrantQdrantVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 9.7.2 内置字段

每条记录在 Payload 中自动包含：

| 字段 | 说明 |
|---|---|
| `content` | 文本内容 |
| `metadata` | 标量字段 JSON 序列化 |
| `record` | 完整记录 JSON（检索反序列化用） |

业务标量属性（如 `DocumentId`、`Index`）会同时作为独立 Payload 字段写入，可直接用于 Filter。

### 9.7.3 向量字段配置

```csharp
new QdrantVectorFieldDefinition
{
    Name = "contentVector",              // 稠密向量字段名
    Dimension = 1024,                    // 必须与 Embedding 模型输出维度一致
    Distance = Distance.Cosine,          // Qdrant.Client.Grpc Distance 枚举
    EnableSparseVector = true            // 启用稀疏向量，自动创建 contentVector_sparse
}
```

#### `Distance` 枚举（Qdrant.Client.Grpc）

| 枚举值 | 说明 | 适用场景 |
|---|---|---|
| `Cosine` | 余弦距离（默认） | 文本 Embedding、语义检索 |
| `Euclid` | 欧氏距离（L2） | 通用向量空间 |
| `Dot` | 点积 | 已归一化向量 |
| `Manhattan` | 曼哈顿距离（L1） | 特殊度量需求 |

> 距离度量在 **Collection 创建时** 确定；`QdrantVectorSearchOptions` **不包含** `MetricType` 字段，检索时使用 Collection 配置的 `Distance`。

#### 稀疏向量命名规则

启用 `EnableSparseVector = true` 后，稀疏向量字段名自动生成为：

```
{稠密向量名}_sparse
```

例如：`contentVector` → `contentVector_sparse`

### 9.7.4 命名约束

- Collection 名不能为空或纯空白字符；
- 向量字段名不能为空；
- Point Id 使用 UUID 字符串格式。

---

## 9.8 API 使用示例

以下示例均基于 `IQdrantVectorStore`，接口继承关系为：

```
IQdrantVectorStore
  └── IVectorStore
        └── IQdrantVectorSearch
              ├── IQdrantSparseSearch
              └── IQdrantHybridSearch
```

> **注意**：`IVectorStore` **不包含** `GetAsync` / `QueryAsync` 方法。  
> 本库仅提供 Collection 管理（Create / Delete / Exists）、写入（Upsert / UpsertBatch）、删除（Delete）以及检索接口（VectorSearch / SparseSearch / HybridSearch）。

### 9.8.1 Collection 管理

```csharp
// 检查 Collection 是否存在
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 9.8.2 写入与删除

```csharp
// 单条 Upsert
await _vectorStore.UpsertAsync("test_collection", record);

// 批量 Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// 按 Id 删除
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 9.8.3 稠密向量检索（带 Filter）

```csharp
var options = new QdrantVectorSearchOptions
{
    Limit = 10,
    ScoreThreshold = 0.8f,
    IncludeVector = false,
    IncludeMetadata = true,
    Filter = new QdrantVectorFilter
    {
        Conditions =
        {
            new QdrantVectorFilterCondition
            {
                Field = "Index",
                Operator = QdrantVectorFilterOperator.In,
                Value = new[] { 1, 2, 3 }
            }
        }
    }
};

var results = await _vectorStore.VectorSearchAsync<QdrantTextVector>(
    "test_collection",
    "contentVector",
    queryVector,
    options);
```

### 9.8.4 稀疏向量检索

稀疏向量由 **索引（Indices）** 与 **权重（Values）** 组成，长度必须一致：

```csharp
var sparseVector = new SparseVectorValue
{
    Indices = new List<uint> { 100, 205, 1024 },
    Values = new List<float> { 0.8f, 1.5f, 0.3f }
};

var results = await _vectorStore.SparseSearchAsync<QdrantTextVector>(
    collectionName: "test_collection",
    vectorName: "contentVector_sparse",
    sparseVector: sparseVector,
    options: new QdrantVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.0f,
        Filter = new QdrantVectorFilter
        {
            Conditions =
            {
                new QdrantVectorFilterCondition
                {
                    Field = "DocumentId",
                    Operator = QdrantVectorFilterOperator.Equal,
                    Value = "doc-001"
                }
            }
        }
    });
```

### 9.8.5 混合检索（Dense + Sparse 加权融合）

与 Redis 后端的 Hybrid Search（向量 + BM25 候选融合）不同，Qdrant 后端在 SDK 层同时执行 **稠密向量检索** 与 **稀疏向量检索**，再按权重合并：

```csharp
var hybridResults = await _vectorStore.HybridSearchAsync<QdrantTextVector>(
    collectionName: "test_collection",
    denseVectorName: "contentVector",
    denseVector: queryEmbedding,
    sparseVectorName: "contentVector_sparse",
    sparseVector: sparseQuery,
    options: new QdrantVectorSearchOptions { Limit = 5 },
    denseWeight: 0.7f,
    sparseWeight: 0.3f);
```

融合算法：

1. 分别以 `Limit × 3` 的候选数执行稠密检索与稀疏检索；
2. 按 Point Id 合并两路结果；
3. 对稠密分、稀疏分分别按各自最大值归一化；
4. 加权求和：`Score = normDense × denseWeight + normSparse × sparseWeight`；
5. 按最终 Score 降序返回 Top-K。

### 9.8.6 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<QdrantTextVector>("test_collection", "contentVector", vector);
var sparseResults = _vectorStore.SparseSearch<QdrantTextVector>("test_collection", "contentVector_sparse", sparseVector);
var hybridResults = _vectorStore.HybridSearch<QdrantTextVector>(
    "test_collection", "contentVector", vector, "contentVector_sparse", sparseVector);
```

> 建议在 ASP.NET Core 业务代码中优先使用异步 API，避免阻塞线程池。

---

## 9.9 过滤与检索能力详解

### 9.9.1 支持的 Filter 运算符

| 运算符 | 说明 | 适用字段类型 | 示例 |
|---|---|---|---|
| `Equal` | 等于 | 数值 / 文本 / 布尔 | `DocumentId = "doc-001"` |
| `NotEqual` | 不等于 | 数值 / 文本 / 布尔 | `Index != 0` |
| `GreaterThan` | 大于 | 数值 | `Index > 5` |
| `GreaterThanOrEqual` | 大于等于 | 数值 | `Index >= 1` |
| `LessThan` | 小于 | 数值 | `Index < 10` |
| `LessThanOrEqual` | 小于等于 | 数值 | `Index <= 100` |
| `Contains` | 关键词匹配 | 文本 Payload | `Content` 包含 `"RAG"` |
| `In` | 多值匹配（OR） | 数值 / 文本 / 布尔 | `Index in (1,2,3)` |

多个 Condition 之间为 **AND** 关系（`Must` 连接）。`NotEqual` 映射为 `MustNot`。`In` 运算符内部为 OR。

### 9.9.2 `QdrantVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限 |
| `ScoreThreshold` | `null` | 相似度阈值，低于此分数的结果被 Qdrant 过滤 |
| `Filter` | `null` | 检索前 Payload 过滤条件 |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

> 与 Redis 后端不同，**无 `MetricType` 字段**——距离度量由 Collection 创建时的 `QdrantVectorFieldDefinition.Distance` 决定。

### 9.9.3 稠密向量检索执行流程

1. 根据 `Filter` 构建 Qdrant Payload Filter；
2. 调用 `QdrantClient.SearchAsync`，指定 Named Vector 与查询向量；
3. Qdrant 按 Collection 配置的 `Distance` 计算 Score；
4. 应用 `ScoreThreshold` 过滤；
5. 反序列化 Payload 中的 `record` JSON 为强类型 `TRecord`；
6. 按 Score 降序返回最多 `Limit` 条。

### 9.9.4 稀疏向量检索执行流程

1. 校验 `SparseVectorValue.Indices` 与 `Values` 长度一致；
2. 构建 Payload Filter（可选）；
3. 调用 `QdrantClient.SearchAsync`，传入 `sparseIndices` 参数；
4. Qdrant 在 Sparse Vector 索引上执行检索；
5. 返回带 Score 的强类型结果。

---

## 9.10 与 EasyCore.Agent.RAG 集成

在 `AspCoreAgent` Demo 中，Qdrant 向量库与 RAG 切块、Embedding 完整串联：

```csharp
// 1) 文档切块
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) 向量化并写入 Qdrant
var embeddingClient = _agent.CreateEmbeddingClient();

foreach (var chunk in chunks)
{
    var embedding = await _agent.EmbedAsync(chunk.Content);

    var record = new QdrantTextVector
    {
        Id = Guid.NewGuid().ToString("N"),
        DocumentId = chunk.DocumentId,
        Index = chunk.Index,
        StartIndex = chunk.StartIndex,
        EndIndex = chunk.EndIndex,
        Content = chunk.Content
    };

    record.SetVector("documentVector", embedding);
    await _qdrantVectorStore.UpsertAsync("test_collection", record);
}

// 3) 检索 + MMR 去重（EasyCore.Agent.RAG）
var candidates = await _qdrantVectorStore.VectorSearchAsync<QdrantTextVector>(...);

var mmrCandidates = candidates.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector("documentVector")
}).ToList();

var finalResults = MmrSelector.Select(mmrCandidates, topK: 2, lambda: 0.7);
```

典型 RAG 流水线（含混合检索增强）：

```text
原始文档
  ↓ DocumentChunker 切块
文本 Chunk
  ↓ Embedding 模型（稠密）+ 稀疏向量化（SPLADE/BM42 等）
稠密向量 + 稀疏向量 + 元数据
  ↓ UpsertAsync
Qdrant Vector Store
  ↓ VectorSearchAsync / SparseSearchAsync / HybridSearchAsync
召回候选
  ↓ MmrSelector / Reranker（EasyCore.Agent.RAG）
精炼上下文
  ↓ Agent ChatRunAsync
最终回答
```

---

## 9.11 最佳实践

- ✅ **Embedding 维度与 Schema 严格一致**：`QdrantVectorFieldDefinition.Dimension` 必须等于模型输出维度，否则写入或检索会失败。
- ✅ **需要混合检索时启用稀疏向量**：创建 Collection 时设置 `EnableSparseVector = true`，确保 `{Name}_sparse` 槽位存在。
- ✅ **Collection 只创建一次**：`CreateCollectionAsync` 在 Collection 已存在时会直接返回，建议在应用启动或首次导入前调用。
- ✅ **稀疏向量 Indices/Values 等长**：`SparseVectorValue` 的两个列表长度必须一致，否则 SDK 会抛出参数异常。
- ✅ **合理设置 Hybrid 权重**：语义为主场景 `denseWeight=0.7~0.8`；关键词精确匹配为主可增大 `sparseWeight`。
- ✅ **合理设置 `ScoreThreshold`**：过滤低质量召回，减少 LLM 上下文噪声。
- ✅ **大批量写入使用 `UpsertBatchAsync`**：减少 gRPC 往返次数；超大批量建议自行分批。
- ✅ **Point Id 使用 UUID**：SDK 以 UUID 格式存储 Point Id，建议使用 `Guid.NewGuid().ToString("N")` 或标准 UUID 格式。
- ⚠️ **Hybrid Search 为 SDK 层融合**：当前实现在客户端分别执行稠密与稀疏检索后加权合并，非 Qdrant 服务端 Prefetch Fusion API；候选池为 `Limit × 3`。
- ⚠️ **敏感数据不要写入 `Content` 明文**：必要时在入库前加密或脱敏。

---

## 9.12 FAQ

### ❓ Q1：`Collection not found` 或连接失败？

说明 Qdrant 服务未启动或 gRPC 端口不正确。请确认：

1. Qdrant 容器/服务已运行；
2. `GrpcPort = 6334`（非 HTTP 6333）；
3. `Host` 与防火墙配置正确。

### ❓ Q2：向量检索无结果或 Score 很低？

请检查：

1. Embedding 模型是否与入库时使用同一模型；
2. `Dimension`、`Distance` 是否与 Collection 定义一致；
3. 是否设置了过高的 `ScoreThreshold`；
4. `Filter` 条件是否过于严格。

### ❓ Q3：稀疏向量检索报错 `indices and values must have the same length`？

`SparseVectorValue.Indices` 与 `Values` 必须一一对应且长度相同。请检查稀疏 Embedding 模型的输出格式。

### ❓ Q4：为什么 `IVectorStore` 没有 `GetAsync` / `QueryAsync`？

Qdrant 后端聚焦于向量写入与相似度检索。按 Id 获取或纯标量查询可通过 Qdrant 原生 Client 扩展，当前 SDK 未暴露这些方法。业务检索请使用 `VectorSearchAsync`、`SparseSearchAsync` 或 `HybridSearchAsync`。

### ❓ Q5：Hybrid Search 与 Redis Hybrid Search 有何区别？

| 维度 | Qdrant Hybrid | Redis Hybrid |
|---|---|---|
| 融合对象 | 稠密向量分 + 稀疏向量分 | 向量分 + BM25 候选分 |
| 稀疏来源 | `SparseVectorValue`（Indices/Values） | 关键词 Query + 手工 BM25 分数 |
| 适用场景 | SPLADE/BM42 等稀疏 Embedding | RediSearch 全文检索 + 向量 |

### ❓ Q6：`EnableSparseVector = true` 后如何写入稀疏向量？

创建 Collection 时会注册 `{Name}_sparse` 稀疏向量槽位。写入时需在 Record 的 `Vectors` 中包含对应稀疏向量数据（可通过扩展 `QdrantVectorValue` 或直接使用 Qdrant Client 写入稀疏 Point）。Demo 中稀疏检索使用查询侧稀疏向量演示，生产环境需配合稀疏 Embedding 模型完成入库。

### ❓ Q7：Cosine / Euclid / Dot 如何选择？

- **Cosine**（默认）：文本语义检索首选；
- **Euclid**：关注绝对距离的场景；
- **Dot**：向量已 L2 归一化时可考虑；
- 创建 Collection 后 **不可更改** Distance，需删库重建。

---

## 9.13 EasyCore.Vector.Qdrant 详细介绍

### 9.13.1 设计目标

`EasyCore.Vector.Qdrant` 的核心目标是：在 .NET 应用中提供**生产可用**的 Qdrant 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：Named Dense Vector + Sparse Vector 联合建表；
2. **类型映射**：通过反射读写 Payload 标量字段，JSON 序列化完整 Record；
3. **检索表达**：屏蔽 Qdrant gRPC Filter 与 Named Vector 语法细节；
4. **差异化检索**：稀疏向量检索与 Dense+Sparse 混合检索一等公民支持。

### 9.13.2 接口分层

```
IQdrantHybridSearch
  ├── HybridSearchAsync / HybridSearch

IQdrantSparseSearch : IQdrantHybridSearch
  ├── SparseSearchAsync / SparseSearch

IQdrantVectorSearch : IQdrantSparseSearch
  ├── VectorSearchAsync / VectorSearch

IVectorStore : IQdrantVectorSearch
  ├── CreateCollectionAsync / DeleteCollectionAsync / CollectionExistsAsync
  ├── UpsertAsync / UpsertBatchAsync
  ├── DeleteAsync
  └── （无 GetAsync / QueryAsync）

IQdrantVectorStore : IVectorStore
  └── （标记接口，DI 注入使用）
```

### 9.13.3 典型落地步骤

1. 部署 Qdrant Server，确认 gRPC 6334 可访问；
2. 调用 `EasyCoreQdrant` 注册 DI；
3. 定义 `QdrantVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync`，按需 `EnableSparseVector`；
5. 文档切块 → 稠密 Embedding（+ 可选稀疏 Embedding）→ `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` / `HybridSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 9.13.4 与其他向量后端对比（选型参考）

| 维度 | Qdrant | 说明 |
|---|---|---|
| 部署复杂度 | 中 | 独立向量数据库，Docker 一键启动 |
| 向量规模 | 中大型 | HNSW 索引，适合百万~亿级 |
| 稀疏向量 | ✅ 原生支持 | `SparseSearchAsync` 一等公民 |
| 混合检索 | ✅ Dense + Sparse | SDK 加权融合，非 BM25 模式 |
| 标量 Query | ❌ SDK 未暴露 | 聚焦向量检索场景 |
| 生态一致性 | 高 | 与 EasyCore 其他向量库 Upsert/Search 用法一致 |

---

## 9.14 Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 Qdrant 向量库 API 示例。

### 9.14.1 启动 Qdrant

```bash
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

### 9.14.2 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

Demo 在 `Program.cs` 中注册 Qdrant：

```csharp
builder.Services.EasyCoreQdrant(options =>
{
    options.Host = "localhost";
    options.GrpcPort = 6334;
});
```

### 9.14.3 相关 API 端点

| 端点 | 说明 |
|---|---|
| `GET /api/Qdrant/QdrantVectorStoreUpsert` | 创建 Collection 并导入切块向量 |
| `GET /api/Qdrant/QdrantVectorStoreSearch` | 稠密向量检索 + Filter |
| `GET /api/Qdrant/QdrantVectorStoreSparseSearch` | **稀疏向量检索** + Filter |
| `GET /api/Qdrant/QdrantVectorStoreHybridSearch` | **Dense + Sparse 混合检索** |
| `GET /api/Qdrant/QdrantVectorStoreMmrSelector` | 向量检索 + MMR 去重 |
| `GET /api/Qdrant/QdrantVectorStoreDelete` | 删除单条记录（`?id=`） |
| `GET /api/Qdrant/QdrantVectorStoreCollectionExists` | 检查 Collection |
| `GET /api/Qdrant/QdrantVectorStoreDeleteCollection` | 删除整个 Collection |

Demo 实体定义见 `demo/AspCoreAgent/VectorEntity/QdrantTextVector.cs`。

---

---

## 10. EasyCore.Vector.Milvus

### 10.1 项目简介

**EasyCore.Vector.Milvus** 封装 Milvus 底层 SDK，提供与 EasyCore 其他向量后端一致的强类型 API，适用于大规模向量检索与 RAG 知识库场景。

### 📦 在项目中的位置

```
EasyCore.Agent → EasyCore.Agent.RAG → EasyCore.Vector.*
                                            └── EasyCore.Vector.Milvus（本文档）
```

---

## 10.2 架构图

![2-架构图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-架构图-ef6518fd.svg)


---

## 10.3 核心特性

- 🗂️ Collection 生命周期：创建、删除、存在性检查
- 📥 Upsert 单条/批量写入
- 🔍 KNN 向量检索 + 标量 Filter
- 🔀 Hybrid Search（向量 + 外部 BM25 候选融合）
- ⚙️ **Milvus 专有**：`FlushAsync`、`LoadAsync`、`ReleaseAsync`
- 🧱 强类型 `MilvusVectorRecord` 映射
- 🔌 `EasyCoreMilvus(...)` DI 注册

---

## 10.4 环境要求

- .NET 8.0+
- Milvus 2.x（Standalone 或 Cluster）
- NuGet：`Milvus.Client` 2.3.0-preview.1

```bash
# Docker 快速启动 Milvus Standalone
docker run -d --name milvus -p 19530:19530 -p 9091:9091 milvusdb/milvus:latest standalone
```

---

## 10.5 快速开始

### 10.5.1 注册服务

```csharp
builder.Services.EasyCoreMilvus(options =>
{
    options.Host = "localhost";
    options.Port = 19530;
    options.DatabaseName = "default";
    options.UserName = "";
    options.Password = "";
    options.UseTls = false;
});
```

### 10.5.2 定义实体

```csharp
public sealed class MilvusTextVector : MilvusVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

### 10.5.3 创建 Collection 并检索

```csharp
var definition = new MilvusVectorCollectionDefinition
{
    ScalarFields =
    {
        new MilvusScalarFieldDefinition { Name = "DocumentId", FieldType = ScalarFieldType.VarChar, MaxLength = 128 },
        new MilvusScalarFieldDefinition { Name = "Index", FieldType = ScalarFieldType.Int64 }
    },
    VectorFields =
    {
        new MilvusVectorFieldDefinition
        {
            Name = "contentVector",
            Dimension = 1024,
            MetricType = SimilarityMetricType.Cosine,
            IndexType = MilvusVectorIndexType.Hnsw
        }
    }
};

await _vectorStore.CreateCollectionAsync("test_collection", definition);

record.SetVector("contentVector", embedding);
await _vectorStore.UpsertAsync("test_collection", record);
await _vectorStore.FlushAsync("test_collection");

var results = await _vectorStore.VectorSearchAsync<MilvusTextVector>(
    "test_collection", "contentVector", queryVector,
    new MilvusVectorSearchOptions { Limit = 10, IncludeMetadata = true });
```

---

## 10.6 配置说明

### 10.6.1 `MilvusOptions`

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Host` | `localhost` | Milvus 主机 |
| `Port` | `19530` | gRPC 端口 |
| `DatabaseName` | `default` | 数据库名 |
| `UserName` / `Password` | — | 认证 |
| `Token` | — | Token 认证 |
| `UseTls` | `false` | 是否启用 TLS |

### 10.6.2 DI 生命周期

| 服务 | 生命周期 |
|---|---|
| `MilvusOptions` | Singleton |
| `MilvusClient` | Singleton |
| `IMilvusVectorStore` | Scoped |

---

## 10.7 数据模型与 Collection 设计

### 10.7.1 向量索引类型

| `MilvusVectorIndexType` | 说明 |
|---|---|
| `AutoIndex` | Milvus 自动选择（默认） |
| `Flat` | 暴力搜索 |
| `IvfFlat` | IVF_FLAT |
| `IvfSq8` | IVF_SQ8 |
| `Hnsw` | HNSW |

HNSW 参数：`M`（默认 16）、`EfConstruction`（默认 200）；IVF 参数：`NList`（默认 1024）。

### 10.7.2 内置字段

自动追加 `Id`（VarChar 主键）、`Content`（VarChar），无需重复声明。

### 10.7.3 命名约束

Collection 与字段名须符合：`^[A-Za-z_][A-Za-z0-9_]*$`

---

## 10.8 API 使用示例

### 10.8.1 Collection 管理

```csharp
await _vectorStore.CreateCollectionAsync("test_collection", definition);
var exists = await _vectorStore.CollectionExistsAsync("test_collection");
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 10.8.2 写入与删除

```csharp
await _vectorStore.UpsertAsync("test_collection", record);
await _vectorStore.UpsertBatchAsync("test_collection", records);
await _vectorStore.DeleteAsync("test_collection", id);
```

### 10.8.3 Get / Query

```csharp
var record = await _vectorStore.GetAsync<MilvusTextVector>(
    "test_collection", id, includeVector: true, vectorName: "contentVector");

var records = await _vectorStore.QueryAsync<MilvusTextVector>(
    "test_collection",
    new MilvusVectorFilter
    {
        Conditions = { new MilvusVectorFilterCondition { Field = "Index", Operator = MilvusVectorFilterOperator.In, Value = new[] { 1, 2, 3 } } }
    },
    limit: 10);
```

### 10.8.4 向量检索（带 Filter）

```csharp
var options = new MilvusVectorSearchOptions
{
    Limit = 10,
    ScoreThreshold = 0.8f,
    IncludeMetadata = true,
    Filter = new MilvusVectorFilter { /* conditions */ }
};

var results = await _vectorStore.VectorSearchAsync<MilvusTextVector>(
    "test_collection", "contentVector", queryVector, options);
```

### 10.8.5 Hybrid Search

```csharp
var hybridResults = await _vectorStore.HybridSearchAsync(
    "test_collection", "contentVector", queryVector, bm25Results,
    options: new MilvusVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f, bm25Weight: 0.3f);
```

---

## 10.9 Milvus 生命周期管理

Milvus 写入后数据在 growing segment，检索前需 Load 到内存。

| 方法 | 说明 |
|---|---|
| `FlushAsync(collectionName)` | 将 growing segment 刷入 sealed segment |
| `LoadAsync(collectionName)` | 将 Collection 加载到 Query Node 内存 |
| `ReleaseAsync(collectionName)` | 从内存释放 Collection |

![9-milvus-生命周期管理](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/9-milvus-生命周期管理-0e62eac8.svg)


> 向量检索内部会自动调用 `LoadAsync`；大批量写入后建议显式 `FlushAsync`。

---

## 10.10 过滤与检索能力详解

### 10.10.1 Filter 运算符

`Equal`、`NotEqual`、`GreaterThan`、`GreaterThanOrEqual`、`LessThan`、`LessThanOrEqual`、`Contains`、`In`

### 10.10.2 `MilvusVectorSearchOptions`

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回数量 |
| `ScoreThreshold` | `null` | 相似度阈值 |
| `Filter` | `null` | 标量过滤 |
| `MetricType` | `Cosine` | Milvus.Client 度量类型 |
| `IncludeVector` | `false` | 是否返回向量 |
| `IncludeMetadata` | `true` | 是否返回自定义标量字段 |

---

## 10.11 与 EasyCore.Agent.RAG 集成

```csharp
var chunks = DocumentChunker.Chunk(content, documentId, 800, 100);
foreach (var chunk in chunks)
{
    var embedding = await agent.EmbedAsync(chunk.Content);
    var record = new MilvusTextVector { /* map chunk fields */ };
    record.SetVector("contentVector", embedding);
    await vectorStore.UpsertAsync("test_collection", record);
}
await vectorStore.FlushAsync("test_collection");

var candidates = await vectorStore.VectorSearchAsync<MilvusTextVector>(...);
var final = MmrSelector.Select(mmrCandidates, topK: 2, lambda: 0.7);
```

---

## 10.12 最佳实践

- ✅ 大批量写入后调用 `FlushAsync`
- ✅ 生产环境监控 Collection Load 状态
- ✅ `Dimension` 与 Embedding 模型严格一致
- ✅ HNSW 适合在线低延迟；IVF 适合超大规模
- ⚠️ `ReleaseAsync` 后需重新 `LoadAsync` 才能检索
- ⚠️ 并行节点写入 Items 时使用不同 Key

---

## 10.13 FAQ

### ❓ Q1：检索无结果？
检查 Collection 是否已 Load、是否已 Flush、Filter 是否过严、维度是否匹配。

### ❓ Q2：Flush 与 Load 区别？
Flush 持久化 segment；Load 加载到内存供查询。

### ❓ Q3：AutoIndex 选什么？
由 Milvus 根据数据规模自动选择，一般无需手动指定。

---

## 10.14 Demo 运行

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

| 端点 | 说明 |
|---|---|
| `GET /api/Milvus/MilvusVectorStoreUpsert` | 创建并导入 |
| `GET /api/Milvus/MilvusVectorStoreSearch` | 向量检索 |
| `GET /api/Milvus/MilvusVectorStoreMmrSelector` | MMR 去重 |
| `GET /api/Milvus/MilvusVectorStoreGet` | 按 Id 获取 |
| `GET /api/Milvus/MilvusVectorStoreQuery` | 标量 Query |
| `GET /api/Milvus/MilvusVectorStoreHybridSearch` | 混合检索 |
| `GET /api/Milvus/MilvusVectorStoreFlush` | Flush |
| `GET /api/Milvus/MilvusVectorStoreLoad` | Load |
| `GET /api/Milvus/MilvusVectorStoreRelease` | Release |
| `GET /api/Milvus/MilvusVectorStoreDelete` | 删除记录 |
| `GET /api/Milvus/MilvusVectorStoreDeleteCollection` | 删除 Collection |

---

---

## 11. EasyCore.Vector.PostgreSQL

### 11.1 项目简介

### 🎯 解决什么问题？

在构建 RAG（检索增强生成）或语义搜索系统时，通常需要：

- 将文档切块并向量化后持久化存储；
- 按相似度快速召回 Top-K 相关片段；
- 结合业务字段（文档 ID、分块序号、租户 ID 等）做过滤；
- 在关键词检索与向量检索之间做融合（Hybrid Search）；
- 与 ASP.NET Core 依赖注入体系无缝集成。

直接使用 Npgsql 与 pgvector 原生 SQL 时，往往需要处理 `CREATE EXTENSION vector`、表结构 DDL、HNSW/IVFFlat 索引创建、`<=>` / `<->` / `<#>` 距离运算符、参数化 Filter 拼接、Upsert 冲突处理等细节，接入成本较高。

**EasyCore.Vector.PostgreSQL** 通过统一的 `IVectorStore` / `IPostgreSqlVectorStore` 抽象，封装上述底层细节，让你用强类型 C# 模型完成向量库的创建、写入、检索与删除。

### 📦 在项目中的位置

```
EasyCore.Agent（Agent SDK）
    └── EasyCore.Agent.RAG（RAG 切块 / MMR / Rerank 等）
            └── EasyCore.Vector.*（向量存储抽象与多后端实现）
                    └── EasyCore.Vector.PostgreSQL（本文档）
```

与其他向量后端（Redis、Qdrant、Milvus、Elasticsearch）保持一致的 API 风格，便于按环境切换存储引擎而无需改动业务代码。

---

## 11.2 架构图

### 11.2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-c7ad4952.svg)


### 11.2.2 一次向量检索时序

![2-2-一次向量检索时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-一次向量检索时序-1d64b161.svg)


### 11.2.3 存储模型

每个 Collection 在 PostgreSQL 中的组织方式：

| 层级 | 映射规则 | 说明 |
|---|---|---|
| Schema | `public` | 默认使用 public schema |
| Collection | 表名（小写） | `collectionName` 映射为 PostgreSQL 表 |
| 行（Row） | 一条 `PostgreSqlVectorRecord` | 每行对应一条向量文档 |
| 列（Column） | 标量字段 + `vector(n)` | `Id` 主键、`Content` 文本、自定义标量、向量列 |

创建 Collection 时 SDK 会自动执行：

```sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE "test_collection" (
    "Id" VARCHAR(128) PRIMARY KEY,
    "Content" VARCHAR(65535) NOT NULL,
    "DocumentId" VARCHAR(128) NOT NULL,
    "Index" BIGINT NOT NULL,
    "documentVector" vector(1024) NOT NULL
);

CREATE INDEX IF NOT EXISTS "ix_test_collection_documentvector"
ON "test_collection"
USING hnsw ("documentVector" vector_cosine_ops);
```

---

## 11.3 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查；删除 Collection 即 `DROP TABLE`。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 `ON CONFLICT (Id) DO UPDATE` 实现幂等写入。
- 🔍 **向量相似度检索**：基于 pgvector 距离运算符，支持 Cosine / L2 / Inner Product 三种度量。
- 🧮 **标量过滤**：向量检索与纯标量 Query 均支持 Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🔀 **混合检索（Hybrid Search）**：将向量检索结果与 BM25/关键词候选按权重融合，提升召回质量。
- 🧱 **强类型 Record 映射**：继承 `PostgreSqlVectorRecord` 即可自动映射标量字段；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCorePostgreSql(...)` 扩展方法注册 Options 与 `IPostgreSqlVectorStore`。

---

## 11.4 环境要求

### 11.4.1 PostgreSQL 与 pgvector

需要 **PostgreSQL 数据库** 并安装 **pgvector 扩展**。

首次创建 Collection 时 SDK 会自动执行 `CREATE EXTENSION IF NOT EXISTS vector;`。若数据库用户无创建扩展权限，请由 DBA 预先执行：

```sql
CREATE EXTENSION vector;
```

推荐部署方式（Docker）：

```bash
# Docker 快速启动带 pgvector 的 PostgreSQL
docker run -d \
  --name pgvector \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=vector_db \
  -p 5432:5432 \
  pgvector/pgvector:pg17
```

使用 Docker Compose：

```yaml
services:
  postgres:
    image: pgvector/pgvector:pg17
    container_name: pgvector
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: your_password
      POSTGRES_DB: vector_db
    ports:
      - "5432:5432"
    volumes:
      - pgvector_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d vector_db"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  pgvector_data:
```

启动后验证扩展：

```bash
docker exec -it pgvector psql -U postgres -d vector_db -c "CREATE EXTENSION IF NOT EXISTS vector;"
docker exec -it pgvector psql -U postgres -d vector_db -c "SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';"
```

### 11.4.2 .NET 版本

- .NET 8.0 及以上

### 11.4.3 NuGet 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| `Npgsql` | 10.x | PostgreSQL 连接与 SQL 执行 |
| `Pgvector` | 0.3.2 | pgvector 类型与向量运算支持 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.x | DI 扩展 |

---

## 11.5 快速开始

### 11.5.1 安装包

```bash
dotnet add package EasyCore.Vector.PostgreSQL
```

### 11.5.2 注册服务

```csharp
using EasyCore.Vector.PostgreSQL;

builder.Services.EasyCorePostgreSql(options =>
{
    options.ConnectionString =
        "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=your_password;";
});
```

### 11.5.3 定义向量实体

```csharp
using EasyCore.Vector.PostgreSQL;

public sealed class PostgreSqlTextVector : PostgreSqlVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

> `PostgreSqlVectorRecord` 已内置 `Id`、`Content`、`Vectors`，子类只需声明业务标量字段。

### 11.5.4 创建 Collection 并写入数据

```csharp
public class KnowledgeService
{
    private readonly IPostgreSqlVectorStore _vectorStore;
    private const string CollectionName = "knowledge_base";
    private const string VectorField = "contentVector";

    public KnowledgeService(IPostgreSqlVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }

    public async Task EnsureCollectionAsync(CancellationToken cancellationToken = default)
    {
        var definition = new PostgreSqlVectorCollectionDefinition
        {
            ScalarFields =
            {
                new PostgreSqlScalarFieldDefinition
                {
                    Name = "DocumentId",
                    FieldType = ScalarFieldType.VarChar,
                    MaxLength = 128
                },
                new PostgreSqlScalarFieldDefinition
                {
                    Name = "Index",
                    FieldType = ScalarFieldType.Int64
                }
            },
            VectorFields =
            {
                new PostgreSqlVectorFieldDefinition
                {
                    Name = VectorField,
                    Dimension = 1024,
                    MetricType = PostgreSqlSimilarityMetricType.Cosine,
                    IndexType = PostgreSqlVectorIndexType.Hnsw
                }
            }
        };

        await _vectorStore.CreateCollectionAsync(CollectionName, definition, cancellationToken);
    }

    public async Task UpsertAsync(
        PostgreSqlTextVector record,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        record.SetVector(VectorField, embedding);
        await _vectorStore.UpsertAsync(CollectionName, record, cancellationToken);
    }
}
```

### 11.5.5 向量检索

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("EasyCore.Agent 支持哪些功能？");

var results = await _vectorStore.VectorSearchAsync<PostgreSqlTextVector>(
    collectionName: CollectionName,
    vectorName: VectorField,
    vector: queryEmbedding,
    options: new PostgreSqlVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.75f,
        IncludeMetadata = true
    });

foreach (var item in results)
{
    Console.WriteLine($"Score={item.Score:F4}, Content={item.Record.Content}");
}
```

---

## 11.6 配置说明

### 11.6.1 `PostgreSqlOptions`

| 字段 | 类型 | 说明 | 示例 |
|---|---|---|---|
| `ConnectionString` | `string` | PostgreSQL 连接字符串（必填） | 见下方示例 |

连接字符串支持 Npgsql 标准格式，例如：

```
Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=your_password
Host=db.example.com;Port=5432;Database=vector_db;Username=app;Password=secret;SSL Mode=Require
```

常用参数说明：

| 参数 | 说明 |
|---|---|
| `Host` | 数据库主机地址 |
| `Port` | 端口，默认 5432 |
| `Database` | 数据库名 |
| `Username` / `Password` | 认证凭据 |
| `SSL Mode` | 生产环境建议 `Require` 或 `VerifyFull` |
| `Pooling` | 连接池，默认开启 |
| `Timeout` | 连接超时（秒） |

### 11.6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `PostgreSqlOptions` | Singleton | 配置快照 |
| `IPostgreSqlVectorStore` | Scoped | 向量存储操作入口，内部持有 `NpgsqlDataSource` |

---

## 11.7 数据模型与 Collection 设计

### 11.7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `PostgreSqlVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors` |
| `PostgreSqlVectorCollectionDefinition` | Collection Schema 定义 |
| `PostgreSqlVectorFieldDefinition` | 向量字段（维度、度量、索引类型） |
| `PostgreSqlScalarFieldDefinition` | 标量字段（类型、是否主键） |
| `PostgreSqlVectorSearchOptions` | 向量检索参数 |
| `PostgreSqlVectorFilter` | 过滤条件容器 |
| `PostgreSqlVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 11.7.2 内置字段

创建 Collection 时，SDK 会自动追加以下字段，**无需**在业务定义中重复声明：

| 字段 | PostgreSQL 类型 | 说明 |
|---|---|---|
| `Id` | `VARCHAR(128) PRIMARY KEY` | 主键，Upsert 冲突键 |
| `Content` | `VARCHAR(65535)` | 文本内容，可用于关键词过滤 |

### 11.7.3 向量字段配置

```csharp
new PostgreSqlVectorFieldDefinition
{
    Name = "contentVector",                              // 向量字段名（对应列名）
    Dimension = 1024,                                    // 必须与 Embedding 模型输出维度一致
    MetricType = PostgreSqlSimilarityMetricType.Cosine,  // Cosine / L2 / InnerProduct
    IndexType = PostgreSqlVectorIndexType.Hnsw,          // Hnsw / Ivfflat
    CreateIndex = true,                                  // 是否创建向量索引
    Lists = 100                                          // IVFFlat 的 lists 参数
}
```

#### 相似度度量说明

| 枚举值 | pgvector 运算符 | Score 转换方式 |
|---|---|---|
| `Cosine` | `<=>`（cosine distance） | `1 - distance`（越大越相似） |
| `L2` | `<->`（Euclidean distance） | `1 / (1 + distance)` |
| `InnerProduct` | `<#>`（negative inner product） | `distance * -1` |

#### 索引类型说明

| 索引类型 | pgvector 语法 | 适用场景 |
|---|---|---|
| `Hnsw`（默认） | `USING hnsw (... vector_cosine_ops)` | 在线检索，低延迟 |
| `Ivfflat` | `USING ivfflat (... vector_cosine_ops) WITH (lists = N)` | 大规模数据，可调 lists 参数 |

索引 ops class 随 `MetricType` 自动选择：

| MetricType | ops class |
|---|---|
| `Cosine` | `vector_cosine_ops` |
| `L2` | `vector_l2_ops` |
| `InnerProduct` | `vector_ip_ops` |

### 11.7.4 标量字段类型

| `ScalarFieldType` | PostgreSQL 映射 |
|---|---|
| `Bool` | `BOOLEAN` |
| `Int8` / `Int16` | `SMALLINT` |
| `Int32` | `INTEGER` |
| `Int64` | `BIGINT` |
| `Float` | `REAL` |
| `Double` | `DOUBLE PRECISION` |
| `String` / `VarChar` | `TEXT` / `VARCHAR(n)` |
| `Json` | `JSONB` |

### 11.7.5 命名约束

Collection 名与字段名必须符合标识符规则：

```
^[A-Za-z_][A-Za-z0-9_]*$
```

例如：`test_collection`、`DocumentId` ✅；`test-collection`、`123abc` ❌。

> Collection 名会映射为 PostgreSQL 表名。`CollectionExistsAsync` 以小写形式查询 `information_schema.tables`，建议统一使用小写 Collection 名（如 `test_collection`）。

---

## 11.8 API 使用示例

以下示例均基于 `IPostgreSqlVectorStore`，接口继承关系为：

```
IPostgreSqlVectorStore
  └── IVectorStore
        └── IPostgreSqlVectorSearch
              └── IPostgreSqlHybridSearch
```

### 11.8.1 Collection 管理

```csharp
// 检查 Collection 是否存在（查询 public schema 下对应表）
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（表已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection（DROP TABLE）
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 11.8.2 写入与删除

```csharp
// 单条 Upsert（ON CONFLICT DO UPDATE）
await _vectorStore.UpsertAsync("test_collection", record);

// 批量 Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// 按 Id 删除
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 11.8.3 按 Id 获取

```csharp
var record = await _vectorStore.GetAsync<PostgreSqlTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 11.8.4 标量 Query（不含向量相似度）

```csharp
var records = await _vectorStore.QueryAsync<PostgreSqlTextVector>(
    collectionName: "test_collection",
    filter: new PostgreSqlVectorFilter
    {
        Conditions =
        {
            new PostgreSqlVectorFilterCondition
            {
                Field = "DocumentId",
                Operator = PostgreSqlVectorFilterOperator.Equal,
                Value = "doc-001"
            }
        }
    },
    limit: 20,
    offset: 0,
    includeMetadata: true);
```

### 11.8.5 向量检索（带 Filter）

```csharp
var options = new PostgreSqlVectorSearchOptions
{
    Limit = 10,
    ScoreThreshold = 0.8f,
    MetricType = PostgreSqlSimilarityMetricType.Cosine,
    IncludeVector = false,
    IncludeMetadata = true,
    Filter = new PostgreSqlVectorFilter
    {
        Conditions =
        {
            new PostgreSqlVectorFilterCondition
            {
                Field = "Index",
                Operator = PostgreSqlVectorFilterOperator.In,
                Value = new[] { 1, 2, 3 }
            }
        }
    }
};

var results = await _vectorStore.VectorSearchAsync<PostgreSqlTextVector>(
    "test_collection",
    "contentVector",
    queryVector,
    options);
```

### 11.8.6 混合检索（Hybrid Search）

Hybrid Search 适用于「语义相似 + 关键词命中」Combined Ranking 场景。BM25 候选可由 `QueryAsync` + `Contains` 等方式获得，再与向量结果融合：

```csharp
// 1) 关键词候选（示例：Content 包含 "RAG"）
var keywordRecords = await _vectorStore.QueryAsync<PostgreSqlTextVector>(
    "test_collection",
    new PostgreSqlVectorFilter
    {
        Conditions =
        {
            new PostgreSqlVectorFilterCondition
            {
                Field = "Content",
                Operator = PostgreSqlVectorFilterOperator.Contains,
                Value = "RAG"
            }
        }
    },
    limit: 10,
    includeMetadata: true);

// 2) 构造 BM25 候选分数（生产环境可替换为真实 BM25 分数）
var bm25Results = keywordRecords
    .Select((record, index) => new PostgreSqlVectorSearchResult<PostgreSqlTextVector>
    {
        Record = record,
        Score = Math.Max(0.1f, 1.0f - index * 0.08f)
    })
    .ToList();

// 3) 混合融合
var hybridResults = await _vectorStore.HybridSearchAsync(
    collectionName: "test_collection",
    vectorName: "contentVector",
    vector: queryVector,
    bm25Results: bm25Results,
    options: new PostgreSqlVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f,
    bm25Weight: 0.3f);
```

融合算法会对向量分与 BM25 分分别归一化后加权求和，返回 Top-K 结果。

### 11.8.7 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<PostgreSqlTextVector>("test_collection", "contentVector", vector);
```

> 建议在 ASP.NET Core 业务代码中优先使用异步 API，避免阻塞线程池。

---

## 11.9 过滤与检索能力详解

### 11.9.1 支持的 Filter 运算符

| 运算符 | 说明 | 适用字段类型 | SQL 实现 |
|---|---|---|---|
| `Equal` | 等于 | 数值 / 文本 / 布尔 | `column = @p` |
| `NotEqual` | 不等于 | 数值 / 文本 / 布尔 | `column <> @p` |
| `GreaterThan` | 大于 | 数值 | `column > @p` |
| `GreaterThanOrEqual` | 大于等于 | 数值 | `column >= @p` |
| `LessThan` | 小于 | 数值 | `column < @p` |
| `LessThanOrEqual` | 小于等于 | 数值 | `column <= @p` |
| `Contains` | 文本包含（不区分大小写） | 文本 | `column ILIKE '%value%'` |
| `In` | 多值匹配 | 数值 / 文本 / 布尔 | `column = ANY(@p)` |

多个 Condition 之间为 **AND** 关系。`In` 运算符内部为 OR 语义（`= ANY` 数组）。

### 11.9.2 `PostgreSqlVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限 |
| `ScoreThreshold` | `null` | 相似度阈值，低于此分数的结果被过滤 |
| `Filter` | `null` | 检索前过滤条件 |
| `MetricType` | `Cosine` | 分数转换使用的度量类型 |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

### 11.9.3 向量检索执行流程

1. 根据 `Filter` 构建参数化 `WHERE` 子句；
2. 在内层子查询中计算 Score 表达式（基于 pgvector 距离运算符）；
3. 外层应用 `ScoreThreshold` 过滤；
4. 按 Score 降序排序并截取 `Limit` 条；
5. 通过反射将行映射为强类型 `TRecord`。

---

## 11.10 与 EasyCore.Agent.RAG 集成

在 `AspCoreAgent` Demo 中，PostgreSQL 向量库与 RAG 切块、Embedding 完整串联：

```csharp
// 1) 文档切块
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) 向量化并写入 PostgreSQL
var embeddingClient = _agent.CreateEmbeddingClient();

foreach (var chunk in chunks)
{
    var embedding = await _agent.EmbedAsync(chunk.Content);

    var record = new PostgreSqlTextVector
    {
        Id = Guid.NewGuid().ToString("N"),
        DocumentId = chunk.DocumentId,
        Index = chunk.Index,
        StartIndex = chunk.StartIndex,
        EndIndex = chunk.EndIndex,
        Content = chunk.Content
    };

    record.SetVector("documentVector", embedding);
    await _postgreSqlVectorStore.UpsertAsync("test_collection", record);
}

// 3) 检索 + MMR 去重（EasyCore.Agent.RAG）
var candidates = await _postgreSqlVectorStore.VectorSearchAsync<PostgreSqlTextVector>(...);

var mmrCandidates = candidates.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector("documentVector")
}).ToList();

var finalResults = MmrSelector.Select(mmrCandidates, topK: 2, lambda: 0.7);
```

典型 RAG 流水线：

```text
原始文档
  ↓ DocumentChunker 切块
文本 Chunk
  ↓ Embedding 模型
向量 + 元数据
  ↓ UpsertAsync
PostgreSQL Vector Store (pgvector)
  ↓ VectorSearchAsync / HybridSearchAsync
召回候选
  ↓ MmrSelector / Reranker（EasyCore.Agent.RAG）
精炼上下文
  ↓ Agent ChatRunAsync
最终回答
```

---

## 11.11 最佳实践

- ✅ **Embedding 维度与 Schema 严格一致**：`PostgreSqlVectorFieldDefinition.Dimension` 必须等于模型输出维度，否则写入或检索会失败。
- ✅ **Collection 只创建一次**：`CreateCollectionAsync` 在表已存在时会直接返回，建议在应用启动或首次导入前调用。
- ✅ **生产环境预先创建 pgvector 扩展**：确保数据库用户有 `CREATE EXTENSION` 权限，或在部署脚本中提前执行。
- ✅ **合理设置 `ScoreThreshold`**：过滤低质量召回，减少 LLM 上下文噪声。
- ✅ **大批量写入使用 `UpsertBatchAsync`**：减少连接开销；超大批量建议自行分批（默认逐条 Upsert）。
- ✅ **IVFFlat 索引需足够数据量**：pgvector 建议 IVFFlat 在有一定数据量后再创建，并调优 `Lists` 参数。
- ✅ **HNSW 适合在线检索**：默认索引类型，查询延迟低，无需额外调参。
- ✅ **Hybrid Search 中 BM25 分数需归一化语义**：SDK 内部会按最大值归一化，但上游 BM25 分数应具有可比性。
- ✅ **敏感数据不要写入 `Content` 明文**：必要时在入库前加密或脱敏。
- ⚠️ **避免频繁 DeleteCollection**：`DeleteCollectionAsync` 会 `DROP TABLE`，大数据量下重建索引成本较高。
- ⚠️ **生产环境启用连接池与 SSL**：通过连接字符串配置 `Pooling=true` 与 `SSL Mode=Require`。

---

## 11.12 FAQ

### ❓ Q1：`relation "xxx" does not exist` 报错？

说明 Collection 尚未创建或表已被删除。请先调用 `CreateCollectionAsync`，并确认 `collectionName` 与写入/检索时一致。

### ❓ Q2：向量检索无结果或 Score 很低？

请检查：

1. Embedding 模型是否与入库时使用同一模型；
2. `Dimension`、`MetricType` 是否与 Collection 定义一致；
3. 是否设置了过高的 `ScoreThreshold`；
4. `Filter` 条件是否过于严格；
5. pgvector 索引 ops class 是否与 MetricType 匹配。

### ❓ Q3：`Invalid identifier` 报错？

Collection 名、字段名必须符合 `^[A-Za-z_][A-Za-z0-9_]*$`，请勿使用连字符或中文。

### ❓ Q4：为什么 `includeVector = true` 时必须传 `vectorName`？

一条记录可能包含多个向量字段，SDK 需要明确读取哪个字段的向量数据。

### ❓ Q5：`permission denied to create extension "vector"`？

当前数据库用户无创建扩展权限。请由超级用户预先执行 `CREATE EXTENSION vector;`，或授予相应权限。

### ❓ Q6：Ivfflat 与 HNSW 如何选择？

- **HNSW**（默认）：查询延迟低，适合在线检索，无需调 lists 参数；
- **Ivfflat**：适合超大规模数据，通过 `Lists` 参数控制聚类数量，但召回率与构建成本需权衡。

### ❓ Q7：能否与现有 PostgreSQL 业务表共存？

可以。每个 Collection 对应独立表，与业务表互不干扰。注意 Collection 名不要与已有表名冲突。

### ❓ Q8：Upsert 是原子操作吗？

单条 Upsert 使用 `INSERT ... ON CONFLICT (Id) DO UPDATE`，在单条语句内原子完成。批量 Upsert 当前为逐条执行。

---

## 11.13 EasyCore.Vector.PostgreSQL 详细介绍

### 11.13.1 设计目标

`EasyCore.Vector.PostgreSQL` 的核心目标是：在 .NET 应用中提供**生产可用**的 PostgreSQL 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：自动补全 `Id` / `Content` 字段，校验主键与字段重名，自动创建 pgvector 扩展；
2. **类型映射**：通过反射读写表列，支持常见标量类型与枚举；
3. **检索表达**：屏蔽 pgvector 距离运算符与参数化 SQL 拼接细节；
4. **可组合性**：向量检索、标量 Query、Hybrid 融合分层接口，便于扩展。

### 11.13.2 接口分层

```
IPostgreSqlHybridSearch
  ├── HybridSearchAsync / HybridSearch

IPostgreSqlVectorSearch : IPostgreSqlHybridSearch
  ├── VectorSearchAsync / VectorSearch

IVectorStore : IPostgreSqlVectorSearch
  ├── CreateCollectionAsync / DeleteCollectionAsync / CollectionExistsAsync
  ├── UpsertAsync / UpsertBatchAsync
  ├── GetAsync / QueryAsync / DeleteAsync

IPostgreSqlVectorStore : IVectorStore
  └── （标记接口，DI 注入使用）
```

### 11.13.3 典型落地步骤

1. 部署 PostgreSQL + pgvector（Docker 或云托管），配置 `ConnectionString`；
2. 调用 `EasyCorePostgreSql` 注册 DI；
3. 定义 `PostgreSqlVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync` 确保表与索引存在；
5. 文档切块 → Embedding → `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 11.13.4 与其他向量后端对比（选型参考）

| 维度 | PostgreSQL + pgvector | 说明 |
|---|---|---|
| 部署复杂度 | 低 | 若已有 PostgreSQL，安装扩展即可 |
| 向量规模 | 中大型 | HNSW/IVFFlat 支持百万级向量 |
| 混合检索 | 支持 | 需自行提供 BM25 候选分数 |
| 事务/关系型 | 强 | 向量与业务数据可同库事务 |
| SQL 生态 | 强 | 可直接用 SQL 分析、备份、复制 |
| 生态一致性 | 高 | 与 EasyCore 其他 `IVectorStore` 用法一致 |

---

## 11.14 Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 PostgreSQL 向量库 API 示例。

### 11.14.1 启动 PostgreSQL + pgvector

```bash
docker run -d \
  --name pgvector \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=Q123456 \
  -e POSTGRES_DB=vector_db \
  -p 5432:5432 \
  pgvector/pgvector:pg17
```

### 11.14.2 配置连接字符串

在 `demo/AspCoreAgent/Program.cs` 中确认连接字符串与 Docker 配置一致：

```csharp
builder.Services.EasyCorePostgreSql(options =>
{
    options.ConnectionString =
        "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=Q123456;";
});
```

### 11.14.3 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 11.14.4 相关 API 端点

| 端点 | 说明 |
|---|---|
| `GET /api/PostgreSQL/PostgreSqlVectorStoreUpsert` | 创建 Collection 并导入切块向量 |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreSearch` | 向量检索 + Filter + Score 过滤 |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreMmrSelector` | 向量检索 + MMR 去重 |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreGet` | 按 Id 获取记录 |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreQuery` | 标量 Query |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreHybridSearch` | 混合检索示例 |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreDelete` | 删除单条记录 |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreCollectionExists` | 检查 Collection |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreDeleteCollection` | 删除整个 Collection |

Demo 实体定义见 `demo/AspCoreAgent/VectorEntity/PostgreSqlTextVector.cs`。

---

---

## 12. EasyCore.Vector.Elasticsearch

### 12.1 项目简介

### 🎯 解决什么问题？

在构建 RAG（检索增强生成）或语义搜索系统时，通常需要：

- 将文档切块并向量化后持久化存储；
- 按相似度快速召回 Top-K 相关片段；
- 结合业务字段（文档 ID、分块序号、租户 ID 等）做过滤；
- 在关键词检索与向量检索之间做融合（Hybrid Search）；
- 与 ASP.NET Core 依赖注入体系无缝集成。

直接使用 Elasticsearch 原生 API 时，往往需要处理 Index Mapping 构建、`dense_vector` 字段配置、KNN 查询 DSL、Bool Filter 拼接、`_source` 字段裁剪等细节，接入成本较高。

**EasyCore.Vector.Elasticsearch** 通过统一的 `IVectorStore` / `IElasticsearchVectorStore` 抽象，封装上述底层细节，让你用强类型 C# 模型完成向量库的创建、写入、检索与删除。

### 📦 在项目中的位置

```
EasyCore.Agent（Agent SDK）
    └── EasyCore.Agent.RAG（RAG 切块 / MMR / Rerank 等）
            └── EasyCore.Vector.*（向量存储抽象与多后端实现）
                    └── EasyCore.Vector.Elasticsearch（本文档）
```

与其他向量后端（Redis、Qdrant、Milvus、PostgreSQL）保持一致的 API 风格，便于按环境切换存储引擎而无需改动业务代码。

---

## 12.2 架构图

### 12.2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-5d67ae37.svg)


### 12.2.2 一次向量检索时序

![2-2-一次向量检索时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-一次向量检索时序-a3c2fa45.svg)


### 12.2.3 存储模型

每个 Collection 在 Elasticsearch 中的组织方式：

| 层级 | 命名规则 | 说明 |
|---|---|---|
| Index | `ToIndexName(collectionName)` | Collection 名经小写与字符规范化后映射为 ES Index |
| Document `_id` | `Record.Id` | 文档主键，Upsert 时作为 Elasticsearch 文档 ID |
| 向量字段 | `dense_vector` | 支持 Cosine / L2 / Inner Product 相似度 |
| 文本字段 | `Content` + `Content.keyword` | 全文检索与精确/通配符过滤 |

每条记录以 **Elasticsearch Document** 形式存储，内置字段 `Id`、`Content`，以及自定义标量字段与 `dense_vector` 向量字段。

---

## 12.3 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查；Index 已存在时 `CreateCollectionAsync` 直接跳过。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 Index API 按 `_id` 覆盖写入。
- 🔍 **KNN 向量检索**：基于 Elasticsearch `dense_vector` + KNN 查询，支持 Cosine / L2 / Inner Product 三种相似度度量。
- 🧮 **标量过滤**：向量检索与纯标量 Query 均支持 Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🔀 **混合检索（Hybrid Search）**：将向量检索结果与外部 BM25/关键词候选按权重融合，提升召回质量。
- 🧱 **强类型 Record 映射**：继承 `ElasticsearchVectorRecord` 即可自动映射标量字段；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCoreElasticsearch(...)` 扩展方法注册 Options 与 `IElasticsearchVectorStore`。

---

## 12.4 环境要求

### 12.4.1 Elasticsearch 版本

需要 **Elasticsearch 8.0 及以上**（支持 `dense_vector` 索引与 KNN 检索）。

推荐部署方式：

```bash
# Docker 快速启动 Elasticsearch 8（单节点，开发环境）
docker run -d --name elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  docker.elastic.co/elasticsearch/elasticsearch:8.15.0
```

> 生产环境请启用安全认证，并在 `ElasticsearchOptions` 中配置 `UserName` / `Password`。

### 12.4.2 .NET 版本

- .NET 8.0 及以上

### 12.4.3 NuGet 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| `Elastic.Clients.Elasticsearch` | 8.15.6 | 官方 .NET 客户端，Index / Search / KNN |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.2 | DI 扩展 |

---

## 12.5 快速开始

### 12.5.1 安装包

```bash
dotnet add package EasyCore.Vector.Elasticsearch
```

### 12.5.2 注册服务

```csharp
using EasyCore.Vector.Elasticsearch;

builder.Services.EasyCoreElasticsearch(options =>
{
    options.Url = "http://localhost:9200";
    // options.UserName = "elastic";   // 可选，Basic 认证
    // options.Password = "your_password";
});
```

### 12.5.3 定义向量实体

```csharp
using EasyCore.Vector.Elasticsearch;

public sealed class ElasticsearchTextVector : ElasticsearchVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

> `ElasticsearchVectorRecord` 已内置 `Id`、`Content`、`Vectors`，子类只需声明业务标量字段。

### 12.5.4 创建 Collection 并写入数据

```csharp
public class KnowledgeService
{
    private readonly IElasticsearchVectorStore _vectorStore;
    private const string CollectionName = "knowledge_base";
    private const string VectorField = "contentVector";

    public KnowledgeService(IElasticsearchVectorStore vectorStore)
    {
        _vectorStore = vectorStore;
    }

    public async Task EnsureCollectionAsync(CancellationToken cancellationToken = default)
    {
        var definition = new ElasticsearchVectorCollectionDefinition
        {
            ScalarFields =
            {
                new ElasticsearchScalarFieldDefinition
                {
                    Name = "DocumentId",
                    FieldType = ScalarFieldType.VarChar,
                    MaxLength = 128
                },
                new ElasticsearchScalarFieldDefinition
                {
                    Name = "Index",
                    FieldType = ScalarFieldType.Int64
                }
            },
            VectorFields =
            {
                new ElasticsearchVectorFieldDefinition
                {
                    Name = VectorField,
                    Dimension = 1024,
                    MetricType = ElasticsearchSimilarityMetricType.Cosine,
                    IndexType = ElasticsearchVectorIndexType.Hnsw
                }
            }
        };

        await _vectorStore.CreateCollectionAsync(CollectionName, definition, cancellationToken);
    }

    public async Task UpsertAsync(
        ElasticsearchTextVector record,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        record.SetVector(VectorField, embedding);
        await _vectorStore.UpsertAsync(CollectionName, record, cancellationToken);
    }
}
```

### 12.5.5 向量检索

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("EasyCore.Agent 支持哪些功能？");

var results = await _vectorStore.VectorSearchAsync<ElasticsearchTextVector>(
    collectionName: CollectionName,
    vectorName: VectorField,
    vector: queryEmbedding,
    options: new ElasticsearchVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.75f,
        IncludeMetadata = true
    });

foreach (var item in results)
{
    Console.WriteLine($"Score={item.Score:F4}, Content={item.Record.Content}");
}
```

---

## 12.6 配置说明

### 12.6.1 `ElasticsearchOptions`

| 字段 | 类型 | 说明 | 示例 |
|---|---|---|---|
| `Url` | `string` | Elasticsearch 服务地址（**必填**） | `http://localhost:9200` |
| `UserName` | `string?` | Basic 认证用户名（可选） | `elastic` |
| `Password` | `string?` | Basic 认证密码（可选） | `your_password` |

当 `UserName` 非空时，SDK 自动启用 Basic Authentication；`Password` 未设置时按空字符串处理。

### 12.6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `ElasticsearchOptions` | Singleton | 配置快照 |
| `IElasticsearchVectorStore` | Scoped | 向量存储操作入口 |

---

## 12.7 数据模型与 Collection 设计

### 12.7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `ElasticsearchVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors` |
| `ElasticsearchVectorCollectionDefinition` | Collection Schema 定义 |
| `ElasticsearchVectorFieldDefinition` | 向量字段（维度、度量、索引类型） |
| `ElasticsearchScalarFieldDefinition` | 标量字段（类型、是否主键） |
| `ElasticsearchVectorSearchOptions` | 向量检索参数 |
| `ElasticsearchVectorFilter` | 过滤条件容器 |
| `ElasticsearchVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 12.7.2 内置字段

创建 Collection 时，SDK 会自动追加以下字段，**无需**在业务定义中重复声明：

| 字段 | 类型 | 说明 |
|---|---|---|
| `Id` | `Keyword`（主键） | 文档 ID，对应 Elasticsearch `_id` |
| `Content` | `Text` + `Content.keyword` | 文本内容，支持全文与关键词过滤 |

### 12.7.3 向量字段配置

```csharp
new ElasticsearchVectorFieldDefinition
{
    Name = "contentVector",           // 向量字段名
    Dimension = 1024,                 // 必须与 Embedding 模型输出维度一致
    MetricType = ElasticsearchSimilarityMetricType.Cosine,  // Cosine / L2 / InnerProduct
    IndexType = ElasticsearchVectorIndexType.Hnsw,          // Hnsw / Ivfflat
    CreateIndex = true,               // 是否创建 dense_vector 索引
    Lists = 100                       // Ivfflat 场景下影响 ef_construction
}
```

#### 相似度度量说明

| 枚举值 | Elasticsearch 映射 | 说明 |
|---|---|---|
| `Cosine` | `cosine` | 余弦相似度（默认，适合文本 Embedding） |
| `L2` | `l2_norm` | 欧氏距离 |
| `InnerProduct` | `dot_product` | 内积（向量需归一化时效果最佳） |

#### 索引类型说明

| 枚举值 | 底层实现 | 说明 |
|---|---|---|
| `Hnsw`（默认） | HNSW（`m=16`, `ef_construction=100`） | 在线检索延迟低，推荐默认 |
| `Ivfflat` | HNSW + 调高 `ef_construction` | 通过 `Lists` 参数影响构建参数 |

### 12.7.4 标量字段类型

| `ScalarFieldType` | Elasticsearch 映射 |
|---|---|
| `Bool` | `boolean` |
| `Int8` ~ `Int64` | `long` |
| `Float` / `Double` | `double` |
| `String` / `VarChar` | `keyword` |
| `Json` | `object` |

### 12.7.5 命名约束

Collection 名与字段名必须符合标识符规则：

```
^[A-Za-z_][A-Za-z0-9_]*$
```

例如：`test_collection`、`DocumentId` ✅；`test-collection`、`123abc` ❌。

**Index 名称规范化**：Collection 名会经 `ToIndexName` 转为小写 Elasticsearch Index 名，非法字符替换为 `_`，并以 `idx_` 前缀处理边界情况。业务代码中始终使用原始 `collectionName` 传参即可。

---

## 12.8 API 使用示例

以下示例均基于 `IElasticsearchVectorStore`，接口继承关系为：

```
IElasticsearchVectorStore
  └── IVectorStore
        └── IElasticsearchVectorSearch
              └── IElasticsearchHybridSearch
```

### 12.8.1 Collection 管理

```csharp
// 检查 Collection 是否存在
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（Index 已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection（删除整个 Index）
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 12.8.2 写入与删除

```csharp
// 单条 Upsert
await _vectorStore.UpsertAsync("test_collection", record);

// 批量 Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// 按 Id 删除
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 12.8.3 按 Id 获取

```csharp
var record = await _vectorStore.GetAsync<ElasticsearchTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 12.8.4 标量 Query（不含向量相似度）

```csharp
var records = await _vectorStore.QueryAsync<ElasticsearchTextVector>(
    collectionName: "test_collection",
    filter: new ElasticsearchVectorFilter
    {
        Conditions =
        {
            new ElasticsearchVectorFilterCondition
            {
                Field = "DocumentId",
                Operator = ElasticsearchVectorFilterOperator.Equal,
                Value = "doc-001"
            }
        }
    },
    limit: 20,
    offset: 0,
    includeMetadata: true);
```

### 12.8.5 向量检索（带 Filter）

```csharp
var options = new ElasticsearchVectorSearchOptions
{
    Limit = 10,
    ScoreThreshold = 0.8f,
    MetricType = ElasticsearchSimilarityMetricType.Cosine,
    IncludeVector = false,
    IncludeMetadata = true,
    Filter = new ElasticsearchVectorFilter
    {
        Conditions =
        {
            new ElasticsearchVectorFilterCondition
            {
                Field = "Index",
                Operator = ElasticsearchVectorFilterOperator.In,
                Value = new[] { 1, 2, 3 }
            }
        }
    }
};

var results = await _vectorStore.VectorSearchAsync<ElasticsearchTextVector>(
    "test_collection",
    "contentVector",
    queryVector,
    options);
```

### 12.8.6 混合检索（Hybrid Search）

Hybrid Search 适用于「语义相似 + 关键词命中」Combined Ranking 场景。BM25 候选可由 `QueryAsync` + `Contains` 等方式获得，再与向量结果融合：

```csharp
// 1) 关键词候选（示例：Content 包含 "RAG"）
var keywordRecords = await _vectorStore.QueryAsync<ElasticsearchTextVector>(
    "test_collection",
    new ElasticsearchVectorFilter
    {
        Conditions =
        {
            new ElasticsearchVectorFilterCondition
            {
                Field = "Content",
                Operator = ElasticsearchVectorFilterOperator.Contains,
                Value = "RAG"
            }
        }
    },
    limit: 10,
    includeMetadata: true);

// 2) 构造 BM25 候选分数（生产环境可替换为真实 BM25 分数）
var bm25Results = keywordRecords
    .Select((record, index) => new ElasticsearchVectorSearchResult<ElasticsearchTextVector>
    {
        Record = record,
        Score = Math.Max(0.1f, 1.0f - index * 0.08f)
    })
    .ToList();

// 3) 混合融合
var hybridResults = await _vectorStore.HybridSearchAsync(
    collectionName: "test_collection",
    vectorName: "contentVector",
    vector: queryVector,
    bm25Results: bm25Results,
    options: new ElasticsearchVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f,
    bm25Weight: 0.3f);
```

融合算法会对向量分与 BM25 分分别归一化后加权求和，返回 Top-K 结果。

### 12.8.7 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<ElasticsearchTextVector>("test_collection", "contentVector", vector);
```

> 建议在 ASP.NET Core 业务代码中优先使用异步 API，避免阻塞线程池。

---

## 12.9 过滤与检索能力详解

### 12.9.1 支持的 Filter 运算符

| 运算符 | 说明 | 适用字段类型 | 示例 |
|---|---|---|---|
| `Equal` | 等于 | 数值 / 文本 / 布尔 | `DocumentId = "doc-001"` |
| `NotEqual` | 不等于 | 数值 / 文本 / 布尔 | `Index != 0` |
| `GreaterThan` | 大于 | 数值 | `Index > 5` |
| `GreaterThanOrEqual` | 大于等于 | 数值 | `Index >= 1` |
| `LessThan` | 小于 | 数值 | `Index < 10` |
| `LessThanOrEqual` | 小于等于 | 数值 | `Index <= 100` |
| `Contains` | 文本包含（通配符） | 文本 | `Content` 包含 `"RAG"` |
| `In` | 多值匹配（OR） | 数值 / 文本 / 布尔 | `Index in (1,2,3)` |

多个 Condition 之间为 **AND** 关系（Bool `must` 组合）。`In` 运算符内部为 OR。

> `Content` 字段过滤自动路由至 `Content.keyword` 子字段；`Contains` 使用大小写不敏感通配符查询。

### 12.9.2 `ElasticsearchVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限（KNN `k`） |
| `ScoreThreshold` | `null` | 相似度阈值，映射为 ES `min_score` |
| `Filter` | `null` | KNN 检索前置过滤条件 |
| `MetricType` | `Cosine` | 度量类型（与 Index Mapping 一致） |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

### 12.9.3 向量检索执行流程

1. 将 `collectionName` 规范化为 Elasticsearch Index 名；
2. 根据 `Filter` 构建 Bool / Term / Range / Wildcard 查询；
3. 构建 `KnnSearch`：`k = Limit`，`num_candidates = max(Limit * 10, Limit)`；
4. 若存在 Filter，附加至 KNN `filter` 子句；
5. 设置 `min_score`（当 `ScoreThreshold` 有值时）；
6. 解析 `_source` 并映射为强类型 `TRecord`；
7. 按 `_score` 降序返回结果。

---

## 12.10 与 EasyCore.Agent.RAG 集成

在 `AspCoreAgent` Demo 中，Elasticsearch 向量库与 RAG 切块、Embedding 完整串联：

```csharp
// 1) 文档切块
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) 向量化并写入 Elasticsearch
var embeddingClient = _agent.CreateEmbeddingClient();

foreach (var chunk in chunks)
{
    var embedding = await _agent.EmbedAsync(chunk.Content);

    var record = new ElasticsearchTextVector
    {
        Id = Guid.NewGuid().ToString("N"),
        DocumentId = chunk.DocumentId,
        Index = chunk.Index,
        StartIndex = chunk.StartIndex,
        EndIndex = chunk.EndIndex,
        Content = chunk.Content
    };

    record.SetVector("documentVector", embedding);
    await _elasticsearchVectorStore.UpsertAsync("test_collection", record);
}

// 3) 检索 + MMR 去重（EasyCore.Agent.RAG）
var candidates = await _elasticsearchVectorStore.VectorSearchAsync<ElasticsearchTextVector>(...);

var mmrCandidates = candidates.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector("documentVector")
}).ToList();

var finalResults = MmrSelector.Select(mmrCandidates, topK: 2, lambda: 0.7);
```

典型 RAG 流水线：

```text
原始文档
  ↓ DocumentChunker 切块
文本 Chunk
  ↓ Embedding 模型
向量 + 元数据
  ↓ UpsertAsync
Elasticsearch Vector Store
  ↓ VectorSearchAsync / HybridSearchAsync
召回候选
  ↓ MmrSelector / Reranker（EasyCore.Agent.RAG）
精炼上下文
  ↓ Agent ChatRunAsync
最终回答
```

---

## 12.11 最佳实践

- ✅ **Embedding 维度与 Schema 严格一致**：`ElasticsearchVectorFieldDefinition.Dimension` 必须等于模型输出维度，否则写入或检索会失败。
- ✅ **Collection 只创建一次**：`CreateCollectionAsync` 在 Index 已存在时会直接返回，建议在应用启动或首次导入前调用。
- ✅ **生产环境启用 ES 安全认证**：配置 `UserName` / `Password`，并使用 HTTPS 端点。
- ✅ **合理设置 `ScoreThreshold`**：过滤低质量召回，减少 LLM 上下文噪声。
- ✅ **大批量写入自行分批**：`UpsertBatchAsync` 逐条 Index，超大批量建议分批以控制请求压力。
- ✅ **Hybrid Search 中 BM25 分数需归一化语义**：SDK 内部会按最大值归一化，但上游 BM25 分数应具有可比性。
- ✅ **敏感数据不要写入 `Content` 明文**：必要时在入库前加密或脱敏。
- ⚠️ **避免频繁 DeleteCollection**：`DeleteCollectionAsync` 会删除整个 Index，大数据量下重建成本较高。
- ⚠️ **Index 名称小写**：Elasticsearch Index 名自动小写化，请勿依赖大小写区分 Collection。

---

## 12.12 FAQ

### ❓ Q1：`index_not_found_exception` 报错？

说明 Collection 尚未创建或 Index 已被删除。请先调用 `CreateCollectionAsync`，并确认 `collectionName` 与写入/检索时一致。

### ❓ Q2：向量检索无结果或 Score 很低？

请检查：

1. Embedding 模型是否与入库时使用同一模型；
2. `Dimension`、`MetricType` 是否与 Collection 定义一致；
3. 是否设置了过高的 `ScoreThreshold`；
4. `Filter` 条件是否过于严格；
5. `dense_vector` 索引是否已创建（`CreateIndex = true`）。

### ❓ Q3：`Invalid identifier` 报错？

Collection 名、字段名必须符合 `^[A-Za-z_][A-Za-z0-9_]*$`，请勿使用连字符或中文。

### ❓ Q4：为什么 `includeVector = true` 时必须传 `vectorName`？

一条记录可能包含多个向量字段，SDK 需要明确读取哪个字段的向量数据。

### ❓ Q5：Collection 名大小写是否敏感？

业务层 `collectionName` 区分大小写，但映射到 Elasticsearch 时会统一小写。`test_collection` 与 `Test_Collection` 会指向同一 Index。

### ❓ Q6：Ivfflat 与 HNSW 如何选择？

- **HNSW**（默认）：查询延迟低，适合在线检索；
- **Ivfflat**：通过 `Lists` 调整 `ef_construction`，适合对构建参数有特殊权衡的场景。

### ❓ Q7：能否直接使用 Elasticsearch 原生查询？

可以。`IElasticsearchVectorStore` 封装了常用向量操作；复杂全文检索或聚合场景可另行注入 `ElasticsearchClient` 补充。

---

## 12.13 EasyCore.Vector.Elasticsearch 详细介绍

### 12.13.1 设计目标

`EasyCore.Vector.Elasticsearch` 的核心目标是：在 .NET 应用中提供**生产可用**的 Elasticsearch 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：自动补全 `Id` / `Content` 字段，校验主键与字段重名；
2. **类型映射**：通过反射读写 Document 字段，支持常见标量类型与枚举；
3. **检索表达**：屏蔽 KNN + Bool Filter DSL 细节；
4. **可组合性**：向量检索、标量 Query、Hybrid 融合分层接口，便于扩展。

### 12.13.2 接口分层

```
IElasticsearchHybridSearch
  ├── HybridSearchAsync / HybridSearch

IElasticsearchVectorSearch : IElasticsearchHybridSearch
  ├── VectorSearchAsync / VectorSearch

IVectorStore : IElasticsearchVectorSearch
  ├── CreateCollectionAsync / DeleteCollectionAsync / CollectionExistsAsync
  ├── UpsertAsync / UpsertBatchAsync
  ├── GetAsync / QueryAsync / DeleteAsync

IElasticsearchVectorStore : IVectorStore
  └── （标记接口，DI 注入使用）
```

### 12.13.3 典型落地步骤

1. 部署 Elasticsearch 8+，配置 `Url`（及认证信息）；
2. 调用 `EasyCoreElasticsearch` 注册 DI；
3. 定义 `ElasticsearchVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync` 确保 Index 存在；
5. 文档切块 → Embedding → `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 12.13.4 与其他向量后端对比（选型参考）

| 维度 | Elasticsearch | 说明 |
|---|---|---|
| 部署复杂度 | 中 | 需 ES 8+ 集群，但生态成熟 |
| 向量规模 | 中大型 | 适合百万级以上 Chunk |
| 混合检索 | 支持 | 原生 BM25 + 外部候选融合 |
| 全文检索 | 强 | `Content` 天然支持全文与关键词 |
| 生态一致性 | 高 | 与 EasyCore 其他 `IVectorStore` 用法一致 |

---

## 12.14 Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 Elasticsearch 向量库 API 示例。

### 12.14.1 启动 Elasticsearch

```bash
docker run -d --name elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  docker.elastic.co/elasticsearch/elasticsearch:8.15.0
```

### 12.14.2 启动 Demo

在 `Program.cs` 中确认 Elasticsearch 地址：

```csharp
builder.Services.EasyCoreElasticsearch(options =>
{
    options.Url = "http://localhost:9200";
});
```

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 12.14.3 相关 API 端点

| 端点 | 说明 |
|---|---|
| `GET /api/Elasticsearch/ElasticsearchVectorStoreUpsert` | 创建 Collection 并导入切块向量 |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreSearch` | 向量检索 + Filter + Score 过滤 |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreMmrSelector` | 向量检索 + MMR 去重 |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreGet` | 按 Id 获取记录 |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreQuery` | 标量 Query |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreHybridSearch` | 混合检索示例 |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreDelete` | 删除单条记录 |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreCollectionExists` | 检查 Collection |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreDeleteCollection` | 删除整个 Collection |

Demo 实体 `ElasticsearchTextVector` 包含 `DocumentId`、`Index`、`StartIndex`、`EndIndex` 字段，向量字段名为 `documentVector`。

---

---

## 13. 完整 RAG 技术栈示例

### 13.1 入库服务

```csharp
public class KnowledgeIngestService
{
    private readonly QianwenAgent _agent;
    private readonly IRedisVectorStore _store;

    public async Task IngestAsync(string docId, string content, CancellationToken ct = default)
    {
        const string collection = "knowledge_base";
        const string vectorField = "documentVector";

        var chunks = DocumentChunker.Chunk(content, docId, chunkSize: 800, overlapSize: 100);

        foreach (var chunk in chunks)
        {
            var embedding = await _agent.EmbedAsync(chunk.Content, ct);

            var record = new RedisTextVector
            {
                Id = chunk.Id,
                DocumentId = chunk.DocumentId,
                ChunkIndex = chunk.Index,
                StartIndex = chunk.StartIndex,
                EndIndex = chunk.EndIndex,
                Content = chunk.Content
            };
            record.SetVector(vectorField, embedding);

            await _store.UpsertAsync(collection, record, ct);
        }
    }
}
```

### 13.2 检索 + 生成服务

```csharp
public class RagChatService
{
    public async Task<string> AskAsync(string sessionId, string userMessage, CancellationToken ct = default)
    {
        var history = _agent.GetChatContext(sessionId);
        var aiAgent = _agent.CreateAgent("kb", "根据检索上下文回答，无法从上下文得出则明确说明。", _tools);

        var query = await QueryRewrite.RewriteAsync(userMessage, aiAgent, history, ct);
        var queries = await MultiQueryGenerator.GenerateAsync(query, aiAgent, count: 3, ct);

        var merged = new Dictionary<string, RedisVectorSearchResult<RedisTextVector>>();

        foreach (var q in queries.Distinct())
        {
            var vec = await _agent.EmbedAsync(q, ct);
            var hits = await _store.VectorSearchAsync<RedisTextVector>(
                "knowledge_base", "documentVector", vec,
                new RedisVectorSearchOptions { Limit = 10, ScoreThreshold = 0.65f, IncludeVector = true }, ct);

            foreach (var hit in hits)
            {
                if (!merged.ContainsKey(hit.Record.Id) || merged[hit.Record.Id].Score < hit.Score)
                    merged[hit.Record.Id] = hit;
            }
        }

        var mmrCandidates = merged.Values.Select(h => new MmrCandidate
        {
            Id = h.Record.Id,
            Content = h.Record.Content,
            Score = h.Score,
            Vector = h.Record.GetVector("documentVector")
        }).ToList();

        var selected = MmrSelector.Select(mmrCandidates, topK: 5, lambda: 0.7);
        var contextBlock = string.Join("\n---\n", selected.Select(c => $"[{c.Id}]\n{c.Content}"));

        var prompt = $"""
            参考以下资料回答问题：

            {contextBlock}

            用户问题：{userMessage}
            """;

        return await _agent.ChatRunAsync(sessionId, aiAgent, prompt, cancellationToken: ct);
    }
}
```

### 13.3 用 Pipeline 编排 RAG 步骤

```csharp
var ragPipeline = global::EasyCore.Pipeline.Pipeline.Create()
    .AddFunc(async ctx =>
    {
        var msg = ctx.Input ?? "";
        ctx.Set("userMessage", msg);
        var history = agent.GetChatContext(ctx.SessionId ?? "default");
        ctx.Set("history", history);
    })
    .AddFunc(async ctx =>
    {
        var rewritten = await QueryRewrite.RewriteAsync(
            ctx.Get<string>("userMessage")!,
            aiAgent,
            (IList<ChatMessage>)ctx.Get<object>("history")!);
        ctx.Set("query", rewritten);
    })
    .AddFunc(async ctx =>
    {
        var vec = await agent.EmbedAsync(ctx.Get<string>("query")!);
        var hits = await store.VectorSearchAsync<RedisTextVector>(/* ... */);
        ctx.Set("hits", hits);
    })
    .AddFunc(async ctx =>
    {
        ctx.Output = await agent.ChatRunAsync(ctx.SessionId!, aiAgent, BuildPrompt(ctx));
    });

await PipelineRunner.RunAsync(ragPipeline, new PipelineContext
{
    Input = userMessage,
    SessionId = sessionId
});
```

---

## 14. Demo 项目详解

### 14.1 独立 Demo 一览

| 项目 | 端口 | Swagger | 启动命令 |
|---|---|---|---|
| `Demo.EasyCore.Agent` | 5230 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Agent` |
| `Demo.EasyCore.Agent.RAG` | 5231 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Agent.RAG` |
| `Demo.EasyCore.Pipeline` | 5232 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Pipeline` |
| `Demo.EasyCore.Vector.Elasticsearch` | 5233 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Vector.Elasticsearch` |
| `Demo.EasyCore.Vector.Milvus` | 5234 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Vector.Milvus` |
| `Demo.EasyCore.Vector.PostgreSQL` | 5235 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Vector.PostgreSQL` |
| `Demo.EasyCore.Vector.Qdrant` | 5236 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Vector.Qdrant` |
| `Demo.EasyCore.Vector.Redis` | 5237 | `/swagger` | `dotnet run --project demo/Demo.EasyCore.Vector.Redis` |

### 14.2 Demo.EasyCore.Agent API

| 端点 | 方法 | 说明 |
|---|---|---|
| `/api/Agent/chat` | GET | 多轮对话，`ChatRequest`: Message, SessionId |
| `/api/Agent/chat-with-tools` | GET | 带 WeatherTool 的对话 |
| `/api/Agent/embedding` | GET | Qwen Embedding，`EmbeddingRequest`: Text |
| `/api/Agent/context` | GET | 查看 session 历史，`SessionRequest`: SessionId |

**示例：**

```http
GET http://localhost:5230/api/Agent/chat?Message=你好&SessionId=demo-1
GET http://localhost:5230/api/Agent/chat-with-tools?Message=北京天气&SessionId=demo-1
GET http://localhost:5230/api/Agent/embedding?Text=EasyCore.Agent
```

### 14.3 Demo.EasyCore.Agent.RAG API

| 端点 | 说明 |
|---|---|
| `/api/Rag/chunk` | 文档切块 |
| `/api/Rag/rewrite` | Query Rewrite（可带 SessionId 多轮） |
| `/api/Rag/multi-query` | Multi Query 生成 |
| `/api/Rag/mmr` | MMR 选取演示 |

### 14.4 Demo.EasyCore.Pipeline API

| 端点 | 说明 |
|---|---|
| `/api/Pipeline/run?Input=1` | Input=1 代码分支；2 SQL；其他聊天分支 |

返回 `PipelineRunResponse`：Output、Intent、Traces（StepName、StepType、Success、ElapsedMilliseconds）。

### 14.5 向量 Demo 通用模式

各 `Demo.EasyCore.Vector.*` 项目通常提供：

- Collection 初始化 / Upsert 示例文档；
- VectorSearch（Embedding 由 Qwen Agent 生成）；
- 部分含 HybridSearch、Filter、MMR 组合端点。

配置 `appsettings.json` 中的 `QianwenClientOptions.ApiKey` 及对应中间件连接字符串。

### 14.6 AspCoreAgent 综合 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
# http://localhost:5229/swagger
```

集成 Agent、RAG、Pipeline、全部向量后端等，适合联调；学习单模块建议用独立 Demo。

### 14.7 编译整个解决方案

```bash
dotnet build EasyCore.Agent.sln
```

---

## 15. Tool 开发指南

### 15.1 基础 Tool

```csharp
public class WeatherTool
{
    [AITool("get_weather")]
    [ToolDescription("Gets weather for a city")]
    [ToolAuthorize("weather.*")]
    public Task<WeatherResult> GetWeatherAsync(
        [ToolDescription("City name")] string city)
    {
        return Task.FromResult(new WeatherResult { City = city, Weather = "Sunny" });
    }
}
```

### 15.2 扫描与注册规则

- 扫描 `AppDomain.CurrentDomain.BaseDirectory` 下 `*.dll`；
- 排除 `Microsoft.*`、`System.*`；
- `public` 实例方法 + `[AITool]`；
- 工具类可通过 DI 注册，Provider 从 `IServiceProvider` 解析实例。

### 15.3 权限过滤

| 模式 | 含义 |
|---|---|
| `*` | 允许全部 |
| `order.read` | 精确匹配 |
| `order.*` | 前缀通配 |

### 15.4 在 CreateAgent 时限制工具

```csharp
var tools = toolProvider.GetToolsByNamesAndAuth(
    auth: new[] { "order.*" },
    "search_orders", "get_order_detail");
var agent = client.CreateAgent("order-bot", instructions, tools);
```

---

## 16. 配置参考

### 16.1 环境变量

| 变量 | 用途 |
|---|---|
| `EASYCORE_AGENT_API_KEY` | 默认 Agent ApiKey（可通过 EnvName 覆盖） |
| `REDIS_PASSWORD` | Redis 密码（示例） |

### 16.2 向量库连接（示例）

| 后端 | appsettings 键示例 |
|---|---|
| Redis | `RedisOptions:ConnectionString` |
| Qdrant | `QdrantOptions:Host`, `Port` |
| Milvus | `MilvusOptions:Host`, `Port` |
| PostgreSQL | `PostgreSqlOptions:ConnectionString` |
| Elasticsearch | `ElasticsearchOptions:Uri` |

各后端完整配置见本文档 §8–§12。

---

## 17. 最佳实践

### Agent 层

- ✅ 生产使用 Redis 会话存储；
- ✅ 设置合理 `MaxContextCount` 防止 Token 溢出；
- ✅ 按角色限制 Tool 暴露范围；
- ✅ Tool 内校验 LLM 传入参数。

### RAG 层

- ✅ 统一 `DocumentChunker` 入库；
- ✅ 多轮场景启用 QueryRewrite；
- ✅ MultiQuery 后按 Id 合并最高分；
- ✅ MMR 前设置 `IncludeVector = true`。

### 向量层

- ✅ 先 CreateCollection 再 Upsert；
- ✅ Embedding 维度与 Collection 一致；
- ✅ 多租户用标量 Filter；
- ✅ 合理设置 ScoreThreshold。

### Pipeline 层

- ✅ 单步单一职责；
- ✅ 并行后顺序合并；
- ✅ 导出 Traces 到日志或前端。

### 安全

- ✅ 密钥不入库，用环境变量或密钥管理；
- ✅ 防 Prompt Injection，对检索内容做边界标记。

---

## 18. FAQ

### Q1：`ApiKey is not configured`？

检查 `appsettings.json`、`EnvName` 环境变量，确认无不可见字符。ApiKey 仅允许 ASCII。

### Q2：Tool 未被调用？

检查 public 实例方法、`[AITool]`、DLL 是否被扫描、是否传入 `CreateAgent` 的 tools 列表。

### Q3：Memory 模式上下文丢失？

Memory 仅进程内；多实例或重启请用 Redis。

### Q4：向量检索无结果？

检查 Collection、维度、是否 Upsert、`ScoreThreshold`、Filter 是否过严。

### Q5：如何切换向量后端？

保持实体字段与 Collection  schema 一致，更换 DI 扩展与 Store 注入即可。

### Q6：Pipeline 步骤异常？

异常向上抛出，Trace 中 `Success=false` 含 `ErrorMessage`；Parallel 任一分支失败则整体失败。

### Q7：RAG 是否必须全部模块？

按场景组合：单轮 FAQ 只需 Chunk+Search；多轮 +Rewrite；召回不足 +MultiQuery；重复多 +MMR。

### Q8：能否不用 EasyCore.Agent 只用 RAG？

`DocumentChunker`、`MmrSelector` 无 LLM 依赖；Rewrite/MultiQuery 需要 `AIAgent`。

---

## 19. License

MIT OR Apache-2.0（与各子项目 Package 声明一致）。请根据企业合规要求选择适用协议。

