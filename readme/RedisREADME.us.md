# 🚀 EasyCore.Vector.Redis

> **EasyCore.Vector.Redis** is the Redis vector store implementation in the EasyCore.Agent ecosystem. Built on **Redis Stack + RediSearch**, it provides collection management, vector similarity search, scalar filtering, hybrid search, and more — ideal for RAG knowledge bases and semantic search.  
> A Redis Stack / RediSearch-based vector store for .NET, designed for RAG and semantic search workloads.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Redis](https://img.shields.io/badge/Redis-Stack-red?logo=redis)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- [中文](RedisREADME.md)
- English (current document)

---

## 📚 Table of Contents

- [1. Introduction](#1-introduction)
- [2. Architecture](#2-architecture)
- [3. Core Features](#3-core-features)
- [4. Requirements](#4-requirements)
- [5. Quick Start](#5-quick-start)
- [6. Configuration](#6-configuration)
- [7. Data Model & Collection Design](#7-data-model--collection-design)
- [8. API Examples](#8-api-examples)
- [9. Filtering & Search Details](#9-filtering--search-details)
- [10. Integration with EasyCore.Agent.RAG](#10-integration-with-easycoreagentrag)
- [11. Best Practices](#11-best-practices)
- [12. FAQ](#12-faq)
- [13. EasyCore.Vector.Redis in Depth](#13-easycorevectorredis-in-depth)
- [14. Running the Demo](#14-running-the-demo)

---

## 1. Introduction

### 🎯 What Problem Does It Solve?

When building RAG (Retrieval-Augmented Generation) or semantic search systems, you typically need to:

- Chunk documents, embed them, and persist the vectors;
- Recall Top-K relevant chunks quickly by similarity;
- Filter by business fields (document ID, chunk index, tenant ID, etc.);
- Combine keyword search with vector search (Hybrid Search);
- Integrate seamlessly with the ASP.NET Core dependency injection system.

Using Redis native APIs or RediSearch commands directly often means handling index schema construction, Hash serialization, KNN query syntax, filter expression building, and other low-level details — which raises integration cost.

**EasyCore.Vector.Redis** wraps these details behind a unified `IVectorStore` / `IRedisVectorStore` abstraction, letting you create, write, search, and delete vector data with strongly typed C# models.

### 📦 Where It Fits in the Project

```
EasyCore.Agent (Agent SDK)
    └── EasyCore.Agent.RAG (chunking / MMR / rerank, etc.)
            └── EasyCore.Vector.* (vector store abstraction & backends)
                    └── EasyCore.Vector.Redis (this document)
```

It shares a consistent API style with other vector backends (Qdrant, Milvus, PostgreSQL, Elasticsearch), so you can switch storage engines by environment without changing business code.

---

## 2. Architecture

### 2.1 Component Diagram

![2-1-component-diagram](docs/svg/2-1-component-diagram-b97986d2.svg)


### 2.2 Vector Search Sequence

![2-2-vector-search-sequence](docs/svg/2-2-vector-search-sequence-1033e99e.svg)


### 2.3 Storage Model

How each collection is organized in Redis:

| Layer | Naming Rule | Description |
|---|---|---|
| Index | `{collectionName}:idx` | RediSearch index name |
| Key prefix | `{collectionName}:` | Shared prefix for all document Hashes |
| Document key | `{collectionName}:{id}` | Redis Hash key for a single record |

Each record is stored as a **Redis Hash** with built-in fields `Id` and `Content`, plus custom scalar fields and vector fields (binary FLOAT32 arrays).

---

## 3. Core Features

- 🗂️ **Collection lifecycle management**: Create, delete, and check existence; deleting a collection also removes the index and all document keys.
- 📥 **Upsert writes**: Single and batch upsert supported, based on Hash overwrite semantics.
- 🔍 **KNN vector search**: Uses RediSearch Dialect 2 `[KNN]` syntax; supports Cosine, L2, and Inner Product distance metrics.
- 🧮 **Scalar filtering**: Both vector search and scalar-only queries support filters with `Equal`, `NotEqual`, comparison operators, `Contains`, and `In`.
- 🔀 **Hybrid search**: Merge vector search results with BM25/keyword candidates by weight to improve recall quality.
- 🧱 **Strongly typed record mapping**: Inherit `RedisVectorRecord` for automatic scalar field mapping; manage vectors via `SetVector` / `GetVector`.
- ⚡ **Sync & async APIs**: Every core method has both async and synchronous versions.
- 🔌 **One-line DI registration**: The `EasyCoreRedis(...)` extension registers connection, options, and `IRedisVectorStore`.

---

## 4. Requirements

### 4.1 Redis Version

Requires **Redis Stack** (with RediSearch and Vector modules), not plain standalone Redis.

Recommended deployment:

```bash
# Quick start with Docker
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack:latest
```

### 4.2 .NET Version

- .NET 8.0 or later

### 4.3 NuGet Dependencies

| Package | Purpose |
|---|---|
| `StackExchange.Redis` | Redis connection and Hash operations |
| `NRedisStack` | RediSearch / Vector command wrappers |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | DI extensions |

---

## 5. Quick Start

### 5.1 Install the Package

```bash
dotnet add package EasyCore.Vector.Redis
```

Or reference the project directly in your solution:

```xml
<ProjectReference Include="..\EasyCore.Vector.Redis\EasyCore.Vector.Redis.csproj" />
```

### 5.2 Register Services

```csharp
using EasyCore.Vector.Redis;

builder.Services.EasyCoreRedis(options =>
{
    options.ConnectionString = "localhost:6379";
    // options.DefaultDatabase = 0; // optional: specify DB index
});
```

### 5.3 Define a Vector Entity

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

> `RedisVectorRecord` already includes `Id`, `Content`, and `Vectors`. Subclasses only need to declare business scalar fields.

### 5.4 Create a Collection and Write Data

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

### 5.5 Vector Search

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("What features does EasyCore.Agent support?");

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

## 6. Configuration

### 6.1 `RedisOptions`

| Field | Type | Description | Example |
|---|---|---|---|
| `ConnectionString` | `string` | Redis connection string (required) | `localhost:6379` |
| `DefaultDatabase` | `int?` | Default DB index; uses connection string default or `-1` if unset | `0` |

Connection strings follow the StackExchange.Redis format, for example:

```
localhost:6379
localhost:6379,password=your_password
redis.example.com:6379,ssl=true,abortConnect=false
```

### 6.2 DI Lifetimes

| Service | Lifetime | Description |
|---|---|---|
| `RedisOptions` | Singleton | Configuration snapshot |
| `IConnectionMultiplexer` | Singleton | Shared Redis connection |
| `IRedisVectorStore` | Scoped | Vector store operation entry point |

---

## 7. Data Model & Collection Design

### 7.1 Core Types

| Type | Description |
|---|---|
| `RedisVectorRecord` | Base record class with `Id`, `Content`, `Vectors` |
| `RedisVectorCollectionDefinition` | Collection schema definition |
| `RedisVectorFieldDefinition` | Vector field (dimension, metric, index type) |
| `RedisScalarFieldDefinition` | Scalar field (type, indexing options) |
| `RedisVectorSearchOptions` | Vector search parameters |
| `RedisVectorFilter` | Filter condition container |
| `RedisVectorSearchResult<TRecord>` | Search result (Record + Score) |

### 7.2 Built-in Fields

When creating a collection, the SDK automatically adds the following fields. **Do not** redeclare them in your business schema:

| Field | Type | Description |
|---|---|---|
| `Id` | `VarChar(128)` | Primary key; suffix of the Redis Hash key |
| `Content` | `VarChar(65535)` | Text content; usable for keyword filtering |

### 7.3 Vector Field Configuration

```csharp
new RedisVectorFieldDefinition
{
    Name = "contentVector",           // vector field name
    Dimension = 1024,                 // must match embedding model output dimension
    MetricType = RedisSimilarityMetricType.Cosine,  // Cosine / L2 / InnerProduct
    IndexType = RedisVectorIndexType.Hnsw,          // Hnsw / Ivfflat
    CreateIndex = true,               // whether to create a vector index
    Lists = 100                       // IVF parameter (default is fine for HNSW)
}
```

#### Similarity Metrics

| Enum | RediSearch Metric | Score Conversion |
|---|---|---|
| `Cosine` | `COSINE` | `1 - distance` (higher = more similar) |
| `L2` | `L2` | `1 / (1 + distance)` |
| `InnerProduct` | `IP` | `-distance` |

### 7.4 Scalar Field Types

| `ScalarFieldType` | RediSearch Mapping |
|---|---|
| `Bool` | Tag Field |
| `String` / `VarChar` / `Json` | Text Field |
| `Int8` ~ `Int64` / `Float` / `Double` | Numeric Field |

### 7.5 Naming Constraints

Collection and field names must match the identifier rule:

```
^[A-Za-z_][A-Za-z0-9_]*$
```

Examples: `test_collection`, `DocumentId` ✅; `test-collection`, `123abc` ❌.

---

## 8. API Examples

All examples below use `IRedisVectorStore`. Interface hierarchy:

```
IRedisVectorStore
  └── IVectorStore
        └── IRedisVectorSearch
              └── IRedisHybridSearch
```

### 8.1 Collection Management

```csharp
// Check if collection exists
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// Create collection (no-op if already exists)
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// Delete collection (removes index + all document keys)
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 Write & Delete

```csharp
// Single upsert
await _vectorStore.UpsertAsync("test_collection", record);

// Batch upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// Delete by id
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.3 Get by Id

```csharp
var record = await _vectorStore.GetAsync<RedisTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.4 Scalar Query (No Vector Similarity)

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

### 8.5 Vector Search (With Filter)

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

### 8.6 Hybrid Search

Hybrid search fits combined ranking scenarios where you want both semantic similarity and keyword hits. BM25 candidates can come from `QueryAsync` + `Contains`, then merge with vector results:

```csharp
// 1) Keyword candidates (example: Content contains "RAG")
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

// 2) Build BM25 candidate scores (replace with real BM25 scores in production)
var bm25Results = keywordRecords
    .Select((record, index) => new RedisVectorSearchResult<RedisTextVector>
    {
        Record = record,
        Score = Math.Max(0.1f, 1.0f - index * 0.08f)
    })
    .ToList();

// 3) Hybrid merge
var hybridResults = await _vectorStore.HybridSearchAsync(
    collectionName: "test_collection",
    vectorName: "contentVector",
    vector: queryVector,
    bm25Results: bm25Results,
    options: new RedisVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f,
    bm25Weight: 0.3f);
```

The merge algorithm normalizes vector and BM25 scores separately, then computes a weighted sum and returns Top-K results.

### 8.7 Synchronous API

Every `Async` method has a synchronous counterpart, for example:

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<RedisTextVector>("test_collection", "contentVector", vector);
```

> Prefer async APIs in ASP.NET Core to avoid blocking the thread pool.

---

## 9. Filtering & Search Details

### 9.1 Supported Filter Operators

| Operator | Description | Applicable Field Types | Example |
|---|---|---|---|
| `Equal` | Equals | numeric / text / bool | `DocumentId = "doc-001"` |
| `NotEqual` | Not equals | numeric / text / bool | `Index != 0` |
| `GreaterThan` | Greater than | numeric | `Index > 5` |
| `GreaterThanOrEqual` | Greater than or equal | numeric | `Index >= 1` |
| `LessThan` | Less than | numeric | `Index < 10` |
| `LessThanOrEqual` | Less than or equal | numeric | `Index <= 100` |
| `Contains` | Text contains | text | `Content` contains `"RAG"` |
| `In` | Multi-value match (OR) | numeric / text / bool | `Index in (1,2,3)` |

Multiple conditions are combined with **AND** (space-separated). The `In` operator uses OR internally.

### 9.2 `RedisVectorSearchOptions` Parameters

| Field | Default | Description |
|---|---|---|
| `Limit` | `10` | Maximum number of results |
| `ScoreThreshold` | `null` | Similarity threshold; results below are filtered out |
| `Filter` | `null` | Pre-search filter conditions |
| `MetricType` | `Cosine` | Metric used for score conversion |
| `IncludeVector` | `false` | Include vector data in results |
| `IncludeMetadata` | `true` | Include custom scalar fields |

### 9.3 Vector Search Execution Flow

1. Build a RediSearch filter expression from `Filter`;
2. Append KNN clause: `(filter)=>[KNN {Limit} @{vectorName} $queryVector AS score]`;
3. Execute search with Dialect 2;
4. Convert distance to a unified Score;
5. Apply `ScoreThreshold` filtering;
6. Sort by Score descending and take `Limit` results.

---

## 10. Integration with EasyCore.Agent.RAG

The `AspCoreAgent` demo wires Redis vector storage with RAG chunking and embedding end to end:

```csharp
// 1) Chunk the document
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) Embed and write to Redis
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

// 3) Search + MMR deduplication (EasyCore.Agent.RAG)
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

Typical RAG pipeline:

```text
Source document
  ↓ DocumentChunker
Text chunks
  ↓ Embedding model
Vectors + metadata
  ↓ UpsertAsync
Redis Vector Store
  ↓ VectorSearchAsync / HybridSearchAsync
Retrieved candidates
  ↓ MmrSelector / Reranker (EasyCore.Agent.RAG)
Refined context
  ↓ Agent ChatRunAsync
Final answer
```

---

## 11. Best Practices

- ✅ **Keep embedding dimension aligned with schema**: `RedisVectorFieldDefinition.Dimension` must match the model output dimension, or writes/searches will fail.
- ✅ **Create collections once**: `CreateCollectionAsync` returns immediately if the index already exists; call it at startup or before first import.
- ✅ **Use Redis Stack cluster or managed cloud in production**: Ensure RediSearch Vector modules are available and configure persistence (AOF/RDB).
- ✅ **Set `ScoreThreshold` appropriately**: Filter low-quality hits to reduce LLM context noise.
- ✅ **Use `UpsertBatchAsync` for bulk writes**: Fewer round trips; split very large batches yourself.
- ✅ **Ensure BM25 scores are comparable in hybrid search**: The SDK normalizes by max value, but upstream BM25 scores should be on a consistent scale.
- ✅ **Do not store sensitive data in plain `Content`**: Encrypt or redact before ingestion when needed.
- ⚠️ **Avoid frequent `DeleteCollection`**: `DeleteCollectionAsync` scans and deletes all `{collection}:*` keys, which can be slow at large scale.

---

## 12. FAQ

### ❓ Q1: `Unknown Index` or `no such index` error?

The collection has not been created or the index was deleted. Call `CreateCollectionAsync` first and ensure `collectionName` matches across write and search operations.

### ❓ Q2: Vector search returns no results or very low scores?

Check:

1. Whether the same embedding model is used for ingestion and query;
2. Whether `Dimension` and `MetricType` match the collection definition;
3. Whether `ScoreThreshold` is set too high;
4. Whether `Filter` conditions are too restrictive.

### ❓ Q3: `Invalid identifier` error?

Collection and field names must match `^[A-Za-z_][A-Za-z0-9_]*$`. Do not use hyphens or non-ASCII characters.

### ❓ Q4: Why is `vectorName` required when `includeVector = true`?

A record may contain multiple vector fields. The SDK needs to know which field's binary vector data to read.

### ❓ Q5: Can I share the connection with a regular Redis client?

Yes. `EasyCoreRedis` registers `IConnectionMultiplexer` as a Singleton; inject it in other services as needed. Watch DB index and key prefix isolation.

### ❓ Q6: How to choose between Ivfflat and HNSW?

- **HNSW** (default): Low query latency; good for online retrieval;
- **Ivfflat**: More flexible build cost and tuning; useful when recall and memory trade-offs matter.

---

## 13. EasyCore.Vector.Redis in Depth

### 13.1 Design Goals

The core goal of `EasyCore.Vector.Redis` is to provide a **production-ready** Redis vector store wrapper for .NET apps, with an API consistent across EasyCore vector backends so RAG business code can migrate across storage engines.

Key problems it addresses:

1. **Schema management**: Auto-adds `Id` / `Content` fields; validates primary key and duplicate field names;
2. **Type mapping**: Reads/writes Hash fields via reflection; supports common scalar types and enums;
3. **Search expression**: Hides RediSearch KNN + filter syntax details;
4. **Composability**: Layered interfaces for vector search, scalar query, and hybrid merge.

### 13.2 Interface Layers

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
  └── (marker interface for DI injection)
```

### 13.3 Typical Rollout Steps

1. Deploy Redis Stack and configure `ConnectionString`;
2. Register DI via `EasyCoreRedis`;
3. Define a `RedisVectorRecord` subclass for business fields;
4. Call `CreateCollectionAsync` at startup to ensure the index exists;
5. Chunk documents → embed → `UpsertBatchAsync`;
6. On user query → embed → `VectorSearchAsync`;
7. Apply MMR / rerank via `EasyCore.Agent.RAG`;
8. Inject retrieved context into the Agent and generate the answer.

### 13.4 Comparison with Other Vector Backends

| Dimension | Redis | Notes |
|---|---|---|
| Deployment complexity | Low | Reuse existing Redis Stack if available |
| Vector scale | Small to medium | Suitable for up to ~millions of chunks |
| Hybrid search | Supported | You provide BM25 candidate scores |
| Multi-model / cache | Strong | Hash + Search + Cache in one stack |
| Ecosystem consistency | High | Same usage as other EasyCore `IVectorStore` backends |

---

## 14. Running the Demo

The repository includes an `AspCoreAgent` demo with full Redis vector store API examples.

### 14.1 Start Redis Stack

```bash
docker run -d --name redis-stack -p 6379:6379 redis/redis-stack:latest
```

### 14.2 Start the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 14.3 API Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/Redis/RedisVectorStoreUpsert` | Create collection and import chunked vectors |
| `GET /api/Redis/RedisVectorStoreSearch` | Vector search + filter + score filtering |
| `GET /api/Redis/RedisVectorStoreMmrSelector` | Vector search + MMR deduplication |
| `GET /api/Redis/RedisVectorStoreGet` | Get record by id |
| `GET /api/Redis/RedisVectorStoreQuery` | Scalar query |
| `GET /api/Redis/RedisVectorStoreHybridSearch` | Hybrid search example |
| `GET /api/Redis/RedisVectorStoreDelete` | Delete a single record |
| `GET /api/Redis/RedisVectorStoreCollectionExists` | Check collection existence |
| `GET /api/Redis/RedisVectorStoreDeleteCollection` | Delete entire collection |

---

## 📄 License

MIT OR Apache-2.0 (same as the EasyCore.Agent main repository)
