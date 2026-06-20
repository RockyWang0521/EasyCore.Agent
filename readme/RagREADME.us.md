# 🚀 EasyCore.Agent.RAG

> **EasyCore.Agent.RAG** is the RAG (Retrieval-Augmented Generation) utility library in the EasyCore.Agent ecosystem. It provides document chunking, query rewriting, multi-query generation, and MMR deduplication — composable with any vector store backend (Redis, Qdrant, Milvus, PostgreSQL, Elasticsearch).  
> A RAG utility library for .NET with document chunking, query rewriting, multi-query generation, and MMR selection.

![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp)
![RAG](https://img.shields.io/badge/RAG-Retrieval-blueviolet)
![Agent](https://img.shields.io/badge/EasyCore-Agent-green)

---

## 🌍 Language

- [中文](RagREADME.md)
- English (current document)

---

## 📚 Table of Contents

- [1. Introduction](#1-introduction)
- [2. Architecture](#2-architecture)
- [3. Core Features](#3-core-features)
- [4. Requirements](#4-requirements)
- [5. Quick Start](#5-quick-start)
- [6. Module Reference](#6-module-reference)
- [7. API Examples](#7-api-examples)
- [8. Full RAG Pipeline](#8-full-rag-pipeline)
- [9. Best Practices](#9-best-practices)
- [10. FAQ](#10-faq)
- [11. EasyCore.Agent.RAG in Depth](#11-easycoreagentrag-in-depth)
- [12. Running the Demo](#12-running-the-demo)

---

## 1. Introduction

### 🎯 What Problem Does It Solve?

Enterprise knowledge-base Q&A (RAG) typically involves several independent steps:

- Long documents must be chunked before embedding;
- Multi-turn user questions with pronouns or omissions need query rewriting;
- Single-query retrieval may under-recall, requiring multi-query expansion;
- Top-K vector hits are often semantically redundant, needing MMR for diversity.

Reimplementing this logic in every project is costly and hard to tune consistently.

**EasyCore.Agent.RAG** wraps these capabilities as lightweight, stateless static utilities. It decouples from `EasyCore.Agent` (Agent / Embedding) and `EasyCore.Vector.*` (vector storage) so you can mix and match as needed.

### 📦 Where It Fits in the Project

```
EasyCore.Agent (Agent SDK / Embedding / session context)
    └── EasyCore.Agent.RAG (this doc: chunking / rewrite / multi-query / MMR)
            └── EasyCore.Vector.* (vector ingest & search)
                    ├── EasyCore.Vector.Redis
                    ├── EasyCore.Vector.Qdrant
                    ├── EasyCore.Vector.Milvus
                    ├── EasyCore.Vector.PostgreSQL
                    └── EasyCore.Vector.Elasticsearch
```

This library does **not** bind to a specific vector database and requires **no** DI registration — reference the package and call static methods directly.

---

## 2. Architecture

### 2.1 RAG Pipeline Overview

![2-1-rag-pipeline-overview](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-1-rag-pipeline-overview-30723cd3.svg)


### 2.2 Module Responsibilities

| Module | Type | LLM Required | Description |
|---|---|---|---|
| `DocumentChunker` | Static utility | No | Fixed-window chunking with overlap |
| `QueryRewrite` | Static utility | Yes | Rewrite retrieval query using conversation history |
| `MultiQueryGenerator` | Static utility | Yes | Generate multiple search queries from one question |
| `MmrSelector` | Static utility | No | Balance relevance and diversity via MMR |

### 2.3 Query Rewrite Sequence

![2-3-query-rewrite-sequence](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/2-3-query-rewrite-sequence-ce95c915.svg)


---

## 3. Core Features

- 📄 **DocumentChunker**: Character-window chunking with configurable `chunkSize` and `overlapSize`; preserves `StartIndex` / `EndIndex` for traceability.
- 🔄 **QueryRewrite**: Uses `AIAgent` to rewrite ambiguous session questions into standalone retrieval queries; auto-detects language and keeps it consistent with the user question.
- 🔀 **MultiQueryGenerator**: Generates N search queries from different angles to improve recall coverage.
- 🎯 **MmrSelector**: Maximum Marginal Relevance — keeps relevance while reducing duplicate results.
- 🧩 **Extensible prompts**: `QueryRewritePromptBuilder` and `MultiQueryPromptBuilder` expose system/user prompt builders for customization.
- ⚡ **Sync & async**: Both `QueryRewrite` and `MultiQueryGenerator` offer synchronous and asynchronous APIs.
- 🔌 **Zero-config**: No `ServiceCollection` extension — use immediately after referencing the assembly.

---

## 4. Requirements

### 4.1 .NET Version

- .NET 8.0 or later

### 4.2 NuGet Dependencies

| Package | Purpose |
|---|---|
| `Microsoft.Agents.AI` | `AIAgent`, `ChatMessage`, and Agent runtime |
| `Microsoft.Agents.AI.OpenAI` | OpenAI-compatible model integration (via EasyCore.Agent) |

### 4.3 Companion Components

| Component | Purpose |
|---|---|
| `EasyCore.Agent` | Create `AIAgent`, embeddings, session context |
| `EasyCore.Vector.*` | Vector ingest and similarity search |

---

## 5. Quick Start

### 5.1 Install the Package

```bash
dotnet add package EasyCore.Agent.RAG
```

### 5.2 Document Chunking

```csharp
using EasyCore.Agent.RAG;

var content = File.ReadAllText("manual.md");

var chunks = DocumentChunker.Chunk(
    content: content,
    documentId: "manual-001",
    chunkSize: 800,
    overlapSize: 100);

foreach (var chunk in chunks)
{
    Console.WriteLine($"[{chunk.Index}] {chunk.StartIndex}-{chunk.EndIndex}: {chunk.Content[..Math.Min(50, chunk.Content.Length)]}...");
}
```

### 5.3 Query Rewrite

```csharp
using EasyCore.Agent.RAG;
using Microsoft.Extensions.AI;

// Assume agent is created via EasyCore.Agent and session history exists
var history = agentClient.GetChatContext(sessionId);

var rewritten = await QueryRewrite.RewriteAsync(
    query: "What features does it support?",
    agent: agent,
    history: history);

// May output: "What features does EasyCore.Agent support?"
```

### 5.4 Multi Query

```csharp
var queries = await MultiQueryGenerator.GenerateAsync(
    query: "How do I apply for annual leave?",
    agent: agent,
    count: 3);

// Example output:
// - How do I apply for annual leave?
// - What is the annual leave application process?
// - What are the employee leave policy rules?
```

### 5.5 MMR Deduplication

```csharp
var candidates = searchResults.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector("contentVector")
}).ToList();

var diversified = MmrSelector.Select(
    candidates: candidates,
    topK: 3,
    lambda: 0.7);
```

---

## 6. Module Reference

### 6.1 DocumentChunker

| Member | Description |
|---|---|
| `Chunk(content, documentId, chunkSize, overlapSize)` | Split text into `List<DocumentChunk>` |

**Parameter constraints:**

| Parameter | Default | Constraint |
|---|---|---|
| `chunkSize` | `800` | Must be > 0 |
| `overlapSize` | `100` | Must be ≥ 0 and < `chunkSize` |

**Behavior:**

- Normalizes line endings (`\r\n` → `\n`) and trims;
- Empty content returns an empty list;
- Each chunk gets a unique `Id` (GUID N format);
- Blank chunks are skipped.

### 6.2 DocumentChunk

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique chunk identifier |
| `DocumentId` | `string` | Source document ID |
| `Index` | `int` | Zero-based chunk index in the document |
| `Content` | `string` | Chunk text |
| `StartIndex` | `int` | Start character index in source text |
| `EndIndex` | `int` | End character index in source text |

### 6.3 QueryRewrite

| Method | Description |
|---|---|
| `RewriteAsync(query, agent, history, cancellationToken)` | Async rewrite |
| `Rewrite(query, agent, history)` | Sync rewrite |

**Fallback:** Returns the original `query` if the LLM output is empty.

**Prompt rules (summary):**

1. Detect the language of the latest user question;
2. Rewrite into a standalone, clear, search-friendly query;
3. Keep the same language as the user question;
4. Do not answer, explain, or invent information not in history;
5. Return unchanged if already clear;
6. Output plain text query only.

### 6.4 MultiQueryGenerator

| Method | Description |
|---|---|
| `GenerateAsync(query, agent, count, cancellationToken)` | Async multi-query generation |
| `Generate(query, agent, count)` | Sync generation |

**Output parsing:**

- Split LLM output by lines;
- Strip prefixes like `1. `, `1、`, `- `;
- Deduplicate (case-insensitive);
- Insert original query at front if missing;
- Return at most `count` queries.

### 6.5 MmrSelector

| Method | Description |
|---|---|
| `Select(candidates, topK, lambda)` | MMR Top-K selection |

**Algorithm:**

```
MMR = λ × relevanceScore − (1 − λ) × maxSimilarity(selected)
```

- `relevanceScore`: original vector search score;
- `maxSimilarity`: max cosine similarity to already selected items;
- `lambda`: default `0.7` — higher favors relevance, lower favors diversity.

**Filtering:** Candidates with empty vectors (`Vector.Length == 0`) are excluded.

### 6.6 MmrCandidate

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Candidate ID |
| `Content` | `string` | Text content |
| `Score` | `float` | Original relevance score |
| `Vector` | `float[]` | Vector for diversity calculation |

---

## 7. API Examples

### 7.1 Ingestion: Chunk + Embed + Upsert

```csharp
using EasyCore.Agent.RAG;
using EasyCore.Vector.Redis;

const string collectionName = "knowledge_base";
const string vectorField = "contentVector";

var chunks = DocumentChunker.Chunk(documentText, documentId, 800, 100);

foreach (var chunk in chunks)
{
    var embedding = await agentClient.EmbedAsync(chunk.Content);

    var record = new RedisTextVector
    {
        Id = chunk.Id,
        DocumentId = chunk.DocumentId,
        Index = chunk.Index,
        StartIndex = chunk.StartIndex,
        EndIndex = chunk.EndIndex,
        Content = chunk.Content
    };

    record.SetVector(vectorField, embedding);
    await vectorStore.UpsertAsync(collectionName, record);
}
```

### 7.2 Retrieval: Rewrite → Embed → Search

```csharp
var history = deepSeekAgent.GetChatContext(sessionId);
var standaloneQuery = await QueryRewrite.RewriteAsync(userMessage, agent, history);

var queryVector = await agentClient.EmbedAsync(standaloneQuery);

var results = await vectorStore.VectorSearchAsync<RedisTextVector>(
    collectionName,
    vectorField,
    queryVector,
    new RedisVectorSearchOptions
    {
        Limit = 10,
        ScoreThreshold = 0.75f
    });
```

### 7.3 Multi-Query Retrieval

```csharp
var queries = await MultiQueryGenerator.GenerateAsync(userMessage, agent, count: 5);

var merged = new Dictionary<string, RedisVectorSearchResult<RedisTextVector>>();

foreach (var q in queries)
{
    var vector = await agentClient.EmbedAsync(q);
    var hits = await vectorStore.VectorSearchAsync<RedisTextVector>(
        collectionName, vectorField, vector,
        new RedisVectorSearchOptions { Limit = 5 });

    foreach (var hit in hits)
    {
        if (!merged.ContainsKey(hit.Record.Id) || merged[hit.Record.Id].Score < hit.Score)
            merged[hit.Record.Id] = hit;
    }
}

var topResults = merged.Values.OrderByDescending(x => x.Score).Take(10).ToList();
```

### 7.4 MMR + Agent Answer

```csharp
var mmrCandidates = topResults.Select(x => new MmrCandidate
{
    Id = x.Record.Id,
    Content = x.Record.Content,
    Score = x.Score,
    Vector = x.Record.GetVector(vectorField)
}).ToList();

var contextChunks = MmrSelector.Select(mmrCandidates, topK: 3, lambda: 0.7);

var context = string.Join("\n\n", contextChunks.Select(c => c.Content));

var answer = await agentClient.ChatRunAsync(
    sessionId,
    agent,
    $"Answer using the following context:\n\n{context}\n\nQuestion: {userMessage}");
```

### 7.5 Custom Prompts (QueryRewrite)

```csharp
var messages = QueryRewritePromptBuilder.Build(query, history);

var systemPrompt = QueryRewritePromptBuilder.GetSystemPrompt();
// Build custom messages from systemPrompt...
```

### 7.6 Custom Prompts (MultiQuery)

```csharp
var messages = MultiQueryPromptBuilder.Build(query, count: 5);

var systemPrompt = MultiQueryPromptBuilder.BuildSystemPrompt(count: 5);
var userPrompt = MultiQueryPromptBuilder.BuildUserPrompt(query, count: 5);
```

---

## 8. Full RAG Pipeline

![8-full-rag-pipeline](https://raw.githubusercontent.com/RockyWang0521/EasyCore.Agent/master/docs/svg/8-full-rag-pipeline-40aa053a.svg)


**Recommended combinations:**

| Scenario | Suggested modules |
|---|---|
| Single-turn FAQ | DocumentChunker + VectorSearch |
| Multi-turn knowledge base | + QueryRewrite |
| Low recall | + MultiQueryGenerator |
| High result redundancy | + MmrSelector |
| High precision needs | + external Reranker (integrate yourself) |

---

## 9. Best Practices

- ✅ **Match `chunkSize` to your embedding model**: For Chinese, 500–1000 characters is common; for English, estimate by tokens. Set `overlapSize` to roughly 10–20% of `chunkSize`.
- ✅ **Accumulate session history before rewrite**: Use `EasyCore.Agent`'s `GetChatContext(sessionId)` for full `ChatMessage` lists.
- ✅ **Merge and dedupe after multi-query**: Keep the highest score per `Record.Id` to avoid duplicate chunks in context.
- ✅ **MMR requires vectors**: Set `IncludeVector = true` during search, or map vectors into `MmrCandidate.Vector`.
- ✅ **Tune `lambda`**: Lower to `0.5–0.6` when the knowledge base has repetitive content; raise to `0.8–0.9` for precise matching.
- ✅ **Combine ScoreThreshold with MMR**: Filter low scores first, then run MMR for final selection.
- ⚠️ **QueryRewrite / MultiQuery use LLM calls**: Watch cost and latency; skip rewrite for simple questions.
- ⚠️ **DocumentChunker is character-based**: It does not respect Markdown headings or paragraph boundaries; consider pre-splitting long documents.

---

## 10. FAQ

### ❓ Q1: Does this library include vector storage?

No. Use `EasyCore.Vector.Redis`, `EasyCore.Vector.Qdrant`, or other vector packages for ingest and search.

### ❓ Q2: Do I need DI registration?

No. All APIs are static — reference the assembly and call directly.

### ❓ Q3: What kind of Agent does QueryRewrite need?

An `AIAgent` that supports `RunAsync(IEnumerable<ChatMessage>)`, typically created via `EasyCore.Agent`'s `CreateAgent(...)`.

### ❓ Q4: What if rewrite returns empty or throws?

`RewriteAsync` falls back to the original `query` when LLM output is empty. Wrap in try/catch and fall back similarly on errors.

### ❓ Q5: MMR returns fewer than `topK` items?

This happens when there are insufficient candidates or many lack valid vectors. Ensure enough search hits and `IncludeVector = true`.

### ❓ Q6: Is Reranker supported?

Cross-Encoder reranking is not built in. Integrate a third-party rerank service after `MmrSelector` if needed.

### ❓ Q7: Multi Query output is in the wrong language?

Prompts require the same language as the user question. If the model drifts, customize `MultiQueryPromptBuilder.BuildSystemPrompt` or filter in application code.

---

## 11. EasyCore.Agent.RAG in Depth

### 11.1 Design Goals

`EasyCore.Agent.RAG` focuses on **reusable RAG retrieval algorithms and prompt wrappers**, not reimplementing Agent or vector store capabilities. Principles:

1. **Lightweight & stateless**: static utilities, no global config, easy to test and compose;
2. **Storage-agnostic**: no reference to any `EasyCore.Vector.*` assembly;
3. **Agent collaboration**: Rewrite / MultiQuery call LLM through standard `AIAgent`;
4. **Enterprise extensibility**: Prompt builders are public for business-level overrides.

### 11.2 Type Map

```
EasyCore.Agent.RAG
├── DocumentChunker/
│   ├── DocumentChunker
│   └── DocumentChunk
├── QueryRewrite/
│   ├── QueryRewrite
│   └── QueryRewritePromptBuilder
├── MultiQueryGenerator/
│   ├── MultiQueryGenerator
│   └── MultiQueryPromptBuilder
└── MmrSelector/
    ├── MmrSelector
    └── MmrCandidate
```

### 11.3 Typical Rollout Steps

1. Reference `EasyCore.Agent.RAG` and your chosen `EasyCore.Vector.*`;
2. Register `EasyCore.Agent` and vector store DI;
3. Ingest: `DocumentChunker` → `EmbedAsync` → `UpsertAsync`;
4. Retrieve: `QueryRewrite` (optional) → `MultiQueryGenerator` (optional) → `VectorSearchAsync`;
5. Post-process: `MmrSelector.Select` → build context → `ChatRunAsync` for the answer.

---

## 12. Running the Demo

The `AspCoreAgent` demo's `EmbeddingController` includes RAG API examples.

### 12.1 Start the Demo

```bash
dotnet run --project demo/AspCoreAgent/AspCoreAgent.csproj
```

### 12.2 RAG Endpoints

| Endpoint | Description |
|---|---|
| `GET /api/Embedding/RagDocumentChunker` | Document chunking example |
| `GET /api/Embedding/RagQueryRewrite?message=...&sessionId=...` | Query rewrite with multi-turn context |
| `GET /api/Embedding/RagMultiQueryRetrieval?message=...` | Multi-query generation |

Vector store controllers (Redis, Qdrant, Milvus, etc.) expose `*MmrSelector` endpoints demonstrating **vector search + MMR** together.

---

## 📄 License

MIT OR Apache-2.0 (same as the EasyCore.Agent main repository)
