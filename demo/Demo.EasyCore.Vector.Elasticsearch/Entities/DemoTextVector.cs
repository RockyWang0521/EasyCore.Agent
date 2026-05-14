using EasyCore.Vector.Elasticsearch;

namespace Demo.EasyCore.Vector.Elasticsearch.Entities;

public class DemoTextVector : ElasticsearchVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;

    public long Index { get; set; }
}
