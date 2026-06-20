# 🚀 EasyCore.Vector.Elasticsearch

> **EasyCore.Vector.Elasticsearch** is the Elasticsearch vector store implementation in the EasyCore.Agent ecosystem. Built on **Elastic.Clients.Elasticsearch** and **dense_vector**, it provides collection management, KNN vector search, scalar filtering, hybrid search, and more — ideal for RAG knowledge bases and semantic search.  
> An Elasticsearch dense_vector-based vector store for .NET, designed for RAG and semantic search workloads.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Elasticsearch](https://img.shields.io/badge/Elasticsearch-8%2B-005571?logo=elasticsearch)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- [中文](ElasticsearchREADME.md)
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
- [13. EasyCore.Vector.Elasticsearch in Depth](#13-easycorevectorelasticsearch-in-depth)
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

Using the Elasticsearch native API directly often requires handling index mapping construction, `dense_vector` field configuration, KNN query DSL, Bool filter composition, `_source` field pruning, and more — all of which raise the integration cost.

**EasyCore.Vector.Elasticsearch** wraps these low-level details behind a unified `IVectorStore` / `IElasticsearchVectorStore` abstraction, letting you manage vector collections with strongly typed C# models.

### 📦 Where It Fits in the Project

```
EasyCore.Agent (Agent SDK)
    └── EasyCore.Agent.RAG (RAG chunking / MMR / Rerank, etc.)
            └── EasyCore.Vector.* (vector store abstractions & backends)
                    └── EasyCore.Vector.Elasticsearch (this document)
```

It shares the same API style as other vector backends (Redis, Qdrant, Milvus, PostgreSQL), so you can switch storage engines without changing business code.

---

## 2. Architecture

### 2.1 Component Diagram

![2-1-component-diagram](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-component-diagram-50381ee1.svg)


### 2.2 Vector Search Sequence

![2-2-vector-search-sequence](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-2-vector-search-sequence-07298f97.svg)


### 2.3 Storage Model

How each collection is organized in Elasticsearch:

| Layer | Naming Rule | Description |
|---|---|---|
| Index | `ToIndexName(collectionName)` | Collection name lowercased and sanitized to an ES index |
| Document `_id` | `Record.Id` | Document primary key used as the Elasticsearch document ID |
| Vector fields | `dense_vector` | Supports Cosine / L2 / Inner Product similarity |
| Text fields | `Content` + `Content.keyword` | Full-text and exact/wildcard filtering |

Each record is stored as an **Elasticsearch document** with built-in `Id` and `Content` fields, plus custom scalar fields and `dense_vector` vector fields.

---

## 3. Core Features

- 🗂️ **Collection lifecycle management**: Create, delete, and check existence; `CreateCollectionAsync` skips if the index already exists.
- 📥 **Upsert writes**: Single and batch upsert via the Index API with `_id` overwrite.
- 🔍 **KNN vector search**: Elasticsearch `dense_vector` + KNN queries with Cosine / L2 / Inner Product metrics.
- 🧮 **Scalar filtering**: Both vector search and scalar query support filters with `Equal`, `NotEqual`, comparison operators, `Contains`, and `In`.
- 🔀 **Hybrid search**: Merge vector search results with external BM25/keyword candidates by weighted scoring.
- 🧱 **Strongly typed record mapping**: Inherit `ElasticsearchVectorRecord` for automatic scalar field mapping; manage vectors via `SetVector` / `GetVector`.
- ⚡ **Sync / async dual API**: All core methods provide both `Async` and synchronous versions.
- 🔌 **One-line DI registration**: `EasyCoreElasticsearch(...)` registers Options and `IElasticsearchVectorStore`.

---

## 4. Requirements

### 4.1 Elasticsearch Version

Requires **Elasticsearch 8.0 or later** (supports `dense_vector` indexing and KNN search).

Recommended deployment:

```bash
# Quick start with Docker (single-node, development)
docker run -d --name elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  docker.elastic.co/elasticsearch/elasticsearch:8.15.0
```

> For production, enable security and configure `UserName` / `Password` in `ElasticsearchOptions`.

### 4.2 .NET Version

- .NET 8.0 or later

### 4.3 NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Elastic.Clients.Elasticsearch` | 8.15.6 | Official .NET client for Index / Search / KNN |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.2 | DI extensions |

---

## 5. Quick Start

### 5.1 Install the Package

```bash
dotnet add package EasyCore.Vector.Elasticsearch
```

### 5.2 Register Services

```csharp
using EasyCore.Vector.Elasticsearch;

builder.Services.EasyCoreElasticsearch(options =>
{
    options.Url = "http://localhost:9200";
    // options.UserName = "elastic";   // optional, Basic auth
    // options.Password = "your_password";
});
```

### 5.3 Define a Vector Entity

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

> `ElasticsearchVectorRecord` already includes `Id`, `Content`, and `Vectors` — subclasses only need business scalar fields.

### 5.4 Create a Collection and Write Data

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

### 5.5 Vector Search

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("What features does EasyCore.Agent support?");

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

## 6. Configuration

### 6.1 `ElasticsearchOptions`

| Field | Type | Description | Example |
|---|---|---|---|
| `Url` | `string` | Elasticsearch server URL (**required**) | `http://localhost:9200` |
| `UserName` | `string?` | Basic auth username (optional) | `elastic` |
| `Password` | `string?` | Basic auth password (optional) | `your_password` |

When `UserName` is set, the SDK enables Basic Authentication; if `Password` is unset, it defaults to an empty string.

### 6.2 DI Lifetimes

| Service | Lifetime | Description |
|---|---|---|
| `ElasticsearchOptions` | Singleton | Configuration snapshot |
| `IElasticsearchVectorStore` | Scoped | Vector store operation entry point |

---

## 7. Data Model & Collection Design

### 7.1 Core Types

| Type | Description |
|---|---|
| `ElasticsearchVectorRecord` | Vector record base class with `Id`, `Content`, `Vectors` |
| `ElasticsearchVectorCollectionDefinition` | Collection schema definition |
| `ElasticsearchVectorFieldDefinition` | Vector field (dimension, metric, index type) |
| `ElasticsearchScalarFieldDefinition` | Scalar field (type, primary key flag) |
| `ElasticsearchVectorSearchOptions` | Vector search parameters |
| `ElasticsearchVectorFilter` | Filter condition container |
| `ElasticsearchVectorSearchResult<TRecord>` | Search result (Record + Score) |

### 7.2 Built-in Fields

The SDK automatically appends these fields when creating a collection — **no need** to declare them in your schema:

| Field | Type | Description |
|---|---|---|
| `Id` | `Keyword` (primary key) | Document ID, maps to Elasticsearch `_id` |
| `Content` | `Text` + `Content.keyword` | Text content for full-text and keyword filtering |

### 7.3 Vector Field Configuration

```csharp
new ElasticsearchVectorFieldDefinition
{
    Name = "contentVector",           // vector field name
    Dimension = 1024,                 // must match embedding model output dimension
    MetricType = ElasticsearchSimilarityMetricType.Cosine,  // Cosine / L2 / InnerProduct
    IndexType = ElasticsearchVectorIndexType.Hnsw,          // Hnsw / Ivfflat
    CreateIndex = true,               // whether to create a dense_vector index
    Lists = 100                       // affects ef_construction in Ivfflat mode
}
```

#### Similarity Metrics

| Enum Value | Elasticsearch Mapping | Description |
|---|---|---|
| `Cosine` | `cosine` | Cosine similarity (default, good for text embeddings) |
| `L2` | `l2_norm` | Euclidean distance |
| `InnerProduct` | `dot_product` | Inner product (best when vectors are normalized) |

#### Index Types

| Enum Value | Underlying Implementation | Description |
|---|---|---|
| `Hnsw` (default) | HNSW (`m=16`, `ef_construction=100`) | Low query latency, recommended default |
| `Ivfflat` | HNSW with higher `ef_construction` | Tune build parameters via `Lists` |

### 7.4 Scalar Field Types

| `ScalarFieldType` | Elasticsearch Mapping |
|---|---|
| `Bool` | `boolean` |
| `Int8` ~ `Int64` | `long` |
| `Float` / `Double` | `double` |
| `String` / `VarChar` | `keyword` |
| `Json` | `object` |

### 7.5 Naming Constraints

Collection and field names must match the identifier rule:

```
^[A-Za-z_][A-Za-z0-9_]*$
```

Examples: `test_collection`, `DocumentId` ✅; `test-collection`, `123abc` ❌.

**Index name normalization**: Collection names are converted to lowercase Elasticsearch index names via `ToIndexName`, with invalid characters replaced by `_` and edge cases prefixed with `idx_`. Always pass the original `collectionName` in application code.

---

## 8. API Examples

All examples below use `IElasticsearchVectorStore`. The interface hierarchy is:

```
IElasticsearchVectorStore
  └── IVectorStore
        └── IElasticsearchVectorSearch
              └── IElasticsearchHybridSearch
```

### 8.1 Collection Management

```csharp
// Check if a collection exists
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// Create collection (skips if index already exists)
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// Delete collection (removes the entire index)
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 Upsert & Delete

```csharp
// Single upsert
await _vectorStore.UpsertAsync("test_collection", record);

// Batch upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// Delete by ID
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.3 Get by ID

```csharp
var record = await _vectorStore.GetAsync<ElasticsearchTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.4 Scalar Query (no vector similarity)

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

### 8.5 Vector Search (with Filter)

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

### 8.6 Hybrid Search

Hybrid search is ideal for combined semantic + keyword ranking. BM25 candidates can come from `QueryAsync` + `Contains`, then merge with vector results:

```csharp
// 1) Keyword candidates (example: Content contains "RAG")
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

// 2) Build BM25 candidate scores (replace with real BM25 scores in production)
var bm25Results = keywordRecords
    .Select((record, index) => new ElasticsearchVectorSearchResult<ElasticsearchTextVector>
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
    options: new ElasticsearchVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f,
    bm25Weight: 0.3f);
```

The merge algorithm normalizes vector and BM25 scores separately, then computes a weighted sum for Top-K results.

### 8.7 Synchronous API

All `Async` methods have synchronous counterparts:

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<ElasticsearchTextVector>("test_collection", "contentVector", vector);
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
| `Contains` | Text contains (wildcard) | text | `Content` contains `"RAG"` |
| `In` | Multi-value match (OR) | numeric / text / bool | `Index in (1,2,3)` |

Multiple conditions are combined with **AND** (Bool `must`). The `In` operator uses OR internally.

> `Content` field filters route to the `Content.keyword` sub-field; `Contains` uses case-insensitive wildcard queries.

### 9.2 `ElasticsearchVectorSearchOptions` Parameters

| Field | Default | Description |
|---|---|---|
| `Limit` | `10` | Max results (KNN `k`) |
| `ScoreThreshold` | `null` | Similarity threshold, mapped to ES `min_score` |
| `Filter` | `null` | Pre-filter for KNN search |
| `MetricType` | `Cosine` | Metric type (must match index mapping) |
| `IncludeVector` | `false` | Include vector data in results |
| `IncludeMetadata` | `true` | Include custom scalar fields |

### 9.3 Vector Search Execution Flow

1. Normalize `collectionName` to an Elasticsearch index name;
2. Build Bool / Term / Range / Wildcard queries from `Filter`;
3. Build `KnnSearch`: `k = Limit`, `num_candidates = max(Limit * 10, Limit)`;
4. Attach filter to KNN `filter` clause when present;
5. Set `min_score` when `ScoreThreshold` is specified;
6. Parse `_source` and map to strongly typed `TRecord`;
7. Return results sorted by `_score` descending.

---

## 10. Integration with EasyCore.Agent.RAG

The `AspCoreAgent` demo wires Elasticsearch vector storage with RAG chunking and embedding end-to-end:

```csharp
// 1) Chunk the document
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) Embed and write to Elasticsearch
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

// 3) Search + MMR deduplication (EasyCore.Agent.RAG)
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

Typical RAG pipeline:

```text
Source document
  ↓ DocumentChunker
Text chunks
  ↓ Embedding model
Vectors + metadata
  ↓ UpsertAsync
Elasticsearch Vector Store
  ↓ VectorSearchAsync / HybridSearchAsync
Retrieved candidates
  ↓ MmrSelector / Reranker (EasyCore.Agent.RAG)
Refined context
  ↓ Agent ChatRunAsync
Final answer
```

---

## 11. Best Practices

- ✅ **Match embedding dimension to schema**: `ElasticsearchVectorFieldDefinition.Dimension` must equal the model output dimension.
- ✅ **Create collections once**: `CreateCollectionAsync` returns immediately if the index exists — call at startup or before first import.
- ✅ **Enable ES security in production**: Configure `UserName` / `Password` and use HTTPS endpoints.
- ✅ **Set `ScoreThreshold` appropriately**: Filter low-quality results to reduce LLM context noise.
- ✅ **Batch large writes yourself**: `UpsertBatchAsync` indexes documents one by one — split very large batches to control request pressure.
- ✅ **Normalize BM25 scores for hybrid search**: The SDK normalizes by max value, but upstream BM25 scores should be comparable.
- ✅ **Don't store sensitive data in plain `Content`**: Encrypt or redact before indexing when needed.
- ⚠️ **Avoid frequent `DeleteCollection`**: It removes the entire index — rebuilding is expensive at scale.
- ⚠️ **Index names are lowercase**: Elasticsearch index names are lowercased automatically — don't rely on case to distinguish collections.

---

## 12. FAQ

### ❓ Q1: `index_not_found_exception` error?

The collection hasn't been created or the index was deleted. Call `CreateCollectionAsync` first and ensure `collectionName` is consistent across write and search operations.

### ❓ Q2: No vector search results or very low scores?

Check:

1. Same embedding model used for indexing and querying;
2. `Dimension` and `MetricType` match the collection definition;
3. `ScoreThreshold` isn't set too high;
4. `Filter` conditions aren't too restrictive;
5. `dense_vector` index was created (`CreateIndex = true`).

### ❓ Q3: `Invalid identifier` error?

Collection and field names must match `^[A-Za-z_][A-Za-z0-9_]*$` — no hyphens or non-ASCII characters.

### ❓ Q4: Why must I pass `vectorName` when `includeVector = true`?

A record can have multiple vector fields — the SDK needs to know which one to read.

### ❓ Q5: Is collection name case-sensitive?

`collectionName` is case-sensitive at the application layer, but maps to a lowercase Elasticsearch index. `test_collection` and `Test_Collection` resolve to the same index.

### ❓ Q6: Ivfflat vs HNSW?

- **HNSW** (default): Low query latency, ideal for online search;
- **Ivfflat**: Adjust `ef_construction` via `Lists` when you need different build-time trade-offs.

### ❓ Q7: Can I use native Elasticsearch queries directly?

Yes. `IElasticsearchVectorStore` covers common vector operations; inject `ElasticsearchClient` separately for advanced full-text or aggregation scenarios.

---

## 13. EasyCore.Vector.Elasticsearch in Depth

### 13.1 Design Goals

The core goal of `EasyCore.Vector.Elasticsearch` is to provide a **production-ready** Elasticsearch vector store wrapper in .NET with API parity across EasyCore backends, so RAG business code can migrate across storage engines.

Key problems solved:

1. **Schema management**: Auto-append `Id` / `Content`, validate primary key and duplicate field names;
2. **Type mapping**: Read/write document fields via reflection, supporting common scalar types and enums;
3. **Search abstraction**: Hide KNN + Bool filter DSL details;
4. **Composability**: Layered interfaces for vector search, scalar query, and hybrid merge.

### 13.2 Interface Layers

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
  └── (marker interface for DI injection)
```

### 13.3 Typical Rollout Steps

1. Deploy Elasticsearch 8+, configure `Url` (and auth);
2. Call `EasyCoreElasticsearch` to register DI;
3. Define an `ElasticsearchVectorRecord` subclass for business fields;
4. Call `CreateCollectionAsync` at startup to ensure the index exists;
5. Chunk documents → embed → `UpsertBatchAsync`;
6. User query → embed → `VectorSearchAsync`;
7. Apply MMR / Rerank via `EasyCore.Agent.RAG`;
8. Inject retrieved context into the Agent for answer generation.

### 13.4 Backend Comparison (selection guide)

| Dimension | Elasticsearch | Notes |
|---|---|---|
| Deployment complexity | Medium | Requires ES 8+ cluster, mature ecosystem |
| Vector scale | Medium–large | Suitable for millions of chunks |
| Hybrid search | Supported | Native BM25 + external candidate merge |
| Full-text search | Strong | `Content` supports full-text and keywords |
| API consistency | High | Same `IVectorStore` usage as other EasyCore backends |

---

## 14. Running the Demo

The repository includes an `AspCoreAgent` demo with full Elasticsearch vector store API examples.

### 14.1 Start Elasticsearch

```bash
docker run -d --name elasticsearch \
  -p 9200:9200 -p 9300:9300 \
  -e "discovery.type=single-node" \
  -e "xpack.security.enabled=false" \
  docker.elastic.co/elasticsearch/elasticsearch:8.15.0
```

### 14.2 Start the Demo

Confirm the Elasticsearch URL in `Program.cs`:

```csharp
builder.Services.EasyCoreElasticsearch(options =>
{
    options.Url = "http://localhost:9200";
});
```

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 14.3 API Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/Elasticsearch/ElasticsearchVectorStoreUpsert` | Create collection and import chunked vectors |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreSearch` | Vector search + filter + score filtering |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreMmrSelector` | Vector search + MMR deduplication |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreGet` | Get record by ID |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreQuery` | Scalar query |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreHybridSearch` | Hybrid search example |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreDelete` | Delete a single record |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreCollectionExists` | Check collection existence |
| `GET /api/Elasticsearch/ElasticsearchVectorStoreDeleteCollection` | Delete entire collection |

The demo entity `ElasticsearchTextVector` includes `DocumentId`, `Index`, `StartIndex`, and `EndIndex` fields, with vector field name `documentVector`.

---

## 📄 License

MIT OR Apache-2.0 (consistent with the EasyCore.Agent main repository)
