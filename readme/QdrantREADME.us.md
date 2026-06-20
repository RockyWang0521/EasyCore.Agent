# 🚀 EasyCore.Vector.Qdrant

> **EasyCore.Vector.Qdrant** is the Qdrant vector store implementation in the EasyCore.Agent ecosystem. Built on **Qdrant.Client 1.18.1**, it provides collection management, dense vector similarity search, **sparse vector search**, **native Dense + Sparse hybrid search**, scalar filtering, and more — ideal for RAG knowledge bases, semantic search, and keyword-enhanced recall.  
> A Qdrant-based vector store for .NET, designed for RAG and semantic search workloads with first-class sparse and hybrid search support.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Qdrant](https://img.shields.io/badge/Qdrant-1.18.1-blue)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)
![Sparse](https://img.shields.io/badge/Sparse-Vector-orange)
![Hybrid](https://img.shields.io/badge/Hybrid-Search-green)

---

## 🌍 Language

- [中文](QdrantREADME.md)
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
- [13. EasyCore.Vector.Qdrant in Depth](#13-easycorevectorqdrant-in-depth)
- [14. Running the Demo](#14-running-the-demo)

---

## 1. Introduction

### 🎯 What Problem Does It Solve?

When building RAG (Retrieval-Augmented Generation) or semantic search systems, you typically need to:

- Chunk documents, embed them, and persist the vectors;
- Recall Top-K relevant chunks quickly by similarity;
- Filter by business fields (document ID, chunk index, tenant ID, etc.);
- Combine **semantic vector search** with **sparse vector (keyword/BM25-style) search**;
- Integrate seamlessly with the ASP.NET Core dependency injection system.

Using the Qdrant gRPC API directly often means handling collection schema construction, Named Vector / Sparse Vector configuration, payload serialization, filter expression building, and hybrid search weight fusion — which raises integration cost.

**EasyCore.Vector.Qdrant** wraps these details behind a unified `IQdrantVectorStore` abstraction, letting you create, write, search, and delete vector data with strongly typed C# models.

### ⭐ Differentiators vs. Other Backends

| Capability | EasyCore.Vector.Qdrant | EasyCore.Vector.Redis, etc. |
|---|---|---|
| Sparse vector search | ✅ `SparseSearchAsync` + `SparseVectorValue` | ❌ |
| Hybrid search | ✅ Dense + Sparse weighted fusion | BM25 candidates + vector score fusion |
| Distance metric | Set at collection creation via `Distance` | `MetricType` at search time |

> **Sparse vector + native hybrid search** are the core differentiators of this library — ideal for production scenarios combining Embedding semantic recall with SPLADE/BM42-style sparse keyword enhancement.

### 📦 Where It Fits in the Project

```
EasyCore.Agent (Agent SDK)
    └── EasyCore.Agent.RAG (chunking / MMR / rerank, etc.)
            └── EasyCore.Vector.* (vector store abstraction & backends)
                    └── EasyCore.Vector.Qdrant (this document)
```

It shares a consistent API style with other vector backends (Redis, Milvus, PostgreSQL, Elasticsearch), so you can switch storage engines by environment without changing business code.

---

## 2. Architecture

### 2.1 Component Diagram

![2-1-component-diagram](docs/svg/2-1-component-diagram-acd281f2.svg)


### 2.2 Hybrid Search Sequence (Dense + Sparse)

![2-2-hybrid-search-sequence-dense-sparse](docs/svg/2-2-hybrid-search-sequence-dense-sparse-03c9f719.svg)


### 2.3 Storage Model

How each collection is organized in Qdrant:

| Layer | Description |
|---|---|
| Collection | Vector collection containing one or more Named Dense Vectors and optional Sparse Vectors |
| Point | Single record; UUID used as Point Id |
| Named Vectors | Dense vectors, e.g. `documentVector` |
| Sparse Vectors | Sparse vectors named `{Name}_sparse`, e.g. `documentVector_sparse` |
| Payload | Business metadata: `content`, `metadata` (JSON), `record` (full record JSON), and reflected scalar fields |

---

## 3. Core Features

- 🗂️ **Collection lifecycle management**: Create, delete, and check existence; supports Named Dense Vector and Sparse Vector in a single collection.
- 📥 **Upsert writes**: Single and batch upsert supported, based on Point UUID overwrite semantics.
- 🔍 **Dense vector search**: `VectorSearchAsync`; distance metric determined by `Distance` set at collection creation.
- 🧩 **Sparse vector search (differentiator)**: `SparseSearchAsync` with `SparseVectorValue` (`Indices` + `Values` lists) — for SPLADE, BM42, or hand-crafted keyword vectors.
- 🔀 **Native hybrid search (differentiator)**: `HybridSearchAsync` runs dense and sparse search concurrently, then fuses by `denseWeight` / `sparseWeight` — **not** the Redis backend's BM25 candidate merge pattern.
- 🧮 **Scalar filtering**: All vector searches support Payload filters with `Equal`, `NotEqual`, comparison operators, `Contains`, and `In`.
- 🧱 **Strongly typed record mapping**: Inherit `QdrantVectorRecord` for automatic scalar field mapping to Payload; manage vectors via `SetVector` / `GetVector`.
- ⚡ **Sync & async APIs**: Every core method has both async and synchronous versions.
- 🔌 **One-line DI registration**: The `EasyCoreQdrant(...)` extension registers Options, `QdrantClient`, and `IQdrantVectorStore`.

---

## 4. Requirements

### 4.1 Qdrant Version

Requires a running **Qdrant Server** (Sparse Vector support, recommended 1.7+).

Recommended deployment:

```bash
# Quick start with Docker (HTTP 6333 / gRPC 6334)
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

> The SDK communicates over **gRPC port 6334** by default (not HTTP 6333).

### 4.2 .NET Version

- .NET 8.0 or later

### 4.3 NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Qdrant.Client` | 1.18.1 | Qdrant gRPC client |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.0.0 | DI extensions |

---

## 5. Quick Start

### 5.1 Install the Package

```bash
dotnet add package EasyCore.Vector.Qdrant
```

Or reference the project directly in your solution:

```xml
<ProjectReference Include="..\EasyCore.Vector.Qdrant\EasyCore.Vector.Qdrant.csproj" />
```

### 5.2 Register Services

```csharp
using EasyCore.Vector.Qdrant;

builder.Services.EasyCoreQdrant(options =>
{
    options.Host = "localhost";
    options.GrpcPort = 6334;       // default gRPC port
    options.ApiKey = null;         // optional for Qdrant Cloud
    options.UseHttps = false;      // whether to use HTTPS
});
```

### 5.3 Define a Vector Entity

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

> `QdrantVectorRecord` already includes `Id`, `Content`, `Vectors`, and `Metadata`. Subclasses only need to declare business scalar fields. Scalar properties are automatically reflected into Payload on Upsert for filtering.

### 5.4 Create a Collection and Write Data

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
                    EnableSparseVector = true   // also creates contentVector_sparse slot
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

### 5.5 Dense Vector Search

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("What features does EasyCore.Agent support?");

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

### 5.6 Sparse Vector Search

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

### 5.7 Hybrid Search (Dense + Sparse)

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

## 6. Configuration

### 6.1 `QdrantOptions`

| Field | Type | Default | Description |
|---|---|---|---|
| `Host` | `string` | `localhost` | Qdrant server hostname or IP |
| `GrpcPort` | `int` | `6334` | Qdrant gRPC port |
| `ApiKey` | `string?` | `null` | API key (for Qdrant Cloud authentication) |
| `UseHttps` | `bool` | `false` | Whether to use HTTPS |

### 6.2 DI Lifetimes

| Service | Lifetime | Description |
|---|---|---|
| `QdrantOptions` | Singleton | Configuration snapshot |
| `QdrantClient` | Singleton | gRPC client connection reuse |
| `IQdrantVectorStore` | Scoped | Vector store operation entry point |

---

## 7. Data Model & Collection Design

### 7.1 Core Types

| Type | Description |
|---|---|
| `QdrantVectorRecord` | Vector record base class with `Id`, `Content`, `Vectors`, `Metadata` |
| `QdrantVectorCollectionDefinition` | Collection schema definition |
| `QdrantVectorFieldDefinition` | Vector field (dimension, distance, sparse vector toggle) |
| `SparseVectorValue` | Sparse vector value (`Indices` + `Values` lists) |
| `QdrantVectorSearchOptions` | Search parameters |
| `QdrantVectorFilter` | Filter condition container |
| `QdrantQdrantVectorSearchResult<TRecord>` | Search result (Record + Score) |

### 7.2 Built-in Payload Fields

Each record automatically includes in Payload:

| Field | Description |
|---|---|
| `content` | Text content |
| `metadata` | Scalar fields as JSON |
| `record` | Full record JSON (used for search deserialization) |

Business scalar properties (e.g. `DocumentId`, `Index`) are also written as independent Payload fields for direct filtering.

### 7.3 Vector Field Configuration

```csharp
new QdrantVectorFieldDefinition
{
    Name = "contentVector",              // dense vector field name
    Dimension = 1024,                      // must match embedding model output dimension
    Distance = Distance.Cosine,          // Qdrant.Client.Grpc Distance enum
    EnableSparseVector = true            // enables contentVector_sparse sparse vector slot
}
```

#### `Distance` Enum (Qdrant.Client.Grpc)

| Value | Description | Use Case |
|---|---|---|
| `Cosine` | Cosine distance (default) | Text embeddings, semantic search |
| `Euclid` | Euclidean distance (L2) | General vector spaces |
| `Dot` | Dot product | L2-normalized vectors |
| `Manhattan` | Manhattan distance (L1) | Special metric requirements |

> Distance is set at **collection creation** time. `QdrantVectorSearchOptions` has **no `MetricType` field** — search uses the collection's configured `Distance`.

#### Sparse Vector Naming Convention

When `EnableSparseVector = true`, the sparse vector field name is auto-generated as:

```
{denseVectorName}_sparse
```

Example: `contentVector` → `contentVector_sparse`

### 7.4 Naming Constraints

- Collection name must not be null or whitespace;
- Vector field name must not be empty;
- Point Id uses UUID string format.

---

## 8. API Examples

All examples below use `IQdrantVectorStore`. Interface hierarchy:

```
IQdrantVectorStore
  └── IVectorStore
        └── IQdrantVectorSearch
              ├── IQdrantSparseSearch
              └── IQdrantHybridSearch
```

> **Note**: `IVectorStore` does **not** include `GetAsync` / `QueryAsync`.  
> This library provides collection management (Create / Delete / Exists), writes (Upsert / UpsertBatch), deletes (Delete), and search (VectorSearch / SparseSearch / HybridSearch) only.

### 8.1 Collection Management

```csharp
// Check if collection exists
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// Create collection (skips if already exists)
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// Delete collection
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 Writes and Deletes

```csharp
// Single upsert
await _vectorStore.UpsertAsync("test_collection", record);

// Batch upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// Delete by Id
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.3 Dense Vector Search (with Filter)

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

### 8.4 Sparse Vector Search

A sparse vector consists of **indices** and **values** lists that must be the same length:

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

### 8.5 Hybrid Search (Dense + Sparse Weighted Fusion)

Unlike the Redis backend's Hybrid Search (vector + BM25 candidate fusion), the Qdrant backend runs **dense vector search** and **sparse vector search** concurrently, then merges by weight:

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

Fusion algorithm:

1. Run dense and sparse search each with `Limit × 3` candidates;
2. Merge results by Point Id;
3. Normalize dense and sparse scores by their respective maximums;
4. Weighted sum: `Score = normDense × denseWeight + normSparse × sparseWeight`;
5. Return Top-K by final Score descending.

### 8.6 Synchronous APIs

Every `Async` method has a synchronous counterpart:

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<QdrantTextVector>("test_collection", "contentVector", vector);
var sparseResults = _vectorStore.SparseSearch<QdrantTextVector>("test_collection", "contentVector_sparse", sparseVector);
var hybridResults = _vectorStore.HybridSearch<QdrantTextVector>(
    "test_collection", "contentVector", vector, "contentVector_sparse", sparseVector);
```

> Prefer async APIs in ASP.NET Core business code to avoid blocking the thread pool.

---

## 9. Filtering & Search Details

### 9.1 Supported Filter Operators

| Operator | Description | Field Types | Example |
|---|---|---|---|
| `Equal` | Equals | numeric / text / bool | `DocumentId = "doc-001"` |
| `NotEqual` | Not equals | numeric / text / bool | `Index != 0` |
| `GreaterThan` | Greater than | numeric | `Index > 5` |
| `GreaterThanOrEqual` | Greater than or equal | numeric | `Index >= 1` |
| `LessThan` | Less than | numeric | `Index < 10` |
| `LessThanOrEqual` | Less than or equal | numeric | `Index <= 100` |
| `Contains` | Keyword match | text Payload | `Content` contains `"RAG"` |
| `In` | Multi-value match (OR) | numeric / text / bool | `Index in (1,2,3)` |

Multiple conditions are combined with **AND** (`Must`). `NotEqual` maps to `MustNot`. `In` uses OR internally.

### 9.2 `QdrantVectorSearchOptions` Parameters

| Field | Default | Description |
|---|---|---|
| `Limit` | `10` | Maximum number of results |
| `ScoreThreshold` | `null` | Minimum similarity score; results below are filtered by Qdrant |
| `Filter` | `null` | Pre-search Payload filter |
| `IncludeVector` | `false` | Whether to include vector data in results |
| `IncludeMetadata` | `true` | Whether to include custom scalar fields |

> Unlike the Redis backend, there is **no `MetricType` field** — distance is determined by `QdrantVectorFieldDefinition.Distance` at collection creation.

### 9.3 Dense Vector Search Flow

1. Build Qdrant Payload Filter from `Filter`;
2. Call `QdrantClient.SearchAsync` with Named Vector and query vector;
3. Qdrant computes Score using the collection's configured `Distance`;
4. Apply `ScoreThreshold` filtering;
5. Deserialize `record` JSON from Payload into strongly typed `TRecord`;
6. Return up to `Limit` results sorted by Score descending.

### 9.4 Sparse Vector Search Flow

1. Validate `SparseVectorValue.Indices` and `Values` have equal length;
2. Build Payload Filter (optional);
3. Call `QdrantClient.SearchAsync` with `sparseIndices` parameter;
4. Qdrant searches on the Sparse Vector index;
5. Return strongly typed results with Score.

---

## 10. Integration with EasyCore.Agent.RAG

In the `AspCoreAgent` Demo, the Qdrant vector store is fully wired with RAG chunking and Embedding:

```csharp
// 1) Chunk document
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) Embed and write to Qdrant
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

// 3) Search + MMR deduplication (EasyCore.Agent.RAG)
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

Typical RAG pipeline (with hybrid search enhancement):

```text
Source document
  ↓ DocumentChunker
Text chunks
  ↓ Embedding model (dense) + sparse vectorization (SPLADE/BM42, etc.)
Dense + sparse vectors + metadata
  ↓ UpsertAsync
Qdrant Vector Store
  ↓ VectorSearchAsync / SparseSearchAsync / HybridSearchAsync
Retrieved candidates
  ↓ MmrSelector / Reranker (EasyCore.Agent.RAG)
Refined context
  ↓ Agent ChatRunAsync
Final answer
```

---

## 11. Best Practices

- ✅ **Match embedding dimension to schema**: `QdrantVectorFieldDefinition.Dimension` must equal model output dimension, or writes/searches will fail.
- ✅ **Enable sparse vectors for hybrid search**: Set `EnableSparseVector = true` at collection creation to ensure the `{Name}_sparse` slot exists.
- ✅ **Create collection once**: `CreateCollectionAsync` returns immediately if the collection already exists; call at startup or before first import.
- ✅ **Sparse vector Indices/Values must be equal length**: SDK throws if the two lists differ in length.
- ✅ **Tune hybrid weights**: Semantic-first scenarios use `denseWeight=0.7~0.8`; increase `sparseWeight` when keyword precision matters more.
- ✅ **Set `ScoreThreshold` appropriately**: Filter low-quality recall to reduce LLM context noise.
- ✅ **Use `UpsertBatchAsync` for bulk writes**: Reduces gRPC round trips; batch very large imports yourself.
- ✅ **Use UUID for Point Id**: SDK stores Point Id as UUID; prefer `Guid.NewGuid().ToString("N")` or standard UUID format.
- ⚠️ **Hybrid Search is SDK-level fusion**: Current implementation runs dense and sparse search separately then merges client-side — not Qdrant server Prefetch Fusion API; candidate pool is `Limit × 3`.
- ⚠️ **Do not store sensitive data in plain `Content`**: Encrypt or redact before ingestion if needed.

---

## 12. FAQ

### ❓ Q1: `Collection not found` or connection failure?

Qdrant is not running or the gRPC port is wrong. Verify:

1. Qdrant container/service is running;
2. `GrpcPort = 6334` (not HTTP 6333);
3. `Host` and firewall settings are correct.

### ❓ Q2: No search results or very low scores?

Check:

1. Same embedding model used for ingestion and query;
2. `Dimension` and `Distance` match collection definition;
3. `ScoreThreshold` is not set too high;
4. `Filter` conditions are not too restrictive.

### ❓ Q3: Sparse search error `indices and values must have the same length`?

`SparseVectorValue.Indices` and `Values` must correspond one-to-one with equal length. Verify sparse embedding model output format.

### ❓ Q4: Why doesn't `IVectorStore` have `GetAsync` / `QueryAsync`?

The Qdrant backend focuses on vector writes and similarity search. Point lookup or scalar-only queries can be added via the native Qdrant Client; the current SDK does not expose these. Use `VectorSearchAsync`, `SparseSearchAsync`, or `HybridSearchAsync` for retrieval.

### ❓ Q5: How does Hybrid Search differ from Redis Hybrid Search?

| Dimension | Qdrant Hybrid | Redis Hybrid |
|---|---|---|
| Fusion inputs | Dense vector score + sparse vector score | Vector score + BM25 candidate score |
| Sparse source | `SparseVectorValue` (Indices/Values) | Keyword query + manual BM25 scores |
| Use case | SPLADE/BM42 sparse embeddings | RediSearch full-text + vector |

### ❓ Q6: How to write sparse vectors after `EnableSparseVector = true`?

Collection creation registers the `{Name}_sparse` sparse vector slot. At write time, include the corresponding sparse vector in the Record's `Vectors` (via extended `QdrantVectorValue` or direct Qdrant Client). The Demo demonstrates sparse search on the query side; production requires a sparse embedding model for ingestion.

### ❓ Q7: How to choose Cosine / Euclid / Dot?

- **Cosine** (default): preferred for text semantic search;
- **Euclid**: when absolute distance matters;
- **Dot**: when vectors are L2-normalized;
- Distance **cannot be changed** after collection creation — delete and recreate to switch.

---

## 13. EasyCore.Vector.Qdrant in Depth

### 13.1 Design Goals

The core goal of `EasyCore.Vector.Qdrant` is to provide a **production-ready** Qdrant vector store wrapper for .NET applications, with API consistency across EasyCore vector backends so RAG business code can migrate across storage engines.

Key problems solved:

1. **Schema management**: Named Dense Vector + Sparse Vector joint collection creation;
2. **Type mapping**: Reflect scalar fields to Payload; JSON-serialize full records;
3. **Search abstraction**: Hide Qdrant gRPC Filter and Named Vector syntax;
4. **Differentiated search**: First-class sparse vector search and Dense+Sparse hybrid search.

### 13.2 Interface Layers

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
  └── (no GetAsync / QueryAsync)

IQdrantVectorStore : IVectorStore
  └── (marker interface for DI injection)
```

### 13.3 Typical Deployment Steps

1. Deploy Qdrant Server; confirm gRPC 6334 is reachable;
2. Call `EasyCoreQdrant` to register DI;
3. Define a `QdrantVectorRecord` subclass for business fields;
4. Call `CreateCollectionAsync` at startup; set `EnableSparseVector` as needed;
5. Chunk documents → dense embedding (+ optional sparse embedding) → `UpsertBatchAsync`;
6. User query → embedding → `VectorSearchAsync` / `HybridSearchAsync` recall;
7. Use `EasyCore.Agent.RAG` for MMR / Rerank;
8. Inject recalled content into Agent context for answer generation.

### 13.4 Comparison with Other Backends (Selection Guide)

| Dimension | Qdrant | Notes |
|---|---|---|
| Deployment complexity | Medium | Dedicated vector DB; Docker one-liner |
| Vector scale | Medium–Large | HNSW index; millions to billions of vectors |
| Sparse vectors | ✅ Native | `SparseSearchAsync` first-class |
| Hybrid search | ✅ Dense + Sparse | SDK weighted fusion, not BM25 pattern |
| Scalar query | ❌ Not exposed in SDK | Focused on vector search scenarios |
| Ecosystem consistency | High | Same Upsert/Search patterns as other EasyCore vector libs |

---

## 14. Running the Demo

The repository includes an `AspCoreAgent` Demo with complete Qdrant vector store API examples.

### 14.1 Start Qdrant

```bash
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant
```

### 14.2 Start the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

The Demo registers Qdrant in `Program.cs`:

```csharp
builder.Services.EasyCoreQdrant(options =>
{
    options.Host = "localhost";
    options.GrpcPort = 6334;
});
```

### 14.3 API Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/Qdrant/QdrantVectorStoreUpsert` | Create collection and import chunked vectors |
| `GET /api/Qdrant/QdrantVectorStoreSearch` | Dense vector search + filter |
| `GET /api/Qdrant/QdrantVectorStoreSparseSearch` | **Sparse vector search** + filter |
| `GET /api/Qdrant/QdrantVectorStoreHybridSearch` | **Dense + Sparse hybrid search** |
| `GET /api/Qdrant/QdrantVectorStoreMmrSelector` | Vector search + MMR deduplication |
| `GET /api/Qdrant/QdrantVectorStoreDelete` | Delete single record (`?id=`) |
| `GET /api/Qdrant/QdrantVectorStoreCollectionExists` | Check collection existence |
| `GET /api/Qdrant/QdrantVectorStoreDeleteCollection` | Delete entire collection |

Demo entity: `demo/AspCoreAgent/VectorEntity/QdrantTextVector.cs`.

---

## 📄 License

MIT OR Apache-2.0 (consistent with the EasyCore.Agent main repository)
