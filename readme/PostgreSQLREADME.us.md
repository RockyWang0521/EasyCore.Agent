# 🚀 EasyCore.Vector.PostgreSQL

> **EasyCore.Vector.PostgreSQL** is the PostgreSQL vector store implementation in the EasyCore.Agent ecosystem. Built on **Npgsql 10 + pgvector 0.3.2**, it provides collection management, vector similarity search, scalar filtering, hybrid search, and more — ideal for RAG knowledge bases and semantic search.  
> A PostgreSQL / pgvector-based vector store for .NET, designed for RAG and semantic search workloads.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-pgvector-336791?logo=postgresql)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- [中文](PostgreSQLREADME.md)
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
- [13. EasyCore.Vector.PostgreSQL in Depth](#13-easycorevectorpostgresql-in-depth)
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

Using Npgsql and pgvector SQL directly often requires handling `CREATE EXTENSION vector`, table DDL, HNSW/IVFFlat index creation, `<=>` / `<->` / `<#>` distance operators, parameterized filter SQL, Upsert conflict handling, and more — all of which raise the integration cost.

**EasyCore.Vector.PostgreSQL** wraps these low-level details behind a unified `IVectorStore` / `IPostgreSqlVectorStore` abstraction, letting you manage vector collections with strongly typed C# models.

### 📦 Where It Fits

```
EasyCore.Agent (Agent SDK)
    └── EasyCore.Agent.RAG (chunking / MMR / Rerank, etc.)
            └── EasyCore.Vector.* (vector store abstractions & backends)
                    └── EasyCore.Vector.PostgreSQL (this document)
```

It shares the same API style as other vector backends (Redis, Qdrant, Milvus, Elasticsearch), so you can switch storage engines without changing business code.

---

## 2. Architecture

### 2.1 Component Diagram

![2-1-component-diagram](docs/svg/2-1-component-diagram-57c46610.svg)


### 2.2 Vector Search Sequence

![2-2-vector-search-sequence](docs/svg/2-2-vector-search-sequence-49b0592d.svg)


### 2.3 Storage Model

How each Collection is organized in PostgreSQL:

| Layer | Mapping | Description |
|---|---|---|
| Schema | `public` | Default schema |
| Collection | Table name (lowercase) | `collectionName` maps to a PostgreSQL table |
| Row | One `PostgreSqlVectorRecord` | Each row is one vector document |
| Column | Scalar fields + `vector(n)` | `Id` PK, `Content` text, custom scalars, vector columns |

When creating a Collection, the SDK automatically runs:

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

## 3. Core Features

- 🗂️ **Collection lifecycle**: Create, delete, existence check; deleting a Collection runs `DROP TABLE`.
- 📥 **Upsert writes**: Single and batch Upsert via `ON CONFLICT (Id) DO UPDATE` for idempotent writes.
- 🔍 **Vector similarity search**: pgvector distance operators with Cosine / L2 / Inner Product metrics.
- 🧮 **Scalar filtering**: Both vector search and scalar Query support filters — `Equal`, `NotEqual`, comparisons, `Contains`, `In`.
- 🔀 **Hybrid Search**: Fuse vector results with BM25/keyword candidates by weight for better recall.
- 🧱 **Strongly typed records**: Extend `PostgreSqlVectorRecord` for scalar fields; manage vectors via `SetVector` / `GetVector`.
- ⚡ **Sync & async APIs**: Every core method has both `Async` and synchronous versions.
- 🔌 **One-line DI registration**: `EasyCorePostgreSql(...)` registers Options and `IPostgreSqlVectorStore`.

---

## 4. Requirements

### 4.1 PostgreSQL & pgvector

You need **PostgreSQL** with the **pgvector extension** installed.

The SDK runs `CREATE EXTENSION IF NOT EXISTS vector` on first Collection creation. If the database user lacks extension privileges, have a DBA run it first:

```sql
CREATE EXTENSION vector;
```

Recommended Docker deployment:

```bash
# Quick start PostgreSQL with pgvector
docker run -d \
  --name pgvector \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=vector_db \
  -p 5432:5432 \
  pgvector/pgvector:pg17
```

Docker Compose example:

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

Verify the extension after startup:

```bash
docker exec -it pgvector psql -U postgres -d vector_db -c "CREATE EXTENSION IF NOT EXISTS vector;"
docker exec -it pgvector psql -U postgres -d vector_db -c "SELECT extname, extversion FROM pg_extension WHERE extname = 'vector';"
```

### 4.2 .NET Version

- .NET 8.0 or later

### 4.3 NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Npgsql` | 10.x | PostgreSQL connection and SQL execution |
| `Pgvector` | 0.3.2 | pgvector type and vector operations |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 8.x | DI extensions |

---

## 5. Quick Start

### 5.1 Install the Package

```bash
dotnet add package EasyCore.Vector.PostgreSQL
```

Or reference the project directly:

```xml
<ProjectReference Include="..\EasyCore.Vector.PostgreSQL\EasyCore.Vector.PostgreSQL.csproj" />
```

### 5.2 Register Services

```csharp
using EasyCore.Vector.PostgreSQL;

builder.Services.EasyCorePostgreSql(options =>
{
    options.ConnectionString =
        "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=your_password;";
});
```

### 5.3 Define a Vector Entity

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

> `PostgreSqlVectorRecord` already includes `Id`, `Content`, and `Vectors` — subclasses only declare business scalar fields.

### 5.4 Create a Collection and Write Data

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

### 5.5 Vector Search

```csharp
var queryEmbedding = await embeddingClient.EmbedAsync("What features does EasyCore.Agent support?");

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

## 6. Configuration

### 6.1 `PostgreSqlOptions`

| Field | Type | Description | Example |
|---|---|---|---|
| `ConnectionString` | `string` | PostgreSQL connection string (required) | See below |

Standard Npgsql connection string formats:

```
Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=your_password
Host=db.example.com;Port=5432;Database=vector_db;Username=app;Password=secret;SSL Mode=Require
```

Common parameters:

| Parameter | Description |
|---|---|
| `Host` | Database host |
| `Port` | Port, default 5432 |
| `Database` | Database name |
| `Username` / `Password` | Credentials |
| `SSL Mode` | Use `Require` or `VerifyFull` in production |
| `Pooling` | Connection pooling, enabled by default |
| `Timeout` | Connection timeout in seconds |

### 6.2 DI Lifetimes

| Service | Lifetime | Description |
|---|---|---|
| `PostgreSqlOptions` | Singleton | Configuration snapshot |
| `IPostgreSqlVectorStore` | Scoped | Vector store entry point; holds `NpgsqlDataSource` internally |

---

## 7. Data Model & Collection Design

### 7.1 Core Types

| Type | Description |
|---|---|
| `PostgreSqlVectorRecord` | Record base class with `Id`, `Content`, `Vectors` |
| `PostgreSqlVectorCollectionDefinition` | Collection schema definition |
| `PostgreSqlVectorFieldDefinition` | Vector field (dimension, metric, index type) |
| `PostgreSqlScalarFieldDefinition` | Scalar field (type, primary key flag) |
| `PostgreSqlVectorSearchOptions` | Vector search parameters |
| `PostgreSqlVectorFilter` | Filter condition container |
| `PostgreSqlVectorSearchResult<TRecord>` | Search result (Record + Score) |

### 7.2 Built-in Fields

The SDK automatically adds these fields when creating a Collection — **do not** redeclare them:

| Field | PostgreSQL Type | Description |
|---|---|---|
| `Id` | `VARCHAR(128) PRIMARY KEY` | Primary key, Upsert conflict key |
| `Content` | `VARCHAR(65535)` | Text content, usable for keyword filtering |

### 7.3 Vector Field Configuration

```csharp
new PostgreSqlVectorFieldDefinition
{
    Name = "contentVector",                              // Vector field name (column name)
    Dimension = 1024,                                    // Must match embedding model output
    MetricType = PostgreSqlSimilarityMetricType.Cosine,  // Cosine / L2 / InnerProduct
    IndexType = PostgreSqlVectorIndexType.Hnsw,          // Hnsw / Ivfflat
    CreateIndex = true,                                  // Whether to create a vector index
    Lists = 100                                          // IVFFlat lists parameter
}
```

#### Similarity Metrics

| Enum | pgvector Operator | Score Conversion |
|---|---|---|
| `Cosine` | `<=>` (cosine distance) | `1 - distance` (higher = more similar) |
| `L2` | `<->` (Euclidean distance) | `1 / (1 + distance)` |
| `InnerProduct` | `<#>` (negative inner product) | `distance * -1` |

#### Index Types

| Index Type | pgvector Syntax | Use Case |
|---|---|---|
| `Hnsw` (default) | `USING hnsw (... vector_cosine_ops)` | Online search, low latency |
| `Ivfflat` | `USING ivfflat (... vector_cosine_ops) WITH (lists = N)` | Large datasets, tunable lists |

Index ops class is selected automatically from `MetricType`:

| MetricType | ops class |
|---|---|
| `Cosine` | `vector_cosine_ops` |
| `L2` | `vector_l2_ops` |
| `InnerProduct` | `vector_ip_ops` |

### 7.4 Scalar Field Types

| `ScalarFieldType` | PostgreSQL Mapping |
|---|---|
| `Bool` | `BOOLEAN` |
| `Int8` / `Int16` | `SMALLINT` |
| `Int32` | `INTEGER` |
| `Int64` | `BIGINT` |
| `Float` | `REAL` |
| `Double` | `DOUBLE PRECISION` |
| `String` / `VarChar` | `TEXT` / `VARCHAR(n)` |
| `Json` | `JSONB` |

### 7.5 Naming Constraints

Collection and field names must match:

```
^[A-Za-z_][A-Za-z0-9_]*$
```

Examples: `test_collection`, `DocumentId` ✅; `test-collection`, `123abc` ❌.

> Collection names map to PostgreSQL table names. `CollectionExistsAsync` queries `information_schema.tables` using lowercase — prefer lowercase collection names (e.g. `test_collection`).

---

## 8. API Examples

All examples use `IPostgreSqlVectorStore`. Interface hierarchy:

```
IPostgreSqlVectorStore
  └── IVectorStore
        └── IPostgreSqlVectorSearch
              └── IPostgreSqlHybridSearch
```

### 8.1 Collection Management

```csharp
// Check if Collection exists (queries table in public schema)
var exists = await _vectorStore.CollectionExistsAsync("test_collection");

// Create Collection (skips if table already exists)
await _vectorStore.CreateCollectionAsync("test_collection", definition);

// Delete Collection (DROP TABLE)
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 Write & Delete

```csharp
// Single Upsert (ON CONFLICT DO UPDATE)
await _vectorStore.UpsertAsync("test_collection", record);

// Batch Upsert
await _vectorStore.UpsertBatchAsync("test_collection", records);

// Delete by Id
await _vectorStore.DeleteAsync("test_collection", recordId);
```

### 8.3 Get by Id

```csharp
var record = await _vectorStore.GetAsync<PostgreSqlTextVector>(
    collectionName: "test_collection",
    id: "abc123",
    includeVector: true,
    vectorName: "contentVector",
    includeMetadata: true);
```

### 8.4 Scalar Query (no vector similarity)

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

### 8.5 Vector Search (with Filter)

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

### 8.6 Hybrid Search

Hybrid Search is for combined ranking of semantic similarity and keyword hits. Obtain BM25 candidates via `QueryAsync` + `Contains`, then fuse with vector results:

```csharp
// 1) Keyword candidates (example: Content contains "RAG")
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

// 2) Build BM25 candidate scores (replace with real BM25 in production)
var bm25Results = keywordRecords
    .Select((record, index) => new PostgreSqlVectorSearchResult<PostgreSqlTextVector>
    {
        Record = record,
        Score = Math.Max(0.1f, 1.0f - index * 0.08f)
    })
    .ToList();

// 3) Hybrid fusion
var hybridResults = await _vectorStore.HybridSearchAsync(
    collectionName: "test_collection",
    vectorName: "contentVector",
    vector: queryVector,
    bm25Results: bm25Results,
    options: new PostgreSqlVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f,
    bm25Weight: 0.3f);
```

The fusion algorithm normalizes vector and BM25 scores separately, then computes a weighted sum for Top-K results.

### 8.7 Synchronous APIs

Every `Async` method has a sync counterpart:

```csharp
_vectorStore.CreateCollection("test_collection", definition);
_vectorStore.Upsert("test_collection", record);
var results = _vectorStore.VectorSearch<PostgreSqlTextVector>("test_collection", "contentVector", vector);
```

> Prefer async APIs in ASP.NET Core to avoid blocking the thread pool.

---

## 9. Filtering & Search Details

### 9.1 Supported Filter Operators

| Operator | Description | Field Types | SQL Implementation |
|---|---|---|---|
| `Equal` | Equals | Numeric / text / bool | `column = @p` |
| `NotEqual` | Not equals | Numeric / text / bool | `column <> @p` |
| `GreaterThan` | Greater than | Numeric | `column > @p` |
| `GreaterThanOrEqual` | Greater than or equal | Numeric | `column >= @p` |
| `LessThan` | Less than | Numeric | `column < @p` |
| `LessThanOrEqual` | Less than or equal | Numeric | `column <= @p` |
| `Contains` | Text contains (case-insensitive) | Text | `column ILIKE '%value%'` |
| `In` | Multi-value match | Numeric / text / bool | `column = ANY(@p)` |

Multiple conditions are combined with **AND**. `In` uses OR semantics internally (`= ANY` array).

### 9.2 `PostgreSqlVectorSearchOptions` Parameters

| Field | Default | Description |
|---|---|---|
| `Limit` | `10` | Maximum number of results |
| `ScoreThreshold` | `null` | Minimum score; results below are filtered out |
| `Filter` | `null` | Pre-search filter conditions |
| `MetricType` | `Cosine` | Metric used for score conversion |
| `IncludeVector` | `false` | Include vector data in results |
| `IncludeMetadata` | `true` | Include custom scalar fields |

### 9.3 Vector Search Execution Flow

1. Build parameterized `WHERE` clause from `Filter`;
2. Compute Score expression in inner subquery (pgvector distance operators);
3. Apply `ScoreThreshold` filter in outer query;
4. Sort by Score descending and take `Limit` rows;
5. Map rows to strongly typed `TRecord` via reflection.

---

## 10. Integration with EasyCore.Agent.RAG

The `AspCoreAgent` demo wires PostgreSQL vector storage with RAG chunking and embedding end to end:

```csharp
// 1) Chunk the document
var chunks = DocumentChunker.Chunk(content, "documentId", chunkSize: 800, overlap: 100);

// 2) Embed and write to PostgreSQL
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

// 3) Search + MMR deduplication (EasyCore.Agent.RAG)
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

Typical RAG pipeline:

```text
Source document
  ↓ DocumentChunker
Text chunks
  ↓ Embedding model
Vectors + metadata
  ↓ UpsertAsync
PostgreSQL Vector Store (pgvector)
  ↓ VectorSearchAsync / HybridSearchAsync
Retrieved candidates
  ↓ MmrSelector / Reranker (EasyCore.Agent.RAG)
Refined context
  ↓ Agent ChatRunAsync
Final answer
```

---

## 11. Best Practices

- ✅ **Match embedding dimension to schema**: `PostgreSqlVectorFieldDefinition.Dimension` must equal model output dimension.
- ✅ **Create Collection once**: `CreateCollectionAsync` returns immediately if the table exists — call at startup or before first import.
- ✅ **Pre-create pgvector extension in production**: Ensure the DB user has `CREATE EXTENSION` privilege, or run it in deployment scripts.
- ✅ **Set `ScoreThreshold` appropriately**: Filter low-quality hits to reduce LLM context noise.
- ✅ **Use `UpsertBatchAsync` for bulk writes**: Reduces connection overhead; split very large batches yourself (default is per-row Upsert).
- ✅ **IVFFlat needs sufficient data**: pgvector recommends creating IVFFlat indexes after enough rows exist; tune the `Lists` parameter.
- ✅ **HNSW for online search**: Default index type with low query latency and no extra tuning.
- ✅ **Normalize BM25 scores for Hybrid Search**: The SDK normalizes by max value, but upstream BM25 scores should be comparable.
- ✅ **Don't store sensitive data in plain `Content`**: Encrypt or redact before ingestion.
- ⚠️ **Avoid frequent DeleteCollection**: `DeleteCollectionAsync` runs `DROP TABLE`; rebuilding indexes is costly at scale.
- ⚠️ **Enable connection pooling and SSL in production**: Configure via `Pooling=true` and `SSL Mode=Require` in the connection string.

---

## 12. FAQ

### ❓ Q1: `relation "xxx" does not exist` error?

The Collection hasn't been created or the table was dropped. Call `CreateCollectionAsync` first and verify `collectionName` matches across write/search calls.

### ❓ Q2: No vector search results or very low scores?

Check:

1. Same embedding model used for ingestion and query;
2. `Dimension` and `MetricType` match the Collection definition;
3. `ScoreThreshold` isn't set too high;
4. `Filter` conditions aren't too restrictive;
5. pgvector index ops class matches `MetricType`.

### ❓ Q3: `Invalid identifier` error?

Collection and field names must match `^[A-Za-z_][A-Za-z0-9_]*$` — no hyphens or non-ASCII characters.

### ❓ Q4: Why is `vectorName` required when `includeVector = true`?

A record may have multiple vector fields; the SDK needs to know which one to read.

### ❓ Q5: `permission denied to create extension "vector"`?

The database user lacks extension privileges. Have a superuser run `CREATE EXTENSION vector;` or grant the appropriate permissions.

### ❓ Q6: Ivfflat vs HNSW?

- **HNSW** (default): Low query latency, good for online search, no lists tuning;
- **Ivfflat**: Better for very large datasets; tune `Lists` but balance recall vs build cost.

### ❓ Q7: Can it coexist with existing PostgreSQL business tables?

Yes. Each Collection is a separate table. Avoid naming conflicts with existing tables.

### ❓ Q8: Is Upsert atomic?

Single-row Upsert uses `INSERT ... ON CONFLICT (Id) DO UPDATE` atomically. Batch Upsert currently executes row by row.

---

## 13. EasyCore.Vector.PostgreSQL in Depth

### 13.1 Design Goals

`EasyCore.Vector.PostgreSQL` provides a **production-ready** PostgreSQL vector store for .NET apps, with API parity across EasyCore vector backends so RAG code can migrate between engines.

It focuses on:

1. **Schema management**: Auto-adds `Id` / `Content`, validates PK and duplicate names, creates pgvector extension;
2. **Type mapping**: Reflection-based column read/write for common scalar types and enums;
3. **Search abstraction**: Hides pgvector distance operators and parameterized SQL;
4. **Composability**: Layered interfaces for vector search, scalar query, and hybrid fusion.

### 13.2 Interface Layers

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
  └── (marker interface for DI injection)
```

### 13.3 Typical Rollout Steps

1. Deploy PostgreSQL + pgvector (Docker or managed), configure `ConnectionString`;
2. Call `EasyCorePostgreSql` to register DI;
3. Define a `PostgreSqlVectorRecord` subclass for business fields;
4. Call `CreateCollectionAsync` at startup to ensure table and indexes exist;
5. Chunk documents → embed → `UpsertBatchAsync`;
6. User query → embed → `VectorSearchAsync`;
7. Apply MMR / Rerank via `EasyCore.Agent.RAG`;
8. Inject retrieved context into Agent for the final answer.

### 13.4 Backend Comparison (selection guide)

| Dimension | PostgreSQL + pgvector | Notes |
|---|---|---|
| Deployment complexity | Low | Add extension to existing PostgreSQL |
| Vector scale | Medium–large | HNSW/IVFFlat supports millions of vectors |
| Hybrid search | Supported | Provide BM25 candidate scores yourself |
| Transactions / relational | Strong | Vectors and business data in one database |
| SQL ecosystem | Strong | Standard backup, replication, analytics |
| API consistency | High | Same `IVectorStore` patterns as other EasyCore backends |

---

## 14. Running the Demo

The repo includes an `AspCoreAgent` demo with full PostgreSQL vector store API examples.

### 14.1 Start PostgreSQL + pgvector

```bash
docker run -d \
  --name pgvector \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=Q123456 \
  -e POSTGRES_DB=vector_db \
  -p 5432:5432 \
  pgvector/pgvector:pg17
```

### 14.2 Configure the Connection String

In `demo/AspCoreAgent/Program.cs`, ensure the connection string matches your Docker setup:

```csharp
builder.Services.EasyCorePostgreSql(options =>
{
    options.ConnectionString =
        "Host=localhost;Port=5432;Database=vector_db;Username=postgres;Password=Q123456;";
});
```

### 14.3 Run the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 14.4 API Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/PostgreSQL/PostgreSqlVectorStoreUpsert` | Create Collection and import chunked vectors |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreSearch` | Vector search + filter + score filtering |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreMmrSelector` | Vector search + MMR deduplication |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreGet` | Get record by Id |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreQuery` | Scalar query |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreHybridSearch` | Hybrid search example |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreDelete` | Delete a single record |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreCollectionExists` | Check Collection existence |
| `GET /api/PostgreSQL/PostgreSqlVectorStoreDeleteCollection` | Delete entire Collection |

Demo entity: `demo/AspCoreAgent/VectorEntity/PostgreSqlTextVector.cs`.

---

## 📄 License

MIT OR Apache-2.0 (same as the EasyCore.Agent main repository)
