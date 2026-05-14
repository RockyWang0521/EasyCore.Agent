using EasyCore.Vector.Redis;

namespace Demo.EasyCore.Vector.Redis.Entities;

public class DemoTextVector : RedisVectorRecord
{
    public string DocumentId { get; set; } = string.Empty;

    public int Index { get; set; }
}
