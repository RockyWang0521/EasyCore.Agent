namespace AspCoreAgent.Route
{
    public class AgentRouteDecision
    {
        public AgentRouteType RouteType { get; set; }

        public string? ToolName { get; set; }

        public string? WorkflowName { get; set; }

        public string Reason { get; set; } = "";

        public string UserQuestion { get; set; } = "";
    }
}
