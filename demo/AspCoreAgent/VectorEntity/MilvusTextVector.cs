using EasyCore.Vector.Milvus;

namespace AspCoreAgent.VectorEntity
{
    public class MilvusTextVector : MilvusVectorRecord
    {
        public string DocumentId { get; set; } = string.Empty;

        public int Index { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
