# Workflow Lab Exercises (.NET)

## Overview

This lab demonstrates three key workflow patterns using **Microsoft.Agents.AI.Workflows** in .NET 10, all using a **Customer Support Ticket System** as the example scenario.

## Architecture

### Sequential Workflow
```
Ticket Intake -> Categorization AI Agent -> Response AI Agent -> Final Response
```

### Concurrent Workflow
```
                    -> Billing Expert  ->
Customer Question                         Combined Response
                    -> Technical Expert ->
```

### Human-in-the-Loop Workflow
```
Ticket Intake -> AI Draft Agent -> Human Review (RequestPort) -> Finalize Response
                                        |
                                   Supervisor
                                   - Approve
                                   - Edit
                                   - Escalate
```

## Exercises

### STEP 1: Create Azure OpenAI Chat Client
**File:** `Common/AzureOpenAIClientFactory.cs`

Create an `IChatClient` using Azure OpenAI. Use `AzureOpenAIClient` with `DefaultAzureCredential`, then call `.GetChatClient(deploymentName).AsIChatClient()` to get an `IChatClient`.

### STEP 2: Implement Sequential Workflow
**Files:** `Sequential/Executors.cs`, `Sequential/SequentialWorkflowDemo.cs`

Build a sequential (linear) workflow that processes a support ticket through multiple AI agents:

1. **TicketIntakeExecutor** - Receives a `SupportTicket`, formats it as a `ChatMessage`, and sends it with a `TurnToken` to the next agent.
2. **CategorizationBridgeExecutor** - Receives categorization results (`List<ChatMessage>`), extracts the categorization text, and sends it as a new `ChatMessage` with a `TurnToken` to the response agent.
3. **ResponseBridgeExecutor** - Receives the final response (`List<ChatMessage>`) and yields the output string.
4. **SequentialWorkflowDemo.RunAsync()** - Wire up two `ChatClientAgent` instances (categorization + response) with the executors using `WorkflowBuilder`, then run with `InProcessExecution.RunStreamingAsync()`.

### STEP 3: Implement Concurrent Workflow
**Files:** `Concurrent/Executors.cs`, `Concurrent/ConcurrentWorkflowDemo.cs`

Build a concurrent (fan-out/fan-in) workflow that sends a question to multiple expert agents simultaneously:

1. **ConcurrentStartExecutor** - Receives a `string` message and broadcasts a `ChatMessage` + `TurnToken` to all connected agents.
2. **ConcurrentAggregationExecutor** - Receives responses (`List<ChatMessage>`) from multiple agents, collects them, and yields a combined output when all agents have responded.
3. **ConcurrentWorkflowDemo.RunAsync()** - Create two `ChatClientAgent` instances (BillingExpert + TechnicalExpert), wire them with `AddFanOutEdge()` and `AddFanInBarrierEdge()`, then run with `InProcessExecution.RunStreamingAsync()`.

### STEP 4: Implement Human-in-the-Loop Workflow
**Files:** `HumanInTheLoop/Executors.cs`, `HumanInTheLoop/HumanInTheLoopWorkflowDemo.cs`

Build a workflow with human oversight using `RequestPort` for external input:

1. **HumanInTheLoopTicketIntakeExecutor** - Receives a `SupportTicket`, stores it as a static property, formats it, and sends it to the AI draft agent with a `TurnToken`.
2. **DraftBridgeExecutor** - Receives the AI draft (`List<ChatMessage>`), creates a `SupervisorReviewRequest`, and sends it to the `RequestPort`.
3. **FinalizeExecutor** - Receives the `SupervisorDecision` and yields a final output string based on the action (Approve/Edit/Escalate).
4. **HumanInTheLoopWorkflowDemo.RunAsync()** - Build workflow with `RequestPort.Create<SupervisorReviewRequest, SupervisorDecision>()`, handle `RequestInfoEvent` to prompt the supervisor interactively, and use `handle.SendResponseAsync()` to resume the workflow.

### STEP 5: Wire Up the Main Menu
**File:** `Program.cs`

Create an interactive console menu that lets the user choose which workflow demo to run (Sequential, Concurrent, or Human-in-the-Loop).

## Running the Lab

```bash
dotnet run
```

Select a demo from the interactive menu:
1. Sequential Workflow
2. Concurrent Workflow
3. Human-in-the-Loop Workflow

## Key Concepts

- **Executor** - A processing node that receives typed input and sends messages or yields output
- **WorkflowBuilder** - Fluent API for constructing workflows by adding edges between nodes
- **ChatClientAgent** - An AI agent backed by an `IChatClient` (Azure OpenAI)
- **Fan-Out Edge** - Distributes a message to multiple agents simultaneously
- **Fan-In Barrier Edge** - Waits for all source agents to complete before proceeding
- **RequestPort** - Pauses the workflow to request external input (e.g., human review)
- **TurnToken** - Signals an AI agent to process its accumulated messages
- **StreamingRun** - Async streaming execution that emits `WorkflowEvent` items
