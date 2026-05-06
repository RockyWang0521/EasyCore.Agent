using System.ComponentModel;

namespace EasyCore.Agent
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ToolDescriptionAttribute : DescriptionAttribute
    {
        public ToolDescriptionAttribute(string description) : base(description)
        {
        }
    }
}
