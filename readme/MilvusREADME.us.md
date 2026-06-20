# 🚀 EasyCore.Vector.Milvus

> **EasyCore.Vector.Milvus** is the Milvus vector store implementation in the EasyCore.Agent ecosystem. Built on **Milvus 2.x + Milvus.Client**, it provides collection management, vector similarity search, scalar filtering, hybrid search, and Milvus lifecycle operations (Flush / Load / Release).  
> A Milvus-based vector store for .NET, designed for RAG and large-scale semantic search.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![Milvus](https://img.shields.io/badge/Milvus-2.x-green)
![Vector](https://img.shields.io/badge/Vector-Search-blueviolet)

---

## 🌍 Language

- [中文](MilvusREADME.md)
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
- [9. Milvus Lifecycle Management](#9-milvus-lifecycle-management)
- [10. Filtering & Search Details](#10-filtering--search-details)
- [11. Integration with EasyCore.Agent.RAG](#11-integration-with-easycoreagentrag)
- [12. Best Practices](#12-best-practices)
- [13. FAQ](#13-faq)
- [14. Running the Demo](#14-running-the-demo)

---

## 1. Introduction

**EasyCore.Vector.Milvus** wraps the Milvus SDK with strongly typed APIs consistent across EasyCore vector backends — ideal for large-scale vector retrieval and RAG knowledge bases.

### 📦 Where It Fits

```
EasyCore.Agent → EasyCore.Agent.RAG → EasyCore.Vector.*
                                            └── EasyCore.Vector.Milvus (this doc)
```

---

## 2. Architecture

![2-architecture](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-架构图-ef6518fd.svg)


---

## 3. Core Features

- 🗂️ Collection lifecycle: create, delete, exists check
- 📥 Single and batch upsert
- 🔍 KNN vector search + scalar filters
- 🔀 Hybrid search (vector + external BM25 candidate merge)
- ⚙️ **Milvus-specific**: `FlushAsync`, `LoadAsync`, `ReleaseAsync`
- 🧱 Strongly typed `MilvusVectorRecord` mapping
- 🔌 `EasyCoreMilvus(...)` DI registration

---

## 4. Requirements

- .NET 8.0+
- Milvus 2.x (Standalone or Cluster)
- NuGet: `Milvus.Client` 2.3.0-preview.1

```bash
docker run -d --name milvus -p 19530:19530 -p 9091:9091 milvusdb/milvus:latest standalone
```

---

## 5. Quick Start

### 5.1 Register Services

```csharp
builder.Services.EasyCoreMilvus(options =>
{
    options.Host = "localhost";
    options.Port = 19530;
    options.DatabaseName = "default";
    options.UseTls = false;
});
```

### 5.2 Define Entity

```csharp
public sealed class MilvusTextVector : MilvusVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;
    public int Index { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
```

### 5.3 Create Collection and Search

```csharp
await _vectorStore.CreateCollectionAsync("test_collection", definition);
record.SetVector("contentVector", embedding);
await _vectorStore.UpsertAsync("test_collection", record);
await _vectorStore.FlushAsync("test_collection");

var results = await _vectorStore.VectorSearchAsync<MilvusTextVector>(
    "test_collection", "contentVector", queryVector,
    new MilvusVectorSearchOptions { Limit = 10, IncludeMetadata = true });
```

---

## 6. Configuration

### 6.1 `MilvusOptions`

| Field | Default | Description |
|---|---|---|
| `Host` | `localhost` | Milvus host |
| `Port` | `19530` | gRPC port |
| `DatabaseName` | `default` | Database name |
| `UserName` / `Password` | — | Authentication |
| `Token` | — | Token auth |
| `UseTls` | `false` | Enable TLS |

### 6.2 DI Lifetimes

| Service | Lifetime |
|---|---|
| `MilvusOptions` | Singleton |
| `MilvusClient` | Singleton |
| `IMilvusVectorStore` | Scoped |

---

## 7. Data Model & Collection Design

### 7.1 Vector Index Types

| `MilvusVectorIndexType` | Description |
|---|---|
| `AutoIndex` | Milvus auto-selects (default) |
| `Flat` | Brute-force |
| `IvfFlat` | IVF_FLAT |
| `IvfSq8` | IVF_SQ8 |
| `Hnsw` | HNSW |

HNSW params: `M` (default 16), `EfConstruction` (default 200). IVF param: `NList` (default 1024).

### 7.2 Built-in Fields

`Id` and `Content` are auto-added — do not redeclare.

### 7.3 Naming

Must match: `^[A-Za-z_][A-Za-z0-9_]*$`

---

## 8. API Examples

### 8.1 Collection Management

```csharp
await _vectorStore.CreateCollectionAsync("test_collection", definition);
var exists = await _vectorStore.CollectionExistsAsync("test_collection");
await _vectorStore.DeleteCollectionAsync("test_collection");
```

### 8.2 Write & Delete

```csharp
await _vectorStore.UpsertAsync("test_collection", record);
await _vectorStore.UpsertBatchAsync("test_collection", records);
await _vectorStore.DeleteAsync("test_collection", id);
```

### 8.3 Get / Query

```csharp
var record = await _vectorStore.GetAsync<MilvusTextVector>(
    "test_collection", id, includeVector: true, vectorName: "contentVector");

var records = await _vectorStore.QueryAsync<MilvusTextVector>("test_collection", filter, limit: 10);
```

### 8.4 Vector Search with Filter

```csharp
var results = await _vectorStore.VectorSearchAsync<MilvusTextVector>(
    "test_collection", "contentVector", queryVector,
    new MilvusVectorSearchOptions { Limit = 10, ScoreThreshold = 0.8f, Filter = filter });
```

### 8.5 Hybrid Search

```csharp
var hybridResults = await _vectorStore.HybridSearchAsync(
    "test_collection", "contentVector", queryVector, bm25Results,
    options: new MilvusVectorSearchOptions { Limit = 5 },
    vectorWeight: 0.7f, bm25Weight: 0.3f);
```

---

## 9. Milvus Lifecycle Management

After upsert, data sits in growing segments; collections must be loaded for search.

| Method | Description |
|---|---|
| `FlushAsync(collectionName)` | Flush growing segments to sealed |
| `LoadAsync(collectionName)` | Load collection into query node memory |
| `ReleaseAsync(collectionName)` | Release from memory |

![9-milvus-lifecycle-management](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/9-milvus-生命周期管理-0e62eac8.svg)


> Search auto-calls `LoadAsync` internally; call `FlushAsync` explicitly after bulk writes.

---

## 10. Filtering & Search Details

### 10.1 Filter Operators

`Equal`, `NotEqual`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `Contains`, `In`

### 10.2 `MilvusVectorSearchOptions`

| Field | Default | Description |
|---|---|---|
| `Limit` | `10` | Max results |
| `ScoreThreshold` | `null` | Similarity threshold |
| `Filter` | `null` | Scalar filter |
| `MetricType` | `Cosine` | Milvus.Client metric |
| `IncludeVector` | `false` | Return vectors |
| `IncludeMetadata` | `true` | Return custom scalar fields |

---

## 11. Integration with EasyCore.Agent.RAG

```csharp
var chunks = DocumentChunker.Chunk(content, documentId, 800, 100);
foreach (var chunk in chunks)
{
    var embedding = await agent.EmbedAsync(chunk.Content);
    var record = new MilvusTextVector { /* map fields */ };
    record.SetVector("contentVector", embedding);
    await vectorStore.UpsertAsync("test_collection", record);
}
await vectorStore.FlushAsync("test_collection");

var candidates = await vectorStore.VectorSearchAsync<MilvusTextVector>(...);
var final = MmrSelector.Select(mmrCandidates, topK: 2, lambda: 0.7);
```

---

## 12. Best Practices

- ✅ Call `FlushAsync` after bulk writes
- ✅ Monitor collection load state in production
- ✅ Keep `Dimension` aligned with embedding model
- ✅ HNSW for low-latency online search; IVF for very large scale
- ⚠️ After `ReleaseAsync`, call `LoadAsync` again before search
- ⚠️ Use distinct Keys when writing Items in parallel steps

---

## 13. FAQ

### ❓ Q1: No search results?
Check Load state, Flush status, filter strictness, and dimension match.

### ❓ Q2: Flush vs Load?
Flush persists segments; Load brings data into memory for queries.

### ❓ Q3: What does AutoIndex pick?
Milvus chooses based on data scale — usually no manual tuning needed.

---

## 14. Running the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

| Endpoint | Description |
|---|---|
| `GET /api/Milvus/MilvusVectorStoreUpsert` | Create and ingest |
| `GET /api/Milvus/MilvusVectorStoreSearch` | Vector search |
| `GET /api/Milvus/MilvusVectorStoreMmrSelector` | MMR dedup |
| `GET /api/Milvus/MilvusVectorStoreGet` | Get by id |
| `GET /api/Milvus/MilvusVectorStoreQuery` | Scalar query |
| `GET /api/Milvus/MilvusVectorStoreHybridSearch` | Hybrid search |
| `GET /api/Milvus/MilvusVectorStoreFlush` | Flush |
| `GET /api/Milvus/MilvusVectorStoreLoad` | Load |
| `GET /api/Milvus/MilvusVectorStoreRelease` | Release |
| `GET /api/Milvus/MilvusVectorStoreDelete` | Delete record |
| `GET /api/Milvus/MilvusVectorStoreDeleteCollection` | Delete collection |

---

## 📄 License

MIT OR Apache-2.0
