using EasyCore.Vector.Redis;

namespace AspCoreAgent.VectorEntity
{
    public class RedisTextVector : RedisVectorRecord
    {
        public string DocumentId { get; set; } = string.Empty;

        public int Index { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
