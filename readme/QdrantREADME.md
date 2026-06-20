# 🚀 EasyCore.Vector.Qdrant

> **EasyCore.Vector.Qdrant** 是 EasyCore.Agent 生态中的 Qdrant 向量存储实现，基于 **Qdrant.Client 1.18.1** 提供 Collection 管理、稠密向量检索、**稀疏向量检索**、**原生 Dense + Sparse 混合检索**、标量过滤等能力，适用于 RAG 知识库、语义搜索、关键词增强召回等场景。  
> A Qdrant-based vector store for .NET, designed for RAG and semantic search workloads with first-class sparse and hybrid search support.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Qdrant](https://img.shields.io/badge/Qdrant-1.18.1-blue)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)
![Sparse](https://img.shields.io/badge/Sparse-Vector-orange)
![Hybrid](https://img.shields.io/badge/Hybrid-Search-green)

---

## 🌍 Language

- 中文（当前文档）
- English: [QdrantREADME.us.md](QdrantREADME.us.md)

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
- [13. EasyCore.Vector.Qdrant 详细介绍](#13-easycorevectorqdrant-详细介绍)
- [14. Demo 运行](#14-demo-运行)

---

## 1. 项目简介

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

## 2. 架构图

### 2.1 组件关系图

![2-1-组件关系图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-组件关系图-33cf79de.svg)


### 2.2 混合检索时序（Dense + Sparse）

![2-2-混合检索时序-dense-sparse](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-混合检索时序-dense-sparse-840ad150.svg)


### 2.3 存储模型

每个 Collection 在 Qdrant 中的组织方式：

| 层级 | 说明 |
|---|---|
| Collection | 向量集合，包含一个或多个 Named Dense Vector 及可选 Sparse Vector |
| Point | 单条记录，UUID 作为 Point Id |
| Named Vectors | 稠密向量，如 `documentVector` |
| Sparse Vectors | 稀疏向量，命名规则 `{Name}_sparse`，如 `documentVector_sparse` |
| Payload | 业务元数据：`content`、`metadata`（JSON）、`record`（完整记录 JSON）及反射出的标量字段 |

---

## 3. 核心特性

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

## 4. 环境要求

### 4.1 Qdrant 版本

需要运行 **Qdrant Server**（支持 Sparse Vector 的版本，推荐 1.7+）。

推荐部署方式：

```bash
# Docker 快速启动 Qdrant（HTTP 6333 / gRPC 6334）
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

> SDK 默认通过 **gRPC 端口 6334** 通信（非 HTTP 6333）。

### 4.2 .NET 版本

- .NET 8.0 及以上

### 4.3 NuGet 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| `Qdrant.Client` | 1.18.1 | Qdrant gRPC 客户端 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.0 | DI 扩展 |

---

## 5. 快速开始

### 5.1 安装包

```bash
dotnet add package EasyCore.Vector.Qdrant
```

### 5.2 注册服务

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

### 5.3 定义向量实体

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

### 5.4 创建 Collection 并写入数据

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

### 5.5 稠密向量检索

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

### 5.6 稀疏向量检索

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

### 5.7 混合检索（Dense + Sparse）

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

## 6. 配置说明

### 6.1 `QdrantOptions`

| 字段 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `Host` | `string` | `localhost` | Qdrant 服务器主机名或 IP |
| `GrpcPort` | `int` | `6334` | Qdrant gRPC 端口 |
| `ApiKey` | `string?` | `null` | API Key（Qdrant Cloud 等认证场景） |
| `UseHttps` | `bool` | `false` | 是否使用 HTTPS 连接 |

### 6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `QdrantOptions` | Singleton | 配置快照 |
| `QdrantClient` | Singleton | gRPC 客户端连接复用 |
| `IQdrantVectorStore` | Scoped | 向量存储操作入口 |

---

## 7. 数据模型与 Collection 设计

### 7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `QdrantVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors`、`Metadata` |
| `QdrantVectorCollectionDefinition` | Collection Schema 定义 |
| `QdrantVectorFieldDefinition` | 向量字段（维度、距离、是否启用稀疏向量） |
| `SparseVectorValue` | 稀疏向量值（`Indices` + `Values` 列表） |
| `QdrantVectorSearchOptions` | 检索参数 |
| `QdrantVectorFilter` | 过滤条件容器 |
| `QdrantQdrantVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 7.2 内置字段

每条记录在 Payload 中自动包含：

| 字段 | 说明 |
|---|---|
| `content` | 文本内容 |
| `metadata` | 标量字段 JSON 序列化 |
| `record` | 完整记录 JSON（检索反序列化用） |

业务标量属性（如 `DocumentId`、`Index`）会同时作为独立 Payload 字段写入，可直接用于 Filter。

### 7.3 向量字段配置

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

### 7.4 命名约束

- Collection 名不能为空或纯空白字符；
- 向量字段名不能为空；
- Point Id 使用 UUID 字符串格式。

---

## 8. API 使用示例

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

### 8.1 Collection 管理

```csharp
// 检查 Collection 是否存在
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection
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

### 8.3 稠密向量检索（带 Filter）

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

### 8.4 稀疏向量检索

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

### 8.5 混合检索（Dense + Sparse 加权融合）

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

### 8.6 同步 API

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
| `Contains` | 关键词匹配 | 文本 Payload | `Content` 包含 `"RAG"` |
| `In` | 多值匹配（OR） | 数值 / 文本 / 布尔 | `Index in (1,2,3)` |

多个 Condition 之间为 **AND** 关系（`Must` 连接）。`NotEqual` 映射为 `MustNot`。`In` 运算符内部为 OR。

### 9.2 `QdrantVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限 |
| `ScoreThreshold` | `null` | 相似度阈值，低于此分数的结果被 Qdrant 过滤 |
| `Filter` | `null` | 检索前 Payload 过滤条件 |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

> 与 Redis 后端不同，**无 `MetricType` 字段**——距离度量由 Collection 创建时的 `QdrantVectorFieldDefinition.Distance` 决定。

### 9.3 稠密向量检索执行流程

1. 根据 `Filter` 构建 Qdrant Payload Filter；
2. 调用 `QdrantClient.SearchAsync`，指定 Named Vector 与查询向量；
3. Qdrant 按 Collection 配置的 `Distance` 计算 Score；
4. 应用 `ScoreThreshold` 过滤；
5. 反序列化 Payload 中的 `record` JSON 为强类型 `TRecord`；
6. 按 Score 降序返回最多 `Limit` 条。

### 9.4 稀疏向量检索执行流程

1. 校验 `SparseVectorValue.Indices` 与 `Values` 长度一致；
2. 构建 Payload Filter（可选）；
3. 调用 `QdrantClient.SearchAsync`，传入 `sparseIndices` 参数；
4. Qdrant 在 Sparse Vector 索引上执行检索；
5. 返回带 Score 的强类型结果。

---

## 10. 与 EasyCore.Agent.RAG 集成

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

## 11. 最佳实践

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

## 12. FAQ

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

## 13. EasyCore.Vector.Qdrant 详细介绍

### 13.1 设计目标

`EasyCore.Vector.Qdrant` 的核心目标是：在 .NET 应用中提供**生产可用**的 Qdrant 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：Named Dense Vector + Sparse Vector 联合建表；
2. **类型映射**：通过反射读写 Payload 标量字段，JSON 序列化完整 Record；
3. **检索表达**：屏蔽 Qdrant gRPC Filter 与 Named Vector 语法细节；
4. **差异化检索**：稀疏向量检索与 Dense+Sparse 混合检索一等公民支持。

### 13.2 接口分层

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

### 13.3 典型落地步骤

1. 部署 Qdrant Server，确认 gRPC 6334 可访问；
2. 调用 `EasyCoreQdrant` 注册 DI；
3. 定义 `QdrantVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync`，按需 `EnableSparseVector`；
5. 文档切块 → 稠密 Embedding（+ 可选稀疏 Embedding）→ `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` / `HybridSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 13.4 与其他向量后端对比（选型参考）

| 维度 | Qdrant | 说明 |
|---|---|---|
| 部署复杂度 | 中 | 独立向量数据库，Docker 一键启动 |
| 向量规模 | 中大型 | HNSW 索引，适合百万~亿级 |
| 稀疏向量 | ✅ 原生支持 | `SparseSearchAsync` 一等公民 |
| 混合检索 | ✅ Dense + Sparse | SDK 加权融合，非 BM25 模式 |
| 标量 Query | ❌ SDK 未暴露 | 聚焦向量检索场景 |
| 生态一致性 | 高 | 与 EasyCore 其他向量库 Upsert/Search 用法一致 |

---

## 14. Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 Qdrant 向量库 API 示例。

### 14.1 启动 Qdrant

```bash
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

### 14.2 启动 Demo

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

### 14.3 相关 API 端点

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

## 📄 License

MIT OR Apache-2.0（与 EasyCore.Agent 主仓库保持一致）
