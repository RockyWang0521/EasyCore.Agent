using EasyCore.Vector.Elasticsearch;

namespace AspCoreAgent.VectorEntity
{
    public class ElasticsearchTextVector : ElasticsearchVectorRecord
    {
        public string DocumentId { get; set; } = string.Empty;

        public int Index { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
