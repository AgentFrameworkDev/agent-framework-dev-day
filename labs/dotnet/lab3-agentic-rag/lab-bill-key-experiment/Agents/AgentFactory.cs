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
            ["semantic_search"] = SemanticSearchAgent.Create(_chatClient, _searchService)
            // TODO: Add other agents here as needed
        };
    }
}
