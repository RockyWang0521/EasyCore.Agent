using EasyCore.Vector.Qdrant;

namespace Demo.EasyCore.Vector.Qdrant.Entities;

public class DemoTextVector : QdrantVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;

    public int Index { get; set; }
}
