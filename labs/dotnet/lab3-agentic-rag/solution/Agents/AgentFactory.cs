using Lab3.Services;
using Microsoft.Agents.AI;
using OpenAI.Chat;


namespace Lab3.Agents;

/// <summary>
/// Factory for creating specialized agents.
/// </summary>
public class AgentFactory
{
    private readonly ChatClient _chatClient;
    private readonly SearchService _searchService;

    public AgentFactory(ChatClient chatClient, SearchService searchService)
    {
        _chatClient = chatClient;
        _searchService = searchService;
    }

    /// <summary>
    /// Create all agents needed for the system.
    /// </summary>
    public Dictionary<string, AIAgent> CreateAllAgents()
    {
        return new Dictionary<string, AIAgent>
        {
            ["classifier"] = ClassifierAgent.Create(_chatClient),
            ["yes_no"] = YesNoAgent.Create(_chatClient, _searchService),
            ["semantic_search"] = SemanticSearchAgent.Create(_chatClient, _searchService),
            ["count"] = CountAgent.Create(_chatClient, _searchService),
            ["comparative"] = ComparativeAgent.Create(_chatClient, _searchService),
            ["difference"] = DifferenceAgent.Create(_chatClient, _searchService),
            ["intersection"] = IntersectionAgent.Create(_chatClient, _searchService),
            ["multi_hop"] = MultiHopAgent.Create(_chatClient, _searchService),
        };
    }
}
