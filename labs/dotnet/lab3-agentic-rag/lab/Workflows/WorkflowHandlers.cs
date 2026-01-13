
namespace Lab3Solution.Workflows;

/// <summary>
/// Workflow event handlers and utilities.
/// </summary>
public static class WorkflowHandlers
{
    /// <summary>
    /// Collect all events from an async stream into a list.
    /// </summary>
    public static async Task<List<IWorkflowEvent>> DrainEventsAsync(IAsyncEnumerable<IWorkflowEvent> stream)
    {
        var events = new List<IWorkflowEvent>();
        await foreach (var evt in stream)
        {
            events.Add(evt);
        }
        return events;
    }

    /// <summary>
    /// Display agent responses since the last user message in a handoff request.
    /// </summary>
    public static void PrintAgentResponses(IHandoffUserInputRequest request)
    {
        if (request.Conversation == null || !request.Conversation.Any())
            return;

        // Reverse iterate to collect agent responses since last user message
        var agentResponses = new List<IChatMessage>();
        foreach (var message in request.Conversation.Reverse())
        {
            if (message.Role == Role.User)
                break;
            agentResponses.Add(message);
        }

        // Print agent responses in original order
        agentResponses.Reverse();
        foreach (var message in agentResponses)
        {
            var speaker = message.AuthorName ?? message.Role.ToString();
            Console.WriteLine($"  {speaker}: {message.Text}");
        }
    }

    /// <summary>
    /// Process workflow events and extract any pending user input requests.
    /// </summary>
    public static List<IRequestInfoEvent> HandleWorkflowEvents(List<IWorkflowEvent> events, bool verbose = true)
    {
        var requests = new List<IRequestInfoEvent>();

        foreach (var evt in events)
        {
            if (evt is IWorkflowStatusEvent statusEvent)
            {
                if (verbose && (statusEvent.State == WorkflowRunState.Idle ||
                               statusEvent.State == WorkflowRunState.IdleWithPendingRequests))
                {
                    Console.WriteLine($"\n[Workflow Status] {statusEvent.State}");
                }
            }
            else if (evt is IWorkflowOutputEvent outputEvent)
            {
                if (outputEvent.Data is List<IChatMessage> conversation && verbose)
                {
                    Console.WriteLine("\n" + new string('=', 60));
                    Console.WriteLine("FINAL CONVERSATION");
                    Console.WriteLine(new string('=', 60));
                    foreach (var message in conversation)
                    {
                        var speaker = message.AuthorName ?? message.Role.ToString();
                        Console.WriteLine($"{speaker}: {message.Text}");
                    }
                    Console.WriteLine(new string('=', 60));
                }
            }
            else if (evt is IRequestInfoEvent requestEvent)
            {
                if (requestEvent.Data is IHandoffUserInputRequest handoffRequest)
                {
                    PrintAgentResponses(handoffRequest);
                }
                requests.Add(requestEvent);
            }
        }

        return requests;
    }
}

// Placeholder interfaces for workflow types (these would come from the agent framework)
public interface IWorkflowEvent { }
public interface IWorkflowStatusEvent : IWorkflowEvent
{
    WorkflowRunState State { get; }
}
public interface IWorkflowOutputEvent : IWorkflowEvent
{
    object Data { get; }
}
public interface IRequestInfoEvent : IWorkflowEvent
{
    object Data { get; }
    string RequestId { get; }
}
public interface IHandoffUserInputRequest
{
    IEnumerable<IChatMessage>? Conversation { get; }
    string Prompt { get; }
}
public interface IChatMessage
{
    Role Role { get; }
    string? AuthorName { get; }
    string Text { get; }
}

public enum WorkflowRunState
{
    Idle,
    IdleWithPendingRequests,
    Running
}

public enum Role
{
    User,
    Assistant,
    System
}
