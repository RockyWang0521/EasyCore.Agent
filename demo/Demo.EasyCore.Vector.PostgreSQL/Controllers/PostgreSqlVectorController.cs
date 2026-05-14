using Demo.Common;
using Demo.Common.Agent;
using Demo.Common.Dto;
using Demo.EasyCore.Vector.PostgreSQL.Entities;
using EasyCore.Agent.RAG;
using EasyCore.Vector.PostgreSQL;
using Microsoft.AspNetCore.Mvc;

namespace Demo.EasyCore.Vector.PostgreSQL.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostgreSqlVectorController : ControllerBase
{
    private const string CollectionName = "demo_pg_collection";
    private const string VectorName = "documentVector";

    private readonly QianwenAgent _agent;
    private readonly IPostgreSqlVectorStore _store;

    public PostgreSqlVectorController(QianwenAgent agent, IPostgreSqlVectorStore store)
    {
        _agent = agent;
        _store = store;
    }

    [HttpGet("upsert")]
    public async Task<object> Upsert()
    {
        await _store.CreateCollectionAsync(CollectionName, new PostgreSqlVectorCollectionDefinition
        {
            ScalarFields =
            {
                new PostgreSqlScalarFieldDefinition { Name = "DocumentId", FieldType = ScalarFieldType.VarChar, MaxLength = 128 },
                new PostgreSqlScalarFieldDefinition { Name = "Index", FieldType = ScalarFieldType.Int64 }
            },
            VectorFields =
            {
                new PostgreSqlVectorFieldDefinition
                {
                    Name = VectorName,
                    Dimension = 1024,
                    MetricType = PostgreSqlSimilarityMetricType.Cosine
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
    public async Task<IReadOnlyList<PostgreSqlVectorSearchResult<DemoTextVector>>> Search([FromQuery] VectorSearchRequest request)
    {
        var vector = await _agent.EmbedAsync(request.Query);
        return await _store.VectorSearchAsync<DemoTextVector>(
            CollectionName,
            VectorName,
            vector,
            new PostgreSqlVectorSearchOptions { Limit = request.Limit, IncludeMetadata = true, ScoreThreshold = 0.5f });
    }

    [HttpGet("exists")]
    public Task<bool> Exists() => _store.CollectionExistsAsync(CollectionName);

    [HttpDelete("collection")]
    public async Task DeleteCollection() => await _store.DeleteCollectionAsync(CollectionName);
}
