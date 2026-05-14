using EasyCore.Vector.PostgreSQL;

namespace Demo.EasyCore.Vector.PostgreSQL.Entities;

public class DemoTextVector : PostgreSqlVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;

    public int Index { get; set; }
}
