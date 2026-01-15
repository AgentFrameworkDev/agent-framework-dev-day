# Lab 4 - Workflow Lab

This lab demonstrates three key workflow patterns using **Microsoft.Agents.AI.Workflows** in .NET 10, all using a **Customer Support Ticket System** as the example scenario.

## 🎯 Learning Goals

1. **Sequential Workflow** - Process data through a linear AI pipeline
2. **Concurrent Workflow** - Fan-out to multiple agents simultaneously
3. **Human-in-the-Loop Workflow** - AI assistance with human oversight and approval

## � Interactive Notebook

To explore workflow concepts interactively, open and run the Jupyter notebook:
```bash
cd lab
jupyter notebook workflow-concepts.ipynb
```
Or open `lab/workflow-concepts.ipynb` directly in VS Code.

## �📝 Lab Exercises

For hands-on exercises, see **[lab/EXERCISES.md](lab/EXERCISES.md)**.

## 🏗️ Architecture

### Sequential Workflow
```
???????????????    ????????????????????    ???????????????????    ????????????????
?   Ticket    ??????  Categorization  ??????    Response     ??????    Final     ?
?   Intake    ?    ?   AI Agent       ?    ?    AI Agent     ?    ?   Response   ?
???????????????    ????????????????????    ???????????????????    ????????????????
```

### Concurrent Workflow
```
                         ????????????????????
                    ??????  Billing Expert  ??????
???????????????     ?    ????????????????????    ?    ???????????????????
?   Customer  ???????                            ??????    Combined     ?
?   Question  ?     ?    ????????????????????    ?    ?    Response     ?
???????????????     ?????? Technical Expert ??????    ???????????????????
                         ????????????????????
```

### Human-in-the-Loop Workflow
```
???????????????    ????????????????    ????????????????????    ????????????????
?   Ticket    ??????   AI Draft   ??????  Human Review    ??????   Finalize   ?
?   Intake    ?    ?    Agent     ?    ?  (RequestPort)   ?    ?   Response   ?
???????????????    ????????????????    ????????????????????    ????????????????
                                              ?
                                              ?
                                       ????????????????
                                       ?  Supervisor  ?
                                       ?  - Approve   ?
                                       ?  - Edit      ?
                                       ?  - Escalate  ?
                                       ????????????????
```

## ?? Project Structure

```
WorkflowLab/
??? WorkflowLab.csproj                # Project with dependencies
??? Program.cs                        # Interactive menu to select demos
??? appsettings.json                  # Configuration template
??? appsettings.Development.json      # Local dev settings (gitignored)
?
??? Common/                           # Shared components
?   ??? AzureOpenAIClientFactory.cs  # Multi-auth Azure OpenAI client
?   ??? SupportTicket.cs             # Shared models (SupportTicket, TicketPriority)
?
??? Sequential/                       # Sequential Workflow Demo
?   ??? SequentialWorkflowDemo.cs    # Demo runner
?   ??? Executors.cs                 # TicketIntakeExecutor, CategorizationBridgeExecutor, ResponseBridgeExecutor
?
??? Concurrent/                       # Concurrent Workflow Demo
?   ??? ConcurrentWorkflowDemo.cs    # Demo runner
?   ??? Executors.cs                 # ConcurrentStartExecutor, ConcurrentAggregationExecutor
?
??? HumanInTheLoop/                   # Human-in-the-Loop Demo
    ??? HumanInTheLoopWorkflowDemo.cs # Demo runner
    ??? Models.cs                     # SupervisorReviewRequest, SupervisorDecision, ReviewAction
    ??? Executors.cs                  # HumanInTheLoopTicketIntakeExecutor, DraftBridgeExecutor, FinalizeExecutor
```

## ?? Running the Lab

### Prerequisites

- .NET 10 SDK
- Azure OpenAI resource with a deployed model (e.g., `gpt-4o-mini`)

Configure Azure OpenAI using **one** of the following methods:

#### Option A: Using appsettings.Development.json (Recommended for local dev)

1. Create `appsettings.Development.json` in the WorkflowLab folder:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini",
    "ApiKey": "your-api-key-here"
  }
}
```

> ?? `appsettings.Development.json` should be gitignored to prevent committing secrets.

#### Option B: Using Environment Variables

```powershell
# Required
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"

# Optional (default: gpt-4o-mini)
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "gpt-4o-mini"

# Authentication (choose one):
# Option 1: API Key
$env:AZURE_OPENAI_API_KEY = "your-api-key"

# Option 2: Service Principal
$env:AZURE_TENANT_ID = "your-tenant-id"
$env:AZURE_CLIENT_ID = "your-client-id"
$env:AZURE_CLIENT_SECRET = "your-client-secret"

# Option 3: Managed Identity (no env vars needed, uses DefaultAzureCredential)
```

### Run the Lab

```powershell
cd WorkflowLab
dotnet run
```

### Select a Demo

```
???????????????????????????????????????????????????????????????????????
?                       WORKFLOW LAB                                  ?
?           Microsoft.Agents.AI Workflow Patterns                     ?
???????????????????????????????????????????????????????????????????????

Select a workflow demo to run:

  [1] ?? Sequential Workflow
      Process tickets through a linear AI pipeline
      (Intake ? Categorization ? Response)

  [2] ? Concurrent Workflow
      Fan-out questions to multiple specialist agents
      (Question ? [Billing + Technical Experts] ? Combined)

  [3] ?? Human-in-the-Loop Workflow
      AI-assisted responses with human supervisor review
      (Ticket ? AI Draft ? Human Review ? Final Response)

  [Q] Exit
