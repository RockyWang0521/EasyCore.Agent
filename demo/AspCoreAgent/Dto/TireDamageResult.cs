namespace AspCoreAgent
{
    public class TireDamageResult
    {
        public bool HasDamage { get; set; }

        public List<TireDamageItem> Damages { get; set; } = [];
    }
}
