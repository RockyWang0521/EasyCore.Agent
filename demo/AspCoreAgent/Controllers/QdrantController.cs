using AspCoreAgent.Agent;
using AspCoreAgent.VectorEntity;
using EasyCore.Agent.RAG;
using EasyCore.Vector.Qdrant;
using Microsoft.AspNetCore.Mvc;

namespace AspCoreAgent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QdrantController : ControllerBase
    {
        private readonly QianwenAgent _agent;
        private readonly DeepSeekAgent _deepSeekAgent;
        private readonly IQdrantVectorStore _qdrantVectorStore;
        private const string documentVector = "documentVector";
        private const string sparseDocumentVector = "documentVector_sparse";

        public QdrantController(QianwenAgent agent,
               DeepSeekAgent deepSeekAgent,
               IQdrantVectorStore qdrantVectorStore)
        {
            _agent = agent;
            _deepSeekAgent = deepSeekAgent;
            _qdrantVectorStore = qdrantVectorStore;
        }

        [HttpGet("QdrantVectorStoreUpsert")]
        public async Task QdrantVectorStoreUpsert()
        {
            await _qdrantVectorStore.CreateCollectionAsync(
                    "test_collection",
                    new QdrantVectorCollectionDefinition
                    {
                        VectorFields =
                        {
                             new QdrantVectorFieldDefinition   { Name = documentVector,   Dimension = 1024   }
                        }
                    });

            var content = @"
         # EasyCore.Agent：基于 .NET 的企业级 AI Agent 开发框架设计与实践

## 前言

随着 ChatGPT、DeepSeek、Qwen、Claude、Gemini 等大语言模型的快速发展，人工智能已经从实验室阶段逐渐进入企业实际业务场景。

从最早的智能客服，到知识库问答，再到智能办公助手、代码生成、数据分析、自动审批、智能工作流等应用，大模型正在改变传统软件的开发模式。

对于 .NET 开发者而言，如何快速将大模型能力集成到现有系统中，如何构建具有工具调用能力的 Agent，如何实现企业知识库检索增强生成（RAG），如何实现多轮对话记忆管理，以及如何统一接入不同的大模型服务商，成为越来越多开发团队需要面对的问题。

目前市面上虽然已经存在一些 Agent 框架，例如 Semantic Kernel、LangChain、AutoGen 等，但这些框架大多存在以下问题：

* 学习成本较高；
* 设计理念偏向研究型项目；
* 与 .NET 企业级开发习惯存在差异；
* 扩展复杂；
* 对国产模型支持不够友好；
* 工具注册与调用方式繁琐；
* 不适合快速集成到现有业务系统。

基于这些实际需求，EasyCore.Agent 应运而生。

EasyCore.Agent 是一个专门面向 .NET 平台设计的企业级 AI Agent 开发框架，目标是让开发者能够像开发 WebAPI 一样开发 AI 应用。

框架坚持以下设计理念：

* 简单优先
* 企业优先
* 扩展优先
* 解耦优先
* 国产模型优先

开发者无需关注复杂的大模型细节，即可快速构建具备对话、工具调用、知识库检索、工作流执行等能力的智能 Agent 系统。

---

# 一、什么是 Agent

在介绍 EasyCore.Agent 之前，需要先理解 Agent 的概念。

很多人刚接触 AI 时，会把 Agent 理解成一个聊天机器人。

实际上并不是。

普通聊天模型的工作方式是：

```text
用户输入
↓
大模型推理
↓
返回结果
```

整个过程只有一次问答。

例如：

```text
用户：
今天上海天气怎么样？

模型：
今天上海晴天，气温28度。
```

模型只是回答问题。

而 Agent 不仅仅能够回答问题，还能够主动完成任务。

例如：

```text
用户：
帮我查询今天上海天气，并发送邮件给客户。
```

Agent 的执行流程可能是：

```text
用户输入
↓
理解任务
↓
调用天气工具
↓
获取天气数据
↓
调用邮件工具
↓
发送邮件
↓
返回结果
```

此时模型已经不仅仅是在聊天，而是在完成任务。

因此可以理解为：

```text
LLM = 大脑

Tool = 双手

Memory = 记忆

Agent = 大脑 + 双手 + 记忆
```

Agent 的核心目标是：

让 AI 从“回答问题”升级为“完成任务”。

---

# 二、EasyCore.Agent 的设计目标

EasyCore.Agent 并不是一个简单的大模型封装库。

它的目标是构建一个完整的 AI 应用开发平台。

主要解决以下问题：

## 统一模型接入

目前市面上的模型服务商非常多。

例如：

* OpenAI
* DeepSeek
* Qwen
* Doubao
* Gemini
* Claude
* Azure OpenAI

不同平台：

* API 地址不同
* 参数不同
* SDK 不同
* 返回格式不同

如果项目直接依赖厂商 SDK，未来更换模型成本非常高。

EasyCore.Agent 提供统一模型接口。

开发者只需要修改配置：

```json
{
  ""Model"":""deepseek-chat""
}
```

即可切换模型。

业务代码无需修改。

---

## 统一 Agent 管理

框架提供 Agent 对象作为核心入口。

例如：

```csharp
var agent = builder.Build();
```

后续所有能力：

* 聊天
* 工具调用
* RAG
* Memory
* Workflow

全部通过 Agent 完成。

统一开发体验。

---

## 企业级扩展能力

企业项目需要的不仅仅是聊天。

还需要：

* 权限控制
* 日志审计
* 数据隔离
* 多租户
* 工作流
* 知识库

EasyCore.Agent 从设计阶段就考虑企业需求。

因此所有模块均采用接口驱动设计。

开发者可以自由替换实现。

---

# 三、Tool 工具体系设计

Tool 是 Agent 最核心的能力之一。

没有 Tool 的 Agent 本质上只是聊天机器人。

EasyCore.Agent 提供统一 Tool 注册机制。

例如：

```csharp
[AITool]
public class WeatherTool
{
    public string GetWeather(string city)
    {
        return ""晴天"";
    }
}
```

注册：

```csharp
builder.AddTool<WeatherTool>();
```

当用户提问：

```text
查询北京天气
```

模型会自动判断是否需要调用工具。

执行：

```text
GetWeather(""北京"")
```

然后将结果返回给模型。

最终生成答案。

---

# 四、RAG 检索增强生成

企业知识库是 Agent 最重要的落地场景之一。

大模型无法知道企业内部数据。

因此需要通过 RAG 提供知识支持。

RAG 全称：

Retrieval-Augmented Generation

即：

检索增强生成。

完整流程如下：

```text
文档上传
↓
文档切块
↓
向量化
↓
向量存储
↓
用户提问
↓
向量检索
↓
相关文档召回
↓
大模型生成答案
```

---

## 文档切块

文档不能直接存入向量数据库。

需要拆分成多个 Chunk。

例如：

```text
10000字文档
↓
500字一个块
↓
20个Chunk
```

同时引入重叠机制：

```text
Chunk1
Chunk2
Chunk3
```

每个 Chunk 保留部分上下文。

避免语义断裂。

---

## Query Rewrite

用户提问往往不够准确。

例如：

```text
它什么时候审批的？
```

这里的“它”是谁？

需要结合上下文重写问题。

例如：

```text
张三提交的请假单是什么时候审批的？
```

重写后的问题更容易检索。

---

## Multi Query

单次查询可能召回不足。

因此框架支持：

```text
一个问题
↓
生成多个问题
↓
多路检索
↓
结果合并
```

例如：

```text
如何申请年假？
```

生成：

```text
年假申请流程
请假审批流程
员工休假制度
```

召回率显著提高。

---

## Similarity Search

最基础检索方式。

通过向量相似度寻找最相关文档。

例如：

```text
Top10
```

返回最相似的十条记录。

---

## Similarity Threshold

设置最低相似度。

例如：

```text
Score >= 0.7
```

低于阈值直接过滤。

提高结果质量。

---

## MMR 检索

MMR：

Maximum Marginal Relevance

最大边际相关性。

作用：

避免召回大量重复内容。

例如：

```text
Chunk1
Chunk2
Chunk3
```

三条内容几乎相同。

MMR 会自动去重。

提升上下文多样性。

---

## Reranker 重排序

向量检索速度快。

但准确率有限。

因此引入：

Cross Encoder

重排序模型。

流程：

```text
召回50条
↓
Rerank
↓
保留5条
```

显著提高答案准确率。

---

# 五、向量数据库抽象设计

EasyCore.Agent 不绑定具体数据库。

统一抽象：

```csharp
IVectorStore
```

支持：

```text
Qdrant
MilvusTextVector
Chroma
PGVector
Faiss
```

开发者可以自由切换。

业务代码无需修改。

这也是框架解耦思想的重要体现。

---

# 六、Memory 记忆体系

Agent 必须具备记忆能力。

否则无法实现连续对话。

例如：

```text
用户：
我叫 Rocky

用户：
我是谁？

模型：
你叫 Rocky
```

记忆分为：

短期记忆：

```text
当前会话
```

长期记忆：

```text
数据库存储
```

未来 EasyCore.Agent 将支持：

* Redis
* SQLServer
* PostgreSQL
* MongoDB

多种记忆存储方案。

---

# 七、Workflow 与 Agent 融合

传统审批流：

```text
发起
↓
审核
↓
通过
```

未来可以结合 Agent。

例如：

```text
发票上传
↓
Agent识别
↓
自动审核
↓
工作流流转
```

实现智能审批。

EasyCore.Agent 与 WorkflowCore 的结合将成为企业场景的重要方向。

---

# 八、框架未来规划

未来 EasyCore.Agent 将持续完善以下能力：

第一阶段：

* Tool
* Chat
* RAG
* Memory

第二阶段：

* Workflow Agent
* Multi Agent
* Planning Agent

第三阶段：

* Agent Marketplace
* Agent Studio
* Visual Workflow

最终目标：

打造属于 .NET 生态的企业级 Agent 开发平台。

---

# 总结

AI 正在改变软件行业。

未来的软件系统将不仅仅由数据库、接口和页面组成。

Agent 将成为新的应用入口。

EasyCore.Agent 希望为 .NET 开发者提供一套简单、高效、可扩展的 Agent 开发框架。

开发者无需研究复杂的 AI 理论，也无需深度理解模型底层实现，只需要专注业务本身，即可快速构建属于自己的智能应用。

从 Chat 到 Tool，从 RAG 到 Workflow，从 Memory 到 Multi-Agent。

EasyCore.Agent 的目标始终只有一个：

让 AI 开发回归简单，让每一个 .NET 开发者都能轻松构建企业级 Agent 应用。

            ";

            var chunks = DocumentChunker.Chunk(content, "documentId", 800, 100);

            var agent = _agent.CreateEmbeddingClient();

            foreach (var chunk in chunks)
            {
                var embedding = await _agent.EmbedAsync(chunk.Content);

                var textVector = new QdrantTextVector
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = chunk.DocumentId,
                    Index = chunk.Index,
                    StartIndex = chunk.StartIndex,
                    EndIndex = chunk.EndIndex,
                    Content = chunk.Content,
                    Metadata = new Dictionary<string, object>()
                };

                textVector.SetVector(documentVector, embedding);

                await _qdrantVectorStore.UpsertAsync("test_collection", textVector);
            }
        }

        [HttpGet("QdrantVectorStoreSearch")]
        public async Task<IReadOnlyList<QdrantQdrantVectorSearchResult<QdrantTextVector>>> QdrantVectorStoreSearch()
        {
            var result = await _agent.EmbedAsync("easycore.agent支持哪些功能？");

            var options = new QdrantVectorSearchOptions
            {
                Limit = 10,
                ScoreThreshold = 0.0f,
                Filter = new QdrantVectorFilter
                {
                    Conditions =
                    {
                        new QdrantVectorFilterCondition
                        {
                            Field = "Index",
                            Operator = QdrantVectorFilterOperator.GreaterThanOrEqual,
                            Value = 1
                        }
                    }
                },
                IncludeVector = false,
                IncludeMetadata = true
            };

            var searchResults = await _qdrantVectorStore.VectorSearchAsync<QdrantTextVector>(
                collectionName: "test_collection",
                vectorName: documentVector,
                vector: result,
                options: options);

            return searchResults;
        }

        [HttpGet("QdrantVectorStoreSparseSearch")]
        public async Task<IReadOnlyList<QdrantQdrantVectorSearchResult<QdrantTextVector>>> QdrantVectorStoreSparseSearch()
        {
            var sparseVector = new SparseVectorValue
            {
                Indices = new List<uint> { 12, 88, 391 },
                Values = new List<float> { 1.2f, 0.7f, 2.4f }
            };

            var options = new QdrantVectorSearchOptions
            {
                Limit = 10,
                ScoreThreshold = 0.0f,
                Filter = new QdrantVectorFilter
                {
                    Conditions =
                    {
                        new QdrantVectorFilterCondition
                        {
                            Field = "Index",
                            Operator = QdrantVectorFilterOperator.GreaterThanOrEqual,
                            Value = 1
                        }
                    }
                },
                IncludeMetadata = true,
                IncludeVector = false
            };

            var searchResults = await _qdrantVectorStore.SparseSearchAsync<QdrantTextVector>(
                collectionName: "test_collection",
                vectorName: sparseDocumentVector,
                sparseVector: sparseVector,
                options: options);

            return searchResults;
        }

        [HttpGet("QdrantVectorStoreHybridSearch")]
        public async Task<IReadOnlyList<QdrantQdrantVectorSearchResult<QdrantTextVector>>> QdrantVectorStoreHybridSearch()
        {
            var queryVector = await _agent.EmbedAsync("easycore.agent支持哪些功能？");

            var sparseVector = new SparseVectorValue
            {
                Indices = new List<uint> { 12, 88, 391 },
                Values = new List<float> { 1.2f, 0.7f, 2.4f }
            };

            var options = new QdrantVectorSearchOptions
            {
                Limit = 10,
                ScoreThreshold = 0.0f,
                Filter = new QdrantVectorFilter
                {
                    Conditions =
                    {
                        new QdrantVectorFilterCondition
                        {
                            Field = "Index",
                            Operator = QdrantVectorFilterOperator.GreaterThanOrEqual,
                            Value = 1
                        }
                    }
                },
                IncludeMetadata = true,
                IncludeVector = false
            };

            var searchResults = await _qdrantVectorStore.HybridSearchAsync<QdrantTextVector>(
                collectionName: "test_collection",
                denseVectorName: documentVector,
                denseVector: queryVector,
                sparseVectorName: sparseDocumentVector,
                sparseVector: sparseVector,
                options: options,
                denseWeight: 0.7f,
                sparseWeight: 0.3f);

            return searchResults;
        }

        [HttpGet("QdrantVectorStoreMmrSelector")]
        public async Task<List<MmrCandidate>> QdrantVectorStoreMmrSelector()
        {
            var queryVector = await _agent.EmbedAsync("easycore.agent支持哪些功能？");

            var options = new QdrantVectorSearchOptions
            {
                Limit = 20,
                ScoreThreshold = 0.6f,
                IncludeVector = true,
                IncludeMetadata = true
            };

            var candidates = await _qdrantVectorStore.VectorSearchAsync<QdrantTextVector>(
                collectionName: "test_collection",
                vectorName: documentVector,
                vector: queryVector,
                options: options);

            var mmrCandidates = candidates.Select(x => new MmrCandidate
            {
                Id = x.Record.Id,
                Content = x.Record.Content,
                Score = x.Score,
                Vector = x.Record.GetVector(documentVector)
            }).ToList();

            var finalResults = MmrSelector.Select(
                candidates: mmrCandidates,
                topK: 2,
                lambda: 0.7);

            return finalResults;
        }

        [HttpGet("QdrantVectorStoreDelete")]
        public async Task QdrantVectorStoreDelete([FromQuery] string id)
        {
            await _qdrantVectorStore.DeleteAsync("test_collection", id);
        }

        [HttpGet("QdrantVectorStoreCollectionExists")]
        public async Task<bool> QdrantVectorStoreCollectionExists()
        {
            return await _qdrantVectorStore.CollectionExistsAsync("test_collection");
        }

        [HttpGet("QdrantVectorStoreDeleteCollection")]
        public async Task QdrantVectorStoreDeleteCollection()
        {
            await _qdrantVectorStore.DeleteCollectionAsync("test_collection");
        }
    }
}