```

## ?? Configuration Reference

### appsettings.json Schema

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini",
    
    "ApiKey": "your-api-key",
    
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

### Configuration Priority

1. **Environment Variables** (highest priority)
2. **appsettings.Development.json**
3. **appsettings.json** (lowest priority)

### Environment Variable Mapping

| appsettings.json Key | Environment Variable |
|---------------------|---------------------|
| `AzureOpenAI:Endpoint` | `AZURE_OPENAI_ENDPOINT` |
| `AzureOpenAI:DeploymentName` | `AZURE_OPENAI_DEPLOYMENT_NAME` |
| `AzureOpenAI:ApiKey` | `AZURE_OPENAI_API_KEY` |
| `AzureOpenAI:TenantId` | `AZURE_TENANT_ID` |
| `AzureOpenAI:ClientId` | `AZURE_CLIENT_ID` |
| `AzureOpenAI:ClientSecret` | `AZURE_CLIENT_SECRET` |

## ?? Key Concepts

### 1. Executors

Executors are the building blocks of workflows. They process messages and pass data to the next step.

```csharp
// Class-based executor that receives and processes a SupportTicket
internal sealed class TicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
{
    public override async ValueTask HandleAsync(
        SupportTicket ticket, 
        IWorkflowContext context, 
        CancellationToken cancellationToken = default)
    {
        // Validate and format the ticket
        var ticketText = $"Ticket ID: {ticket.TicketId}\nSubject: {ticket.Subject}";
        
        // Send to AI agent as a chat message
        await context.SendMessageAsync(new ChatMessage(ChatRole.User, ticketText), cancellationToken);
        
        // Trigger the AI agent to process
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
    }
}
```

### 2. AI Agents (ChatClientAgent)

Integrate AI models into workflows using `ChatClientAgent`:

```csharp
ChatClientAgent categorizationAgent = new(
    chatClient,
    name: "CategorizationAgent",
    instructions: """
        You are a customer support ticket categorization specialist.
        Categorize tickets into: BILLING, TECHNICAL, GENERAL
        Respond with JSON: {"category": "CATEGORY", "priority": "HIGH|MEDIUM|LOW"}
        """
);
```

### 3. Workflow Builder

Connect executors using edges:

```csharp
// Sequential workflow
var workflow = new WorkflowBuilder(ticketIntake)
    .AddEdge(ticketIntake, categorizationAgent)
    .AddEdge(categorizationAgent, categorizationBridge)
    .AddEdge(categorizationBridge, responseAgent)
    .AddEdge(responseAgent, responseBridge)
    .WithOutputFrom(responseBridge)
    .Build();

// Concurrent workflow with fan-out/fan-in
var workflow = new WorkflowBuilder(startExecutor)
    .AddFanOutEdge(startExecutor, targets: [billingExpert, technicalExpert])
    .AddFanInEdge(sources: [billingExpert, technicalExpert], aggregationExecutor)
    .WithOutputFrom(aggregationExecutor)
    .Build();
```

### 4. RequestPort (Human-in-the-Loop)

Pause workflow execution to wait for external input:

```csharp
// Define request and response types
public sealed record SupervisorReviewRequest(string TicketId, string Category, string Priority, string DraftResponse);
public sealed record SupervisorDecision(ReviewAction Action, string? ModifiedResponse, string? Notes);

// Create the RequestPort
RequestPort reviewPort = RequestPort.Create<SupervisorReviewRequest, SupervisorDecision>("SupervisorReview");

// Handle the request in the event loop
case RequestInfoEvent requestEvt:
    var request = requestEvt.Request.DataAs<SupervisorReviewRequest>();
    var decision = GetSupervisorDecision();  // Get human input
    var response = requestEvt.Request.CreateResponse(decision);
    await run.SendResponseAsync(response);
    break;
```

### 5. Streaming Execution

Execute workflows and process events in real-time:

```csharp
await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, inputData);
await foreach (WorkflowEvent evt in run.WatchStreamAsync())
{
    switch (evt)
    {
        case ExecutorCompletedEvent completed:
            Console.WriteLine($"[{completed.ExecutorId}] completed");
            break;
        case WorkflowOutputEvent output:
            Console.WriteLine($"Output: {output.Data}");
            break;
        case RequestInfoEvent request:
            // Handle human-in-the-loop request
            break;
    }
}
```

## ?? Workflow Pattern Comparison

| Pattern | Use Case | Key Methods | Example |
|---------|----------|-------------|---------|
| **Sequential** | Linear processing pipeline | `AddEdge()` | Ticket ? Categorize ? Respond |
| **Concurrent** | Parallel processing | `AddFanOutEdge()` / `AddFanInEdge()` | Multi-expert analysis |
| **Human-in-the-Loop** | Human oversight/approval | `RequestPort`, `RequestInfoEvent` | Supervisor review |

## ?? Learn More

- See [workflow-concepts.ipynb](workflow-concepts.ipynb) for detailed explanations
- [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

## ?? Lab Exercises

### Exercise 1: Add a New Sequential Step
Add a "Sentiment Analysis" step between Ticket Intake and Categorization.

### Exercise 2: Add a Third Expert
Add a "Policy Expert" agent to the Concurrent workflow.

### Exercise 3: Add Rejection Flow
Modify the Human-in-the-Loop workflow to allow supervisors to reject tickets and request more information from the customer.

## ?? Dependencies

This lab uses the following NuGet packages:

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Agents.AI.Workflows` | 1.0.0-preview | Core workflow framework |
| `Azure.AI.OpenAI` | 2.8.0-beta.1 | Azure OpenAI client |
| `Azure.Identity` | 1.17.1 | Azure authentication |
| `Microsoft.Extensions.AI.OpenAI` | 10.1.1-preview | AI abstractions |
| `Microsoft.Extensions.Configuration.*` | 9.0.6 | Configuration management |
