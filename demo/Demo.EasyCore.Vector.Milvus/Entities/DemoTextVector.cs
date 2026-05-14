using EasyCore.Vector.Milvus;
using Milvus.Client;

namespace Demo.EasyCore.Vector.Milvus.Entities;

public class DemoTextVector : MilvusVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;

    public long Index { get; set; }
}
