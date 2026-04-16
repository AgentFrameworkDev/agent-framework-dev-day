using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Lab3.Workflows;

/// <summary>
/// Structured classification result returned by the classifier agent.
/// </summary>
public sealed class ClassifyResult
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = "";
}

/// <summary>
/// Carries both the classification category and original user question for routing.
/// </summary>
public sealed class ClassifiedQuery
{
    public string Category { get; set; } = "";
    public string UserQuestion { get; set; } = "";
}

/// <summary>
/// Executor that wraps the classifier agent, invokes it, parses the structured JSON output,
/// and returns a ClassifiedQuery for switch-case routing.
/// </summary>
internal sealed class ClassifierExecutor : Executor<ChatMessage, ClassifiedQuery>
{
    private readonly AIAgent _classifierAgent;

    public ClassifierExecutor(AIAgent classifierAgent) : base("ClassifierExecutor")
    {
        _classifierAgent = classifierAgent;
    }

    public override async ValueTask<ClassifiedQuery> HandleAsync(
        ChatMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var response = await _classifierAgent.RunAsync(message, cancellationToken: cancellationToken);
        var text = response.Text?.Trim() ?? "";

        // Strip markdown code fences if present
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0)
            {
                text = text[(firstNewline + 1)..];
            }
            var lastFence = text.LastIndexOf("```");
            if (lastFence >= 0)
            {
                text = text[..lastFence].Trim();
            }
        }

        // Extract the first complete JSON object using brace-depth counting
        int braceDepth = 0;
        int jsonEnd = -1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{') braceDepth++;
            else if (text[i] == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    jsonEnd = i + 1;
                    break;
                }
            }
        }
        if (jsonEnd > 0)
        {
            text = text[..jsonEnd];
        }

        var result = JsonSerializer.Deserialize<ClassifyResult>(text, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ClassifyResult { Category = "semantic_search", Reasoning = "Failed to parse classification" };

        Console.WriteLine($"  [Classified as: {result.Category} -- {result.Reasoning}]");

        return new ClassifiedQuery
        {
            Category = result.Category,
            UserQuestion = message.Text ?? ""
        };
    }
}

/// <summary>
/// Generic executor that wraps a specialist AIAgent, invokes it with the user question
/// from a ClassifiedQuery, and yields the agent's response as the workflow output.
/// </summary>
[YieldsOutput(typeof(string))]
internal sealed class SpecialistExecutor : Executor<ClassifiedQuery>
{
    private readonly AIAgent _agent;

    public SpecialistExecutor(string name, AIAgent agent) : base(name)
    {
        _agent = agent;
    }

    public override async ValueTask HandleAsync(
        ClassifiedQuery query,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"\n  [{_agent.Name ?? Id}] processing: {query.UserQuestion}");

        var response = await _agent.RunAsync(query.UserQuestion, cancellationToken: cancellationToken);

        Console.WriteLine();
        await context.YieldOutputAsync(response.Text ?? "No response generated.", cancellationToken);
    }
}

/// <summary>
/// Helper to create switch-case condition functions for category routing.
/// </summary>
public static class CategoryConditions
{
    public static Func<object?, bool> Is(string category) =>
        result => result is ClassifiedQuery q &&
                  string.Equals(q.Category, category, StringComparison.OrdinalIgnoreCase);
}
