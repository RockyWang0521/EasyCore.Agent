# 🚀 EasyCore.Agent.RAG

> **EasyCore.Agent.RAG** 是 EasyCore.Agent 生态中的 RAG（检索增强生成）工具库，提供文档切块、Query Rewrite、Multi Query、MMR 去重等检索链路能力，可与任意向量存储后端（Redis、Qdrant、Milvus、PostgreSQL、Elasticsearch）组合使用。  
> A RAG utility library for .NET with document chunking, query rewriting, multi-query generation, and MMR selection.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![RAG](https://img.shields.io/badge/RAG-Retrieval-blueviolet)
![Agent](https://img.shields.io/badge/EasyCore-Agent-green)

---

## 🌍 Language

- 中文（当前文档）
- English: [RagREADME.us.md](RagREADME.us.md)

---

## 📚 目录

- [1. 项目简介](#1-项目简介)
- [2. 架构图](#2-架构图)
- [3. 核心特性](#3-核心特性)
- [4. 环境要求](#4-环境要求)
- [5. 快速开始](#5-快速开始)
- [6. 模块说明](#6-模块说明)
- [7. API 使用示例](#7-api-使用示例)
- [8. 完整 RAG 流水线](#8-完整-rag-流水线)
- [9. 最佳实践](#9-最佳实践)
- [10. FAQ](#10-faq)
- [11. EasyCore.Agent.RAG 详细介绍](#11-easycoreagentrag-详细介绍)
- [12. Demo 运行](#12-demo-运行)

---

## 1. 项目简介

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

## 2. 架构图

### 2.1 RAG 链路总览

![2-1-rag-链路总览](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-rag-链路总览-7d4e369a.svg)


### 2.2 各模块职责

| 模块 | 类型 | 是否依赖 LLM | 说明 |
|---|---|---|---|
| `DocumentChunker` | 静态工具 | 否 | 固定窗口 + 重叠切块 |
| `QueryRewrite` | 静态工具 | 是 | 结合会话历史改写检索 Query |
| `MultiQueryGenerator` | 静态工具 | 是 | 从一个问题生成多条检索 Query |
| `MmrSelector` | 静态工具 | 否 | 在相关性与多样性间做 MMR 平衡 |

### 2.3 Query Rewrite 时序

![2-3-query-rewrite-时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-3-query-rewrite-时序-db16607d.svg)


---

## 3. 核心特性

- 📄 **DocumentChunker**：按字符窗口切块，支持可配置 `chunkSize` 与 `overlapSize`，保留 `StartIndex` / `EndIndex` 便于溯源。
- 🔄 **QueryRewrite**：利用 `AIAgent` 将会话中的模糊问题改写为独立、可检索的 Query；自动检测语言并与用户问题保持一致。
- 🔀 **MultiQueryGenerator**：从一个用户问题生成 N 条不同角度的检索 Query，提升召回覆盖率。
- 🎯 **MmrSelector**：Maximum Marginal Relevance 算法，在保持相关性的同时降低结果重复度。
- 🧩 **Prompt 可扩展**：`QueryRewritePromptBuilder`、`MultiQueryPromptBuilder` 暴露 System / User Prompt 构建方法，便于业务定制。
- ⚡ **同步 / 异步**：`QueryRewrite`、`MultiQueryGenerator` 均提供同步与异步 API。
- 🔌 **零配置接入**：无 ServiceCollection 扩展，引用程序集即可使用。

---

## 4. 环境要求

### 4.1 .NET 版本

- .NET 8.0 及以上

### 4.2 NuGet 依赖

| 包 | 用途 |
|---|---|
| `Microsoft.Agents.AI` | `AIAgent`、`ChatMessage` 等 Agent 运行时 |
| `Microsoft.Agents.AI.OpenAI` | OpenAI 兼容模型接入（通过 EasyCore.Agent 间接使用） |

### 4.3 配合使用的组件

| 组件 | 用途 |
|---|---|
| `EasyCore.Agent` | 创建 `AIAgent`、Embedding、会话上下文 |
| `EasyCore.Vector.*` | 向量入库与相似度检索 |

---

## 5. 快速开始

### 5.1 安装包

```bash
dotnet add package EasyCore.Agent.RAG
```

### 5.2 文档切块

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

### 5.3 Query Rewrite

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

### 5.4 Multi Query

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

### 5.5 MMR 去重

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

## 6. 模块说明

### 6.1 DocumentChunker

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

### 6.2 DocumentChunk

| 属性 | 类型 | 说明 |
|---|---|---|
| `Id` | `string` | Chunk 唯一标识 |
| `DocumentId` | `string` | 来源文档 ID |
| `Index` | `int` | 在文档中的序号（从 0 开始） |
| `Content` | `string` | 切块文本 |
| `StartIndex` | `int` | 在原文中的起始字符位置 |
| `EndIndex` | `int` | 在原文中的结束字符位置 |

### 6.3 QueryRewrite

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

### 6.4 MultiQueryGenerator

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

### 6.5 MmrSelector

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

### 6.6 MmrCandidate

| 属性 | 类型 | 说明 |
|---|---|---|
| `Id` | `string` | 候选 ID |
| `Content` | `string` | 文本内容 |
| `Score` | `float` | 原始相关性分数 |
| `Vector` | `float[]` | 用于多样性计算的向量 |

---

## 7. API 使用示例

### 7.1 入库：切块 + Embedding + 向量写入

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

### 7.2 检索：Rewrite → Embed → Search

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

### 7.3 Multi Query 多路检索

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

### 7.4 MMR + Agent 回答

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

### 7.5 自定义 Prompt（QueryRewrite）

```csharp
// 直接使用 PromptBuilder 构建消息，再自行调用 Agent
var messages = QueryRewritePromptBuilder.Build(query, history);

// 或替换 System Prompt
var customSystem = QueryRewritePromptBuilder.GetSystemPrompt();
// 基于 customSystem 自行组装 messages...
```

### 7.6 自定义 Prompt（MultiQuery）

```csharp
var messages = MultiQueryPromptBuilder.Build(query, count: 5);

var systemPrompt = MultiQueryPromptBuilder.BuildSystemPrompt(count: 5);
var userPrompt = MultiQueryPromptBuilder.BuildUserPrompt(query, count: 5);
```

---

## 8. 完整 RAG 流水线

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

## 9. 最佳实践

- ✅ **`chunkSize` 与 Embedding 模型匹配**：中文建议 500~1000 字符，英文可按 token 估算；`overlapSize` 通常取 `chunkSize` 的 10%~20%。
- ✅ **Rewrite 前先积累会话历史**：通过 `EasyCore.Agent` 的 `GetChatContext(sessionId)` 获取完整 `ChatMessage` 列表。
- ✅ **Multi Query 后做结果合并去重**：按 `Record.Id` 保留最高分，避免重复 chunk 进入上下文。
- ✅ **MMR 需要向量数据**：检索时设置 `IncludeVector = true`，或将向量一并映射到 `MmrCandidate.Vector`。
- ✅ **`lambda` 调参**：知识库重复内容多时可降至 `0.5~0.6`；追求精确匹配时可提高至 `0.8~0.9`。
- ✅ **ScoreThreshold 与 MMR 配合**：先用向量库阈值过滤低分结果，再 MMR 精选。
- ⚠️ **QueryRewrite / MultiQuery 依赖 LLM**：注意 API 成本与延迟，可对简单问题跳过 Rewrite。
- ⚠️ **DocumentChunker 为字符级切块**：不感知 Markdown 标题或段落边界，长文档可考虑先按段落预分割。

---

## 10. FAQ

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

## 11. EasyCore.Agent.RAG 详细介绍

### 11.1 设计目标

`EasyCore.Agent.RAG` 聚焦 **RAG 检索链路中的可复用算法与 Prompt 封装**，而非重复实现 Agent 或向量库能力。设计原则：

1. **轻量无状态**：静态工具类，无全局配置，便于测试与组合；
2. **与存储解耦**：不引用任何 `EasyCore.Vector.*` 程序集；
3. **与 Agent 协作**：Rewrite / MultiQuery 通过标准 `AIAgent` 接口调用 LLM；
4. **企业可扩展**：Prompt Builder 公开，允许业务覆盖 System Prompt。

### 11.2 类型一览

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

### 11.3 典型落地步骤

1. 引用 `EasyCore.Agent.RAG` 与目标 `EasyCore.Vector.*`；
2. 注册 `EasyCore.Agent` 与向量库 DI；
3. 入库：`DocumentChunker` → `EmbedAsync` → `UpsertAsync`；
4. 检索：`QueryRewrite`（可选）→ `MultiQueryGenerator`（可选）→ `VectorSearchAsync`；
5. 后处理：`MmrSelector.Select` → 拼接上下文 → `ChatRunAsync` 生成答案。

---

## 12. Demo 运行

`AspCoreAgent` Demo 的 `EmbeddingController` 提供了 RAG 相关 API 示例。

### 12.1 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 12.2 RAG 相关端点

| 端点 | 说明 |
|---|---|
| `GET /api/Embedding/RagDocumentChunker` | 文档切块示例 |
| `GET /api/Embedding/RagQueryRewrite?message=...&sessionId=...` | Query Rewrite（含多轮上下文） |
| `GET /api/Embedding/RagMultiQueryRetrieval?message=...` | Multi Query 生成 |

各向量库 Controller（Redis / Qdrant / Milvus 等）中的 `*MmrSelector` 端点演示了 **向量检索 + MMR** 的组合用法。

---

## 📄 License

MIT OR Apache-2.0（与 EasyCore.Agent 主仓库保持一致）
