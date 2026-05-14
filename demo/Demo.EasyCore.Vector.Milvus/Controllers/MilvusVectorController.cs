using Demo.Common;
using Demo.Common.Agent;
using Demo.Common.Dto;
using Demo.EasyCore.Vector.Milvus.Entities;
using EasyCore.Agent.RAG;
using EasyCore.Vector.Milvus;
using Microsoft.AspNetCore.Mvc;
using Milvus.Client;

namespace Demo.EasyCore.Vector.Milvus.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MilvusVectorController : ControllerBase
{
    private const string CollectionName = "demo_milvus_collection";
    private const string VectorName = "documentVector";

    private readonly QianwenAgent _agent;
    private readonly IMilvusVectorStore _store;

    public MilvusVectorController(QianwenAgent agent, IMilvusVectorStore store)
    {
        _agent = agent;
        _store = store;
    }

    [HttpGet("upsert")]
    public async Task<object> Upsert()
    {
        await _store.CreateCollectionAsync(CollectionName, new MilvusVectorCollectionDefinition
        {
            ScalarFields =
            {
                new MilvusScalarFieldDefinition { Name = "DocumentId", FieldType = ScalarFieldType.VarChar, MaxLength = 128 },
                new MilvusScalarFieldDefinition { Name = "Index", FieldType = ScalarFieldType.Int64 }
            },
            VectorFields =
            {
                new MilvusVectorFieldDefinition
                {
                    Name = VectorName,
                    Dimension = 1024,
                    MetricType = SimilarityMetricType.Cosine
                }
            }
        });

        var chunks = DocumentChunker.Chunk(DemoDocument.Content, DemoDocument.DocumentId, 120, 30);
        var count = 0;

        foreach (var chunk in chunks)
        {
            var embedding = await _agent.EmbedAsync(chunk.Content);
            var record = new DemoTextVector
            {
                Id = Guid.NewGuid().ToString("N"),
                DocumentId = chunk.DocumentId,
                Index = chunk.Index,
                Content = chunk.Content
            };
            record.SetVector(VectorName, embedding);
            await _store.UpsertAsync(CollectionName, record);
            count++;
        }

        return new { collection = CollectionName, upserted = count };
    }

    [HttpGet("search")]
    public async Task<IReadOnlyList<MilvusVectorSearchResult<DemoTextVector>>> Search([FromQuery] VectorSearchRequest request)
    {
        var vector = await _agent.EmbedAsync(request.Query);
        return await _store.VectorSearchAsync<DemoTextVector>(
            CollectionName,
            VectorName,
            vector,
            new MilvusVectorSearchOptions { Limit = request.Limit, IncludeMetadata = true, ScoreThreshold = 0.5f });
    }

    [HttpGet("exists")]
    public Task<bool> Exists() => _store.CollectionExistsAsync(CollectionName);

    [HttpDelete("collection")]
    public async Task DeleteCollection() => await _store.DeleteCollectionAsync(CollectionName);
}
