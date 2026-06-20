# 🚀 EasyCore.Vector.Redis

> **EasyCore.Vector.Redis** 是 EasyCore.Agent 生态中的 Redis 向量存储实现，基于 **Redis Stack + RediSearch** 提供 Collection 管理、向量相似度检索、标量过滤、混合检索等能力，适用于 RAG 知识库、语义搜索等场景。  
> A Redis Stack / RediSearch-based vector store for .NET, designed for RAG and semantic search workloads.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Redis](https://img.shields.io/badge/Redis-Stack-red?logo=redis)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- 中文（当前文档）
- English: [RedisREADME.us.md](RedisREADME.us.md)

---

## 📚 目录

- [1. 项目简介](#1-项目简介)
- [2. 架构图](#2-架构图)
- [3. 核心特性](#3-核心特性)
- [4. 环境要求](#4-环境要求)
- [5. 快速开始](#5-快速开始)
- [6. 配置说明](#6-配置说明)
- [7. 数据模型与 Collection 设计](#7-数据模型与-collection-设计)
- [8. API 使用示例](#8-api-使用示例)
- [9. 过滤与检索能力详解](#9-过滤与检索能力详解)
- [10. 与 EasyCore.Agent.RAG 集成](#10-与-easycoreagentrag-集成)
- [11. 最佳实践](#11-最佳实践)
- [12. FAQ](#12-faq)
- [13. EasyCore.Vector.Redis 详细介绍](#13-easycorevectorredis-详细介绍)
- [14. Demo 运行](#14-demo-运行)

---

## 1. 项目简介

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

## 2. 架构图

### 2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-b4df3e64.svg)


### 2.2 一次向量检索时序

![2-2-一次向量检索时序](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-一次向量检索时序-2124a1eb.svg)


### 2.3 存储模型

每个 Collection 在 Redis 中的组织方式：

| 层级 | 命名规则 | 说明 |
|---|---|---|
| Index | `{collectionName}:idx` | RediSearch 索引名 |
| Key 前缀 | `{collectionName}:` | 所有文档 Hash 的统一前缀 |
| 文档 Key | `{collectionName}:{id}` | 单条记录的 Redis Hash Key |

每条记录以 **Redis Hash** 形式存储，内置字段 `Id`、`Content`，以及自定义标量字段与向量字段（二进制 FLOAT32 数组）。

---

## 3. 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查，删除 Collection 时同步清理 Index 与文档 Key。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 Hash 覆盖写入。
- 🔍 **KNN 向量检索**：基于 RediSearch Dialect 2 的 `[KNN]` 语法，支持 Cosine / L2 / Inner Product 三种距离度量。
- 🧮 **标量过滤**：向量检索与纯标量 Query 均支持 Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🔀 **混合检索（Hybrid Search）**：将向量检索结果与 BM25/关键词候选按权重融合，提升召回质量。
- 🧱 **强类型 Record 映射**：继承 `RedisVectorRecord` 即可自动映射标量字段；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCoreRedis(...)` 扩展方法注册连接、Options 与 `IRedisVectorStore`。

---

## 4. 环境要求

### 4.1 Redis 版本

需要 **Redis Stack**（包含 RediSearch 与 Vector 模块），而非普通 Redis 单机版。

推荐部署方式：

```bash
# Docker 快速启动 Redis Stack
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack:latest
```

### 4.2 .NET 版本

- .NET 8.0 及以上

### 4.3 NuGet 依赖

| 包 | 用途 |
|---|---|
| `StackExchange.Redis` | Redis 连接与 Hash 操作 |
| `NRedisStack` | RediSearch / Vector 命令封装 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI 扩展 |

---

## 5. 快速开始

### 5.1 安装包

```bash
dotnet add package EasyCore.Vector.Redis
```

### 5.2 注册服务

```csharp
using EasyCore.Vector.Redis;

builder.Services.EasyCoreRedis(options =>
{
    options.ConnectionString = "localhost:6379";
    // options.DefaultDatabase = 0; // 可选，指定 DB 索引
});
```

### 5.3 定义向量实体

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

### 5.4 创建 Collection 并写入数据

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

### 5.5 向量检索

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

## 6. 配置说明

### 6.1 `RedisOptions`

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

### 6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `RedisOptions` | Singleton | 配置快照 |
| `IConnectionMultiplexer` | Singleton | Redis 连接复用 |
| `IRedisVectorStore` | Scoped | 向量存储操作入口 |

---

## 7. 数据模型与 Collection 设计

### 7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `RedisVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors` |
| `RedisVectorCollectionDefinition` | Collection Schema 定义 |
| `RedisVectorFieldDefinition` | 向量字段（维度、度量、索引类型） |
| `RedisScalarFieldDefinition` | 标量字段（类型、是否建索引） |
| `RedisVectorSearchOptions` | 向量检索参数 |
| `RedisVectorFilter` | 过滤条件容器 |
| `RedisVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 7.2 内置字段

创建 Collection 时，SDK 会自动追加以下字段，**无需**在业务定义中重复声明：

| 字段 | 类型 | 说明 |
|---|---|---|
| `Id` | `VarChar(128)` | 主键，对应 Redis Hash Key 后缀 |
| `Content` | `VarChar(65535)` | 文本内容，可用于关键词过滤 |

### 7.3 向量字段配置

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

### 7.4 标量字段类型

| `ScalarFieldType` | RediSearch 映射 |
|---|---|
| `Bool` | Tag Field |
| `String` / `VarChar` / `Json` | Text Field |
| `Int8` ~ `Int64` / `Float` / `Double` | Numeric Field |

### 7.5 命名约束

Collection 名与字段名必须符合标识符规则：

```
^[A-Za-z_][A-Za-z0-9_]*$
```

例如：`test_collection`、`DocumentId` ✅；`test-collection`、`123abc` ❌。

---

## 8. API 使用示例

以下示例均基于 `IRedisVectorStore`，接口继承关系为：

```
IRedisVectorStore
  └── IVectorStore
        └── IRedisVectorSearch
              └── IRedisHybridSearch
```

### 8.1 Collection 管理

```csharp
// 检查 Collection 是否存在
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection（删除 Index + 所有文档 Key）
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 写入与删除

```csharp
// 单条 Upsert
await _vectorStore.UpsertAsync("test_collection", record);

// 批量 Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// 按 Id 删除
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.3 按 Id 获取

```csharp
var record = await _vectorStore.GetAsync<RedisTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.4 标量 Query（不含向量相似度）

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

### 8.5 向量检索（带 Filter）

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

### 8.6 混合检索（Hybrid Search）

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

### 8.7 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<RedisTextVector>("test_collection", "contentVector", vector);
```

> 建议在 ASP.NET Core 业务代码中优先使用异步 API，避免阻塞线程池。

---

## 9. 过滤与检索能力详解

### 9.1 支持的 Filter 运算符

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

### 9.2 `RedisVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限 |
| `ScoreThreshold` | `null` | 相似度阈值，低于此分数的结果被过滤 |
| `Filter` | `null` | 检索前过滤条件 |
| `MetricType` | `Cosine` | 分数转换使用的度量类型 |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

### 9.3 向量检索执行流程

1. 根据 `Filter` 构建 RediSearch 过滤表达式；
2. 拼接 KNN 子句：`(filter)=>[KNN {Limit} @{vectorName} $queryVector AS score]`；
3. 使用 Dialect 2 执行 Search；
4. 将 distance 转换为统一 Score；
5. 应用 `ScoreThreshold` 过滤；
6. 按 Score 降序排序并截取 `Limit` 条。

---

## 10. 与 EasyCore.Agent.RAG 集成

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

## 11. 最佳实践

- ✅ **Embedding 维度与 Schema 严格一致**：`RedisVectorFieldDefinition.Dimension` 必须等于模型输出维度，否则写入或检索会失败。
- ✅ **Collection 只创建一次**：`CreateCollectionAsync` 在 Index 已存在时会直接返回，建议在应用启动或首次导入前调用。
- ✅ **生产环境使用 Redis Stack 集群或云托管**：确保 RediSearch Vector 模块可用，并配置持久化（AOF/RDB）。
- ✅ **合理设置 `ScoreThreshold`**：过滤低质量召回，减少 LLM 上下文噪声。
- ✅ **大批量写入使用 `UpsertBatchAsync`**：减少往返次数；超大批量建议自行分批。
- ✅ **Hybrid Search 中 BM25 分数需归一化语义**：SDK 内部会按最大值归一化，但上游 BM25 分数应具有可比性。
- ✅ **敏感数据不要写入 `Content` 明文**：必要时在入库前加密或脱敏。
- ⚠️ **避免频繁 DeleteCollection**：`DeleteCollectionAsync` 会扫描并删除所有 `{collection}:*` Key，大数据量下可能耗时较长。

---

## 12. FAQ

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

## 13. EasyCore.Vector.Redis 详细介绍

### 13.1 设计目标

`EasyCore.Vector.Redis` 的核心目标是：在 .NET 应用中提供**生产可用**的 Redis 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：自动补全 `Id` / `Content` 字段，校验主键与字段重名；
2. **类型映射**：通过反射读写 Hash 字段，支持常见标量类型与枚举；
3. **检索表达**：屏蔽 RediSearch KNN + Filter 语法细节；
4. **可组合性**：向量检索、标量 Query、Hybrid 融合分层接口，便于扩展。

### 13.2 接口分层

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

### 13.3 典型落地步骤

1. 部署 Redis Stack，配置 `ConnectionString`；
2. 调用 `EasyCoreRedis` 注册 DI；
3. 定义 `RedisVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync` 确保 Index 存在；
5. 文档切块 → Embedding → `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 13.4 与其他向量后端对比（选型参考）

| 维度 | Redis | 说明 |
|---|---|---|
| 部署复杂度 | 低 | 若已有 Redis Stack，可直接复用 |
| 向量规模 | 中小型 | 适合百万级以内 Chunk |
| 混合检索 | 支持 | 需自行提供 BM25 候选分数 |
| 事务/多模 | 强 | Hash + Search + Cache 一体 |
| 生态一致性 | 高 | 与 EasyCore 其他 `IVectorStore` 用法一致 |

---

## 14. Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 Redis 向量库 API 示例。

### 14.1 启动 Redis Stack

```bash
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack:latest
```

### 14.2 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 14.3 相关 API 端点

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

## 📄 License

MIT OR Apache-2.0（与 EasyCore.Agent 主仓库保持一致）
