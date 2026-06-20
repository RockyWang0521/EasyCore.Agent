# 🚀 EasyCore.Vector.Elasticsearch

> **EasyCore.Vector.Elasticsearch** 是 EasyCore.Agent 生态中的 Elasticsearch 向量存储实现，基于 **Elastic.Clients.Elasticsearch** 与 **dense_vector** 提供 Collection 管理、KNN 向量检索、标量过滤、混合检索等能力，适用于 RAG 知识库、语义搜索等场景。  
> An Elasticsearch dense_vector-based vector store for .NET, designed for RAG and semantic search workloads.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Elasticsearch](https://img.shields.io/badge/Elasticsearch-8%2B-005571?logo=elasticsearch)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- 中文（当前文档）
- English: [ElasticsearchREADME.us.md](ElasticsearchREADME.us.md)

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
- [13. EasyCore.Vector.Elasticsearch 详细介绍](#13-easycorevectorelasticsearch-详细介绍)
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

## 2. 架构图

### 2.1 组件关系图

![2-1-组件关系图](docs/svg/2-1-组件关系图-5d67ae37.svg)


### 2.2 一次向量检索时序

![2-2-一次向量检索时序](docs/svg/2-2-一次向量检索时序-a3c2fa45.svg)


### 2.3 存储模型

每个 Collection 在 Elasticsearch 中的组织方式：

| 层级 | 命名规则 | 说明 |
|---|---|---|
| Index | `ToIndexName(collectionName)` | Collection 名经小写与字符规范化后映射为 ES Index |
| Document `_id` | `Record.Id` | 文档主键，Upsert 时作为 Elasticsearch 文档 ID |
| 向量字段 | `dense_vector` | 支持 Cosine / L2 / Inner Product 相似度 |
| 文本字段 | `Content` + `Content.keyword` | 全文检索与精确/通配符过滤 |

每条记录以 **Elasticsearch Document** 形式存储，内置字段 `Id`、`Content`，以及自定义标量字段与 `dense_vector` 向量字段。

---

## 3. 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查；Index 已存在时 `CreateCollectionAsync` 直接跳过。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 Index API 按 `_id` 覆盖写入。
- 🔍 **KNN 向量检索**：基于 Elasticsearch `dense_vector` + KNN 查询，支持 Cosine / L2 / Inner Product 三种相似度度量。
- 🧮 **标量过滤**：向量检索与纯标量 Query 均支持 Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🔀 **混合检索（Hybrid Search）**：将向量检索结果与外部 BM25/关键词候选按权重融合，提升召回质量。
- 🧱 **强类型 Record 映射**：继承 `ElasticsearchVectorRecord` 即可自动映射标量字段；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCoreElasticsearch(...)` 扩展方法注册 Options 与 `IElasticsearchVectorStore`。

---

## 4. 环境要求

### 4.1 Elasticsearch 版本

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

### 4.2 .NET 版本

- .NET 8.0 及以上

### 4.3 NuGet 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| `Elastic.Clients.Elasticsearch` | 8.15.6 | 官方 .NET 客户端，Index / Search / KNN |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.2 | DI 扩展 |

---

## 5. 快速开始

### 5.1 安装包

```bash
dotnet add package EasyCore.Vector.Elasticsearch
```

或在解决方案中直接引用项目：

```xml
<ProjectReference Include="..\EasyCore.Vector.Elasticsearch\EasyCore.Vector.Elasticsearch.csproj" />
```

### 5.2 注册服务

```csharp
using EasyCore.Vector.Elasticsearch;

builder.Services.EasyCoreElasticsearch(options =>
{
    options.Url = "http://localhost:9200";
    // options.UserName = "elastic";   // 可选，Basic 认证
    // options.Password = "your_password";
});
```

### 5.3 定义向量实体

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

### 5.4 创建 Collection 并写入数据

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

### 5.5 向量检索

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

## 6. 配置说明

### 6.1 `ElasticsearchOptions`

| 字段 | 类型 | 说明 | 示例 |
|---|---|---|---|
| `Url` | `string` | Elasticsearch 服务地址（**必填**） | `http://localhost:9200` |
| `UserName` | `string?` | Basic 认证用户名（可选） | `elastic` |
| `Password` | `string?` | Basic 认证密码（可选） | `your_password` |

当 `UserName` 非空时，SDK 自动启用 Basic Authentication；`Password` 未设置时按空字符串处理。

### 6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `ElasticsearchOptions` | Singleton | 配置快照 |
| `IElasticsearchVectorStore` | Scoped | 向量存储操作入口 |

---

## 7. 数据模型与 Collection 设计

### 7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `ElasticsearchVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors` |
| `ElasticsearchVectorCollectionDefinition` | Collection Schema 定义 |
| `ElasticsearchVectorFieldDefinition` | 向量字段（维度、度量、索引类型） |
| `ElasticsearchScalarFieldDefinition` | 标量字段（类型、是否主键） |
| `ElasticsearchVectorSearchOptions` | 向量检索参数 |
| `ElasticsearchVectorFilter` | 过滤条件容器 |
| `ElasticsearchVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 7.2 内置字段

创建 Collection 时，SDK 会自动追加以下字段，**无需**在业务定义中重复声明：

| 字段 | 类型 | 说明 |
|---|---|---|
| `Id` | `Keyword`（主键） | 文档 ID，对应 Elasticsearch `_id` |
| `Content` | `Text` + `Content.keyword` | 文本内容，支持全文与关键词过滤 |

### 7.3 向量字段配置

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

### 7.4 标量字段类型

| `ScalarFieldType` | Elasticsearch 映射 |
|---|---|
| `Bool` | `boolean` |
| `Int8` ~ `Int64` | `long` |
| `Float` / `Double` | `double` |
| `String` / `VarChar` | `keyword` |
| `Json` | `object` |

### 7.5 命名约束

Collection 名与字段名必须符合标识符规则：

```
^[A-Za-z_][A-Za-z0-9_]*$
```

例如：`test_collection`、`DocumentId` ✅；`test-collection`、`123abc` ❌。

**Index 名称规范化**：Collection 名会经 `ToIndexName` 转为小写 Elasticsearch Index 名，非法字符替换为 `_`，并以 `idx_` 前缀处理边界情况。业务代码中始终使用原始 `collectionName` 传参即可。

---

## 8. API 使用示例

以下示例均基于 `IElasticsearchVectorStore`，接口继承关系为：

```
IElasticsearchVectorStore
  └── IVectorStore
        └── IElasticsearchVectorSearch
              └── IElasticsearchHybridSearch
```

### 8.1 Collection 管理

```csharp
// 检查 Collection 是否存在
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（Index 已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection（删除整个 Index）
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
var record = await _vectorStore.GetAsync<ElasticsearchTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.4 标量 Query（不含向量相似度）

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

### 8.5 向量检索（带 Filter）

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

### 8.6 混合检索（Hybrid Search）

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

### 8.7 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<ElasticsearchTextVector>("test_collection", "contentVector", vector);
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
| `Contains` | 文本包含（通配符） | 文本 | `Content` 包含 `"RAG"` |
| `In` | 多值匹配（OR） | 数值 / 文本 / 布尔 | `Index in (1,2,3)` |

多个 Condition 之间为 **AND** 关系（Bool `must` 组合）。`In` 运算符内部为 OR。

> `Content` 字段过滤自动路由至 `Content.keyword` 子字段；`Contains` 使用大小写不敏感通配符查询。

### 9.2 `ElasticsearchVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限（KNN `k`） |
| `ScoreThreshold` | `null` | 相似度阈值，映射为 ES `min_score` |
| `Filter` | `null` | KNN 检索前置过滤条件 |
| `MetricType` | `Cosine` | 度量类型（与 Index Mapping 一致） |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

### 9.3 向量检索执行流程

1. 将 `collectionName` 规范化为 Elasticsearch Index 名；
2. 根据 `Filter` 构建 Bool / Term / Range / Wildcard 查询；
3. 构建 `KnnSearch`：`k = Limit`，`num_candidates = max(Limit * 10, Limit)`；
4. 若存在 Filter，附加至 KNN `filter` 子句；
5. 设置 `min_score`（当 `ScoreThreshold` 有值时）；
6. 解析 `_source` 并映射为强类型 `TRecord`；
7. 按 `_score` 降序返回结果。

---

## 10. 与 EasyCore.Agent.RAG 集成

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

## 11. 最佳实践

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

## 12. FAQ

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

## 13. EasyCore.Vector.Elasticsearch 详细介绍

### 13.1 设计目标

`EasyCore.Vector.Elasticsearch` 的核心目标是：在 .NET 应用中提供**生产可用**的 Elasticsearch 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：自动补全 `Id` / `Content` 字段，校验主键与字段重名；
2. **类型映射**：通过反射读写 Document 字段，支持常见标量类型与枚举；
3. **检索表达**：屏蔽 KNN + Bool Filter DSL 细节；
4. **可组合性**：向量检索、标量 Query、Hybrid 融合分层接口，便于扩展。

### 13.2 接口分层

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

### 13.3 典型落地步骤

1. 部署 Elasticsearch 8+，配置 `Url`（及认证信息）；
2. 调用 `EasyCoreElasticsearch` 注册 DI；
3. 定义 `ElasticsearchVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync` 确保 Index 存在；
5. 文档切块 → Embedding → `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 13.4 与其他向量后端对比（选型参考）

| 维度 | Elasticsearch | 说明 |
|---|---|---|
| 部署复杂度 | 中 | 需 ES 8+ 集群，但生态成熟 |
| 向量规模 | 中大型 | 适合百万级以上 Chunk |
| 混合检索 | 支持 | 原生 BM25 + 外部候选融合 |
| 全文检索 | 强 | `Content` 天然支持全文与关键词 |
| 生态一致性 | 高 | 与 EasyCore 其他 `IVectorStore` 用法一致 |

---

## 14. Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 Elasticsearch 向量库 API 示例。

### 14.1 启动 Elasticsearch

```bash
docker run -d --name elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  docker.elastic.co/elasticsearch/elasticsearch:8.15.0
```

### 14.2 启动 Demo

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

### 14.3 相关 API 端点

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

## 📄 License

MIT OR Apache-2.0（与 EasyCore.Agent 主仓库保持一致）
