namespace EasyCore.Agent
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AIToolAttribute : System.Attribute
    {
        public string AIToolName { get; }

        public AIToolAttribute(string aiToolName)
        {
            AIToolName = aiToolName;
        }
    }
}
