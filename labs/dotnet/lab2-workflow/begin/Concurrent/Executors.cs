using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace WorkflowLab.Concurrent;

// ========================================================================
// STEP 3: Implement Concurrent Workflow Executors
// ========================================================================
// TODO: Create two executor classes for the concurrent workflow:
//
// 1. ConcurrentStartExecutor - Receives a string message and broadcasts it
//    to all connected agents (billing + technical experts)
//
// 2. ConcurrentAggregationExecutor - Receives responses from multiple agents
//    (List<ChatMessage>), collects them, and yields a combined output
//
// Hints:
// - ConcurrentStartExecutor sends ChatMessage and TurnToken to all connected agents
// - ConcurrentAggregationExecutor collects messages and waits for 2 responses
// - Use _messages list to accumulate responses from multiple agents
// - Format combined output with agent names: [AuthorName]: Text
//
// [SendsMessage(typeof(ChatMessage))]
// [SendsMessage(typeof(TurnToken))]
// internal sealed class ConcurrentStartExecutor() : Executor<string>("ConcurrentStart")
// {
//     public override async ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         Console.WriteLine("Broadcasting question to all experts...");
//         Console.WriteLine();
//
//         await context.SendMessageAsync(new ChatMessage(ChatRole.User, message), cancellationToken);
//         await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
//     }
// }
//
// [YieldsOutput(typeof(string))]
// internal sealed class ConcurrentAggregationExecutor() : Executor<List<ChatMessage>>("ConcurrentAggregation")
// {
//     private readonly List<ChatMessage> _messages = [];
//
//     public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         _messages.AddRange(messages);
//
//         if (_messages.Count >= 2)
//         {
//             var formattedMessages = string.Join(
//                 Environment.NewLine + Environment.NewLine,
//                 _messages.Select(m => $"[{m.AuthorName}]: {m.Text}")
//             );
//
//             await context.YieldOutputAsync(formattedMessages, cancellationToken);
//         }
//     }
// }
// ========================================================================

// Placeholder classes so the project compiles - replace with your implementation above
[SendsMessage(typeof(ChatMessage))]
[SendsMessage(typeof(TurnToken))]
internal sealed class ConcurrentStartExecutor() : Executor<string>("ConcurrentStart")
{
    public override ValueTask HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 3: Implement ConcurrentStartExecutor");
    }
}

[YieldsOutput(typeof(string))]
internal sealed class ConcurrentAggregationExecutor() : Executor<List<ChatMessage>>("ConcurrentAggregation")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 3: Implement ConcurrentAggregationExecutor");
    }
}
