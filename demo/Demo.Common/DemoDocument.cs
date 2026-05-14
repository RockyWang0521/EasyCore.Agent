namespace Demo.Common;

/// <summary>
/// Shared sample document content for demo projects.
/// </summary>
public static class DemoDocument
{
    public const string DocumentId = "easycore-demo-doc";

    public const string Content = """
        EasyCore.Agent is an enterprise AI Agent framework for .NET with Tool calling, Memory, RAG, and workflow orchestration.
        It provides a unified model layer so you can switch between DeepSeek, Qwen, and other providers without changing business code.
        The RAG module supports document chunking, Query Rewrite, Multi Query, MMR deduplication, and hybrid search.
        The Pipeline module supports sequential, branch, and parallel step orchestration.
        The Workflow module supports in-step event publish/wait, pause, terminate, and node jump.
        Vector storage is available via Qdrant, Milvus, PostgreSQL, Redis, and Elasticsearch implementations.
        """;
}
