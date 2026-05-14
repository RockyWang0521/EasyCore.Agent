namespace AspCoreAgent
{
    public class TireDamageItem
    {
        public string Type { get; set; } = string.Empty;

        public double Confidence { get; set; }

        public TireDamageBox Box { get; set; } = new();
    }
}
