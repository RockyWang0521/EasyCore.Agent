using EasyCore.Vector.PostgreSQL;

namespace AspCoreAgent.VectorEntity
{
    public class PostgreSqlTextVector : PostgreSqlVectorRecord
    {
        public string DocumentId { get; set; } = string.Empty;

        public int Index { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}
