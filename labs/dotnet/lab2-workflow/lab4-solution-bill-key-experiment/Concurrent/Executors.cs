using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace WorkflowLab.Concurrent;

/// <summary>
/// Executor that starts the concurrent processing by broadcasting messages to all connected agents.
/// </summary>
internal sealed class ConcurrentStartExecutor() : Executor<string>("ConcurrentStart")
{
    public override async ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Broadcasting question to all experts...");
        Console.WriteLine();

        // Broadcast the message to all connected agents
        await context.SendMessageAsync(new ChatMessage(ChatRole.User, message), cancellationToken);

        // Broadcast the turn token to kick off the agents
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
    }
}

/// <summary>
/// Executor that aggregates the results from multiple concurrent agents.
/// </summary>
internal sealed class ConcurrentAggregationExecutor() : Executor<List<ChatMessage>>("ConcurrentAggregation")
{
    private readonly List<ChatMessage> _messages = [];

    public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        _messages.AddRange(messages);

        // Wait for responses from both agents (2 in this demo)
        if (_messages.Count >= 2)
        {
            var formattedMessages = string.Join(
                Environment.NewLine + Environment.NewLine,
                _messages.Select(m => $"[{m.AuthorName}]: {m.Text}")
            );

            await context.YieldOutputAsync(formattedMessages, cancellationToken);
        }
    }
}
