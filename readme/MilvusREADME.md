# 🚀 EasyCore.Vector.Milvus

> **EasyCore.Vector.Milvus** 是 EasyCore.Agent 生态中的 Milvus 向量存储实现，基于 **Milvus 2.x + Milvus.Client** 提供 Collection 管理、向量相似度检索、标量过滤、混合检索，以及 Flush / Load / Release 等 Milvus 生命周期管理能力。  
> A Milvus-based vector store for .NET, designed for RAG and large-scale semantic search.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Milvus](https://img.shields.io/badge/Milvus-2.x-green)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- 中文（当前文档）
- English: [MilvusREADME.us.md](MilvusREADME.us.md)

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
- [9. Milvus 生命周期管理](#9-milvus-生命周期管理)
- [10. 过滤与检索能力详解](#10-过滤与检索能力详解)
- [11. 与 EasyCore.Agent.RAG 集成](#11-与-easycoreagentrag-集成)
- [12. 最佳实践](#12-最佳实践)
- [13. FAQ](#13-faq)
- [14. Demo 运行](#14-demo-运行)

---

## 1. 项目简介

**EasyCore.Vector.Milvus** 封装 Milvus 底层 SDK，提供与 EasyCore 其他向量后端一致的强类型 API，适用于大规模向量检索与 RAG 知识库场景。

### 📦 在项目中的位置

```
EasyCore.Agent → EasyCore.Agent.RAG → EasyCore.Vector.*
                                            └── EasyCore.Vector.Milvus（本文档）
```

---

## 2. 架构图

![2-架构图](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-架构图-ef6518fd.svg)


---

## 3. 核心特性

- 🗂️ Collection 生命周期：创建、删除、存在性检查
- 📥 Upsert 单条/批量写入
- 🔍 KNN 向量检索 + 标量 Filter
- 🔀 Hybrid Search（向量 + 外部 BM25 候选融合）
- ⚙️ **Milvus 专有**：`FlushAsync`、`LoadAsync`、`ReleaseAsync`
- 🧱 强类型 `MilvusVectorRecord` 映射
- 🔌 `EasyCoreMilvus(...)` DI 注册

---

## 4. 环境要求

- .NET 8.0+
- Milvus 2.x（Standalone 或 Cluster）
- NuGet：`Milvus.Client` 2.3.0-preview.1

```bash
# Docker 快速启动 Milvus Standalone
docker run -d --name milvus -p 19530:19530 -p 9091:9091 milvusdb/milvus:latest standalone
```

---

## 5. 快速开始

### 5.1 注册服务

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

### 5.2 定义实体

```csharp
public sealed class MilvusTextVector : MilvusVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

### 5.3 创建 Collection 并检索

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

## 6. 配置说明

### 6.1 `MilvusOptions`

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Host` | `localhost` | Milvus 主机 |
| `Port` | `19530` | gRPC 端口 |
| `DatabaseName` | `default` | 数据库名 |
| `UserName` / `Password` | — | 认证 |
| `Token` | — | Token 认证 |
| `UseTls` | `false` | 是否启用 TLS |

### 6.2 DI 生命周期

| 服务 | 生命周期 |
|---|---|
| `MilvusOptions` | Singleton |
| `MilvusClient` | Singleton |
| `IMilvusVectorStore` | Scoped |

---

## 7. 数据模型与 Collection 设计

### 7.1 向量索引类型

| `MilvusVectorIndexType` | 说明 |
|---|---|
| `AutoIndex` | Milvus 自动选择（默认） |
| `Flat` | 暴力搜索 |
| `IvfFlat` | IVF_FLAT |
| `IvfSq8` | IVF_SQ8 |
| `Hnsw` | HNSW |

HNSW 参数：`M`（默认 16）、`EfConstruction`（默认 200）；IVF 参数：`NList`（默认 1024）。

### 7.2 内置字段

自动追加 `Id`（VarChar 主键）、`Content`（VarChar），无需重复声明。

### 7.3 命名约束

Collection 与字段名须符合：`^[A-Za-z_][A-Za-z0-9_]*$`

---

## 8. API 使用示例

### 8.1 Collection 管理

```csharp
await _vectorStore.CreateCollectionAsync("test_collection", definition);
var exists = await _vectorStore.CollectionExistsAsync("test_collection");
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 写入与删除

```csharp
await _vectorStore.UpsertAsync("test_collection", record);
await _vectorStore.UpsertBatchAsync("test_collection", records);
await _vectorStore.DeleteAsync("test_collection", id);
```

### 8.3 Get / Query

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

### 8.4 向量检索（带 Filter）

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

### 8.5 Hybrid Search

```csharp
var hybridResults = await _vectorStore.HybridSearchAsync(
    "test_collection", "contentVector", queryVector, bm25Results,
    options: new MilvusVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f, bm25Weight: 0.3f);
```

---

## 9. Milvus 生命周期管理

Milvus 写入后数据在 growing segment，检索前需 Load 到内存。

| 方法 | 说明 |
|---|---|
| `FlushAsync(collectionName)` | 将 growing segment 刷入 sealed segment |
| `LoadAsync(collectionName)` | 将 Collection 加载到 Query Node 内存 |
| `ReleaseAsync(collectionName)` | 从内存释放 Collection |

![9-milvus-生命周期管理](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/9-milvus-生命周期管理-0e62eac8.svg)


> 向量检索内部会自动调用 `LoadAsync`；大批量写入后建议显式 `FlushAsync`。

---

## 10. 过滤与检索能力详解

### 10.1 Filter 运算符

`Equal`、`NotEqual`、`GreaterThan`、`GreaterThanOrEqual`、`LessThan`、`LessThanOrEqual`、`Contains`、`In`

### 10.2 `MilvusVectorSearchOptions`

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回数量 |
| `ScoreThreshold` | `null` | 相似度阈值 |
| `Filter` | `null` | 标量过滤 |
| `MetricType` | `Cosine` | Milvus.Client 度量类型 |
| `IncludeVector` | `false` | 是否返回向量 |
| `IncludeMetadata` | `true` | 是否返回自定义标量字段 |

---

## 11. 与 EasyCore.Agent.RAG 集成

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

## 12. 最佳实践

- ✅ 大批量写入后调用 `FlushAsync`
- ✅ 生产环境监控 Collection Load 状态
- ✅ `Dimension` 与 Embedding 模型严格一致
- ✅ HNSW 适合在线低延迟；IVF 适合超大规模
- ⚠️ `ReleaseAsync` 后需重新 `LoadAsync` 才能检索
- ⚠️ 并行节点写入 Items 时使用不同 Key

---

## 13. FAQ

### ❓ Q1：检索无结果？
检查 Collection 是否已 Load、是否已 Flush、Filter 是否过严、维度是否匹配。

### ❓ Q2：Flush 与 Load 区别？
Flush 持久化 segment；Load 加载到内存供查询。

### ❓ Q3：AutoIndex 选什么？
由 Milvus 根据数据规模自动选择，一般无需手动指定。

---

## 14. Demo 运行

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

## 📄 License

MIT OR Apache-2.0
