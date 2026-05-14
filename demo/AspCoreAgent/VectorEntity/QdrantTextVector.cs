using EasyCore.Vector.Qdrant;

namespace AspCoreAgent.VectorEntity
{
    public class QdrantTextVector : QdrantVectorRecord
    {
        public string DocumentId { get; set; } = string.Empty;

        public int Index { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
