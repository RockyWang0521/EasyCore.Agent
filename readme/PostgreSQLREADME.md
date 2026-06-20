# 🚀 EasyCore.Vector.PostgreSQL

> **EasyCore.Vector.PostgreSQL** 是 EasyCore.Agent 生态中的 PostgreSQL 向量存储实现，基于 **Npgsql 10 + pgvector 0.3.2** 提供 Collection 管理、向量相似度检索、标量过滤、混合检索等能力，适用于 RAG 知识库、语义搜索等场景。  
> A PostgreSQL / pgvector-based vector store for .NET, designed for RAG and semantic search workloads.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-pgvector-336791?logo=postgresql)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- 中文（当前文档）
- English: [PostgreSQLREADME.us.md](PostgreSQLREADME.us.md)

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
- [13. EasyCore.Vector.PostgreSQL 详细介绍](#13-easycorevectorpostgresql-详细介绍)
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

## 2. 架构图

### 2.1 组件关系图

![2-1-组件关系图](docs/svg/2-1-组件关系图-c7ad4952.svg)


### 2.2 一次向量检索时序

![2-2-一次向量检索时序](docs/svg/2-2-一次向量检索时序-1d64b161.svg)


### 2.3 存储模型

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

## 3. 核心特性

- 🗂️ **Collection 生命周期管理**：创建、删除、存在性检查；删除 Collection 即 `DROP TABLE`。
- 📥 **Upsert 写入**：支持单条与批量 Upsert，基于 `ON CONFLICT (Id) DO UPDATE` 实现幂等写入。
- 🔍 **向量相似度检索**：基于 pgvector 距离运算符，支持 Cosine / L2 / Inner Product 三种度量。
- 🧮 **标量过滤**：向量检索与纯标量 Query 均支持 Filter，运算符包括 `Equal`、`NotEqual`、比较运算、`Contains`、`In`。
- 🔀 **混合检索（Hybrid Search）**：将向量检索结果与 BM25/关键词候选按权重融合，提升召回质量。
- 🧱 **强类型 Record 映射**：继承 `PostgreSqlVectorRecord` 即可自动映射标量字段；向量通过 `SetVector` / `GetVector` 管理。
- ⚡ **同步 / 异步双 API**：所有核心方法均提供 `Async` 与同步版本。
- 🔌 **DI 一键注册**：`EasyCorePostgreSql(...)` 扩展方法注册 Options 与 `IPostgreSqlVectorStore`。

---

## 4. 环境要求

### 4.1 PostgreSQL 与 pgvector

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

### 4.2 .NET 版本

- .NET 8.0 及以上

### 4.3 NuGet 依赖

| 包 | 版本 | 用途 |
|---|---|---|
| `Npgsql` | 10.x | PostgreSQL 连接与 SQL 执行 |
| `Pgvector` | 0.3.2 | pgvector 类型与向量运算支持 |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.x | DI 扩展 |

---

## 5. 快速开始

### 5.1 安装包

```bash
dotnet add package EasyCore.Vector.PostgreSQL
```

或在解决方案中直接引用项目：

```xml
<ProjectReference Include="..\EasyCore.Vector.PostgreSQL\EasyCore.Vector.PostgreSQL.csproj" />
```

### 5.2 注册服务

```csharp
using EasyCore.Vector.PostgreSQL;

builder.Services.EasyCorePostgreSql(options =>
{
    options.ConnectionString =
        "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=your_password;";
});
```

### 5.3 定义向量实体

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

### 5.4 创建 Collection 并写入数据

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

### 5.5 向量检索

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

## 6. 配置说明

### 6.1 `PostgreSqlOptions`

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

### 6.2 DI 生命周期

| 服务 | 生命周期 | 说明 |
|---|---|---|
| `PostgreSqlOptions` | Singleton | 配置快照 |
| `IPostgreSqlVectorStore` | Scoped | 向量存储操作入口，内部持有 `NpgsqlDataSource` |

---

## 7. 数据模型与 Collection 设计

### 7.1 核心类型一览

| 类型 | 说明 |
|---|---|
| `PostgreSqlVectorRecord` | 向量记录基类，含 `Id`、`Content`、`Vectors` |
| `PostgreSqlVectorCollectionDefinition` | Collection Schema 定义 |
| `PostgreSqlVectorFieldDefinition` | 向量字段（维度、度量、索引类型） |
| `PostgreSqlScalarFieldDefinition` | 标量字段（类型、是否主键） |
| `PostgreSqlVectorSearchOptions` | 向量检索参数 |
| `PostgreSqlVectorFilter` | 过滤条件容器 |
| `PostgreSqlVectorSearchResult<TRecord>` | 检索结果（Record + Score） |

### 7.2 内置字段

创建 Collection 时，SDK 会自动追加以下字段，**无需**在业务定义中重复声明：

| 字段 | PostgreSQL 类型 | 说明 |
|---|---|---|
| `Id` | `VARCHAR(128) PRIMARY KEY` | 主键，Upsert 冲突键 |
| `Content` | `VARCHAR(65535)` | 文本内容，可用于关键词过滤 |

### 7.3 向量字段配置

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

### 7.4 标量字段类型

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

### 7.5 命名约束

Collection 名与字段名必须符合标识符规则：

```
^[A-Za-z_][A-Za-z0-9_]*$
```

例如：`test_collection`、`DocumentId` ✅；`test-collection`、`123abc` ❌。

> Collection 名会映射为 PostgreSQL 表名。`CollectionExistsAsync` 以小写形式查询 `information_schema.tables`，建议统一使用小写 Collection 名（如 `test_collection`）。

---

## 8. API 使用示例

以下示例均基于 `IPostgreSqlVectorStore`，接口继承关系为：

```
IPostgreSqlVectorStore
  └── IVectorStore
        └── IPostgreSqlVectorSearch
              └── IPostgreSqlHybridSearch
```

### 8.1 Collection 管理

```csharp
// 检查 Collection 是否存在（查询 public schema 下对应表）
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// 创建 Collection（表已存在则跳过）
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// 删除 Collection（DROP TABLE）
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 写入与删除

```csharp
// 单条 Upsert（ON CONFLICT DO UPDATE）
await _vectorStore.UpsertAsync("test_collection", record);

// 批量 Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// 按 Id 删除
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.3 按 Id 获取

```csharp
var record = await _vectorStore.GetAsync<PostgreSqlTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.4 标量 Query（不含向量相似度）

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

### 8.5 向量检索（带 Filter）

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

### 8.6 混合检索（Hybrid Search）

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

### 8.7 同步 API

所有 `Async` 方法均提供同步版本，例如：

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<PostgreSqlTextVector>("test_collection", "contentVector", vector);
```

> 建议在 ASP.NET Core 业务代码中优先使用异步 API，避免阻塞线程池。

---

## 9. 过滤与检索能力详解

### 9.1 支持的 Filter 运算符

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

### 9.2 `PostgreSqlVectorSearchOptions` 参数

| 字段 | 默认值 | 说明 |
|---|---|---|
| `Limit` | `10` | 返回结果数量上限 |
| `ScoreThreshold` | `null` | 相似度阈值，低于此分数的结果被过滤 |
| `Filter` | `null` | 检索前过滤条件 |
| `MetricType` | `Cosine` | 分数转换使用的度量类型 |
| `IncludeVector` | `false` | 是否在结果中包含向量数据 |
| `IncludeMetadata` | `true` | 是否包含自定义标量字段 |

### 9.3 向量检索执行流程

1. 根据 `Filter` 构建参数化 `WHERE` 子句；
2. 在内层子查询中计算 Score 表达式（基于 pgvector 距离运算符）；
3. 外层应用 `ScoreThreshold` 过滤；
4. 按 Score 降序排序并截取 `Limit` 条；
5. 通过反射将行映射为强类型 `TRecord`。

---

## 10. 与 EasyCore.Agent.RAG 集成

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

## 11. 最佳实践

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

## 12. FAQ

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

## 13. EasyCore.Vector.PostgreSQL 详细介绍

### 13.1 设计目标

`EasyCore.Vector.PostgreSQL` 的核心目标是：在 .NET 应用中提供**生产可用**的 PostgreSQL 向量存储封装，并与 EasyCore 其他向量后端保持 API 一致，使 RAG 业务代码可以跨存储引擎迁移。

重点解决：

1. **Schema 管理**：自动补全 `Id` / `Content` 字段，校验主键与字段重名，自动创建 pgvector 扩展；
2. **类型映射**：通过反射读写表列，支持常见标量类型与枚举；
3. **检索表达**：屏蔽 pgvector 距离运算符与参数化 SQL 拼接细节；
4. **可组合性**：向量检索、标量 Query、Hybrid 融合分层接口，便于扩展。

### 13.2 接口分层

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

### 13.3 典型落地步骤

1. 部署 PostgreSQL + pgvector（Docker 或云托管），配置 `ConnectionString`；
2. 调用 `EasyCorePostgreSql` 注册 DI；
3. 定义 `PostgreSqlVectorRecord` 子类映射业务字段；
4. 启动时 `CreateCollectionAsync` 确保表与索引存在；
5. 文档切块 → Embedding → `UpsertBatchAsync` 入库；
6. 用户提问 → Embedding → `VectorSearchAsync` 召回；
7. 结合 `EasyCore.Agent.RAG` 做 MMR / Rerank；
8. 将召回内容注入 Agent 上下文生成答案。

### 13.4 与其他向量后端对比（选型参考）

| 维度 | PostgreSQL + pgvector | 说明 |
|---|---|---|
| 部署复杂度 | 低 | 若已有 PostgreSQL，安装扩展即可 |
| 向量规模 | 中大型 | HNSW/IVFFlat 支持百万级向量 |
| 混合检索 | 支持 | 需自行提供 BM25 候选分数 |
| 事务/关系型 | 强 | 向量与业务数据可同库事务 |
| SQL 生态 | 强 | 可直接用 SQL 分析、备份、复制 |
| 生态一致性 | 高 | 与 EasyCore 其他 `IVectorStore` 用法一致 |

---

## 14. Demo 运行

仓库内置 `AspCoreAgent` Demo，包含完整的 PostgreSQL 向量库 API 示例。

### 14.1 启动 PostgreSQL + pgvector

```bash
docker run -d \
  --name pgvector \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=Q123456 \
  -e POSTGRES_DB=vector_db \
  -p 5432:5432 \
  pgvector/pgvector:pg17
```

### 14.2 配置连接字符串

在 `demo/AspCoreAgent/Program.cs` 中确认连接字符串与 Docker 配置一致：

```csharp
builder.Services.EasyCorePostgreSql(options =>
{
    options.ConnectionString =
        "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=Q123456;";
});
```

### 14.3 启动 Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 14.4 相关 API 端点

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

## 📄 License

MIT OR Apache-2.0（与 EasyCore.Agent 主仓库保持一致）
