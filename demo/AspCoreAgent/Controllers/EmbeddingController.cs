using AspCoreAgent.Agent;
using EasyCore.Agent.RAG;
using Microsoft.AspNetCore.Mvc;

namespace AspCoreAgent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmbeddingController : ControllerBase
    {
        private readonly QianwenAgent _agent;
        private readonly DeepSeekAgent _deepSeekAgent;

        public EmbeddingController(QianwenAgent agent,
               DeepSeekAgent deepSeekAgent)
        {
            _agent = agent;
            _deepSeekAgent = deepSeekAgent;
        }

        [HttpGet("GetEmbedding")]
        public async Task GetEmbedding()
        {
            var agent = _agent.CreateEmbeddingClient();

            var result = await agent.GenerateEmbeddingAsync("今天天气怎么样");

            var result1 = await _agent.EmbedAsync("今天天气怎么样");

            ReadOnlyMemory<float> vector = result.Value.ToFloats();

            await Task.CompletedTask;
        }

        [HttpGet("RagQueryRewrite")]
        public async Task<string> RagQueryRewrite(string message, string sessionId = "123456")
        {
            var agent = _deepSeekAgent.CreateAgent();

            await _deepSeekAgent.ChatRunAsync(sessionId, agent, message);

            var history = _deepSeekAgent.GetChatContext(sessionId);

            return await QueryRewrite.RewriteAsync(message, agent, history);
        }

        [HttpGet("RagDocumentChunker")]
        public List<DocumentChunk> RagDocumentChunker(string documentId = "documentId", int chunkSize = 800, int overlapSize = 100)
        {
            var content = @"
            # EasyCore.Agent: A Modern AI Agent Framework for .NET Developers
            
                    EasyCore.Agent is a lightweight, extensible, and developer-friendly AI Agent framework designed specifically for the.NET ecosystem.Its primary goal is to simplify  the         process   of  building intelligent applications powered by Large Language Models (LLMs) while maintaining flexibility, performance, and ease of integration.
            
                    As artificial intelligence becomes an increasingly important part of modern software development, developers often face a common challenge: connecting language          models,    tools, memory systems, retrieval pipelines, and business logic into a single maintainable architecture. EasyCore.Agent was created to solve this problem   by        providing a clean    and unified framework that allows developers to focus on business requirements rather than infrastructure complexity.
            
                    ## Why EasyCore.Agent?
            
                    Many AI frameworks are either too complex for enterprise applications or too tightly coupled to a specific model provider.Developers frequently need to write     repetitive       code   for chat completion, tool invocation, conversation management, document retrieval, embedding generation, and workflow orchestration.
            
                    EasyCore.Agent addresses these issues through a modular architecture that supports multiple AI providers and allows developers to build production-ready AI   systems       with     minimal effort.
                        
                    The framework follows several core principles:
            
                    * Simplicity over complexity
                    * Extensibility over rigidity
                    * Provider independence
                    * Enterprise readiness
                    * Developer productivity
            
                    These principles make EasyCore.Agent suitable for both small projects and large-scale enterprise solutions.
            
            ## Core Architecture
            
                    The framework is built around the concept of an AI Agent. An agent represents an intelligent entity capable of understanding user requests, reasoning abouttasks,          invoking     tools, accessing memory, and generating responses.
            
                    A typical workflow includes:
            
                    1. Receiving user input
                            2. Processing conversation history
                            3. Rewriting queries when necessary
                    4. Retrieving relevant knowledge
                            5. Calling external tools
                            6. Generating final responses
                            7. Persisting memory and context

                    This architecture allows developers to create intelligent assistants, customer service systems, knowledge bases, workflow automation platforms, and domain-specific     AI         applications.
            
            ## Tool Calling System
            
                    One of the most important features of EasyCore.Agent is its tool calling mechanism.
            
                    Modern language models are powerful, but they cannot directly access databases, APIs, file systems, or business services.EasyCore.Agent bridges this gap by  providing     a        structured tool framework.
            
                    Developers can expose existing business methods as AI tools and allow the language model to invoke them automatically when appropriate.
            
                    Examples include:
            
                    * Querying customer information
                    * Reading inventory data
                    * Creating orders
                    * Sending notifications
                    * Generating reports
                    * Accessing internal APIs
            
                    The framework handles tool registration, parameter binding, execution, and result processing, significantly reducing development effort.
            
            ## Memory Management
            
                    Context is essential for intelligent conversations.
            
                    EasyCore.Agent provides conversation memory capabilities that allow agents to maintain awareness across multiple interactions.
            
                    Instead of treating every request as an isolated event, the framework can preserve historical messages and use them to generate more accurate responses.
            
                    Memory can be applied to:
            
                    * Customer support systems
                    * Personal assistants
                    * Workflow approvals
                    * Enterprise knowledge systems
                    * Multi-step task execution
            
                    By maintaining context, agents can understand references such as ""that project,"" ""the previous order,"" or ""the last question"" without requiring users to repeat           information.
            
                    ## Retrieval-Augmented Generation
            
                    Retrieval-Augmented Generation, commonly known as RAG, is a key capability of EasyCore.Agent.
            
                    The framework supports the complete RAG pipeline:
            
                    * Document ingestion
                    * Document chunking
                    * Embedding generation
                    * Vector storage
                    * Similarity search
                    * Reranking
                    * Context construction
                    * Response generation
            
                    This enables developers to build AI systems that can answer questions based on private knowledge rather than relying solely on model training data.
            
                    For example, organizations can upload:
            
                    * Technical documentation
                    * Product manuals
                    * Internal procedures
                    * Knowledge base articles
                    * Training materials
                    * Business regulations
            
                    The agent can then retrieve relevant information and provide accurate answers grounded in enterprise knowledge.
            
            ## Query Rewriting
            
                    A common challenge in RAG systems is that user questions are often incomplete or ambiguous.
            
                    EasyCore.Agent includes support for query rewriting, which transforms conversational questions into standalone search queries.
            
                    For example:
            
                    User question:
            
                    ""How does it work?""
            
                    Rewritten query:
            
                    ""How does EasyCore.Agent perform document retrieval in a RAG workflow?""
            
                    This process significantly improves retrieval quality and helps vector databases return more relevant results.
            
            ## Embedding Integration
            
                    Semantic search relies on embeddings.
            
                    EasyCore.Agent supports embedding generation through multiple AI providers, allowing text to be converted into high-dimensional vectors.
            
                    These vectors capture semantic meaning rather than simple keyword matching.
            
                    As a result, searches become more intelligent and can identify conceptually related information even when exact keywords do not match.
            
                    Embedding support enables:
            
                    * Semantic search
                    * Similarity matching
                    * Document clustering
                    * Recommendation systems
                    * Knowledge retrieval
            
                    ## Flexible Provider Support
            
                    Organizations often use different AI providers depending on cost, performance, availability, or compliance requirements.
            
                    EasyCore.Agent is designed to be provider-agnostic.
            
                    Developers can integrate various model providers without rewriting business logic.
            
                    This flexibility protects long-term investments and prevents vendor lock-in.
            
                    Whether using cloud-hosted models or self-hosted deployments, the framework provides a consistent programming experience.
            
            ## Enterprise-Oriented Design
            
                    Enterprise applications require more than simple chatbot functionality.
            
                    They demand reliability, maintainability, security, and scalability.
            
                    EasyCore.Agent was designed with these requirements in mind.
            
                    Important enterprise features include:
            
                    * Dependency Injection support
                    * Modular architecture
                    * Configurable components
                    * Structured logging
                    * Extensible pipelines
                    * Workflow integration
                    * Distributed deployment compatibility
            
                    These capabilities make the framework suitable for production environments where stability and maintainability are critical.
            
                    ## Developer Experience
            
                    A major objective of EasyCore.Agent is improving developer productivity.
            
                    The framework minimizes boilerplate code and provides intuitive APIs that align with modern.NET development practices.
            
                    Developers can quickly:
            
                    * Create agents
                    * Register tools
                    * Connect AI providers
                    * Configure memory
                    * Build RAG pipelines
                    * Extend framework behavior
            
                    This allows teams to focus on solving business problems rather than building infrastructure from scratch.
            
                    ## Future Vision
            
                    The future of software development is increasingly AI-driven.
            
                    Applications are evolving from static systems into intelligent platforms capable of understanding user intent, reasoning about tasks, and interacting with external       systems       autonomously.
            
                    EasyCore.Agent aims to become a comprehensive AI development platform for the.NET ecosystem.
            
                    Future directions include:
            
                    * Advanced workflow orchestration
                    * Multi-agent collaboration
                    * Agent-to-agent communication
                    * Enhanced memory systems
                    * Improved RAG capabilities
                    * Visual workflow design
                    * Expanded provider integrations
            
                    These enhancements will help developers build increasingly sophisticated AI-powered applications while maintaining simplicity and productivity.
            
                    ## Conclusion
            
                    EasyCore.Agent is more than a wrapper around language models.It is a complete framework designed to help .NET developers build practical, scalable, and     maintainable     AI     solutions.
            
                    By combining tool calling, memory management, retrieval-augmented generation, query rewriting, embedding integration, and enterprise-focused architecture,       EasyCore.      Agent provides a solid foundation for modern AI application development.
            
                    As artificial intelligence continues to transform the software industry, frameworks such as EasyCore.Agent make advanced AI capabilities accessible to everyday developers, enabling them to create intelligent systems that deliver real business value.
            ";

            return DocumentChunker.Chunk(content, documentId, chunkSize, overlapSize);
        }

        [HttpGet("RagMultiQueryRetrieval")]
        public async Task<List<string>> RagMultiQueryRetrieval(string message)
        {
            var agent = _deepSeekAgent.CreateAgent();

            return await MultiQueryGenerator.GenerateAsync(message, agent, 5);
        }
    }
}
