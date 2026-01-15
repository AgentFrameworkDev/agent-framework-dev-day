# Workflow Lab Exercises (.NET)

Welcome to the Workflow Lab! In this hands-on lab, you'll learn how to build AI-powered workflows using Microsoft.Agents.AI.Workflows:

- Configure Azure OpenAI for AI agents
- Build sequential workflows with linear pipelines
- Build concurrent workflows with fan-out/fan-in patterns
- Build human-in-the-loop workflows with approval gates
- Use executors, edges, and workflow events

## Prerequisites

- .NET 10 SDK installed
- Azure OpenAI resource with deployment
- Visual Studio 2022 or VS Code with C# extension

## Lab Structure

```
lab/
├── EXERCISES.md                               # This file
├── workflow-concepts.ipynb                    # Educational notebook
├── Program.cs                                 # Main entry point (Exercise 5)
├── WorkflowExercises.csproj
├── Common/
│   ├── SupportTicket.cs                       # Shared models
│   └── AzureOpenAIClientFactory.cs            # Exercise 1
├── Sequential/
│   ├── Executors.cs                           # Exercise 2 (Steps 2.1-2.3)
│   └── SequentialWorkflowDemo.cs              # Exercise 2 (Steps 2.4-2.8)
├── Concurrent/
│   ├── Executors.cs                           # Exercise 3 (Steps 3.1-3.2)
│   └── ConcurrentWorkflowDemo.cs              # Exercise 3 (Steps 3.3-3.7)
└── HumanInTheLoop/
    ├── Models.cs                              # Shared models
    ├── Executors.cs                           # Exercise 4 (Steps 4.1-4.3)
    └── HumanInTheLoopWorkflowDemo.cs          # Exercise 4 (Steps 4.4-4.8)
```

---

## Exercise 1: Configure Azure OpenAI Client

**Objective:** Set up the Azure OpenAI client factory to enable AI functionality.

### Step 1.1: Configure the endpoint

Open `Common/AzureOpenAIClientFactory.cs` and find **STEP 1.1**. Uncomment:

```csharp
// var endpoint = GetConfigValue("AzureOpenAI:Endpoint", "AZURE_OPENAI_ENDPOINT")
//     ?? throw new InvalidOperationException(...);
```

**Delete** the placeholder:
```csharp
var endpoint = "https://YOUR-RESOURCE.openai.azure.com/";
```

### Step 1.2: Configure the deployment name

Find **STEP 1.2** and uncomment:

```csharp
// var deploymentName = GetConfigValue("AzureOpenAI:DeploymentName", "AZURE_OPENAI_DEPLOYMENT_NAME") 
//     ?? "gpt-4o-mini";
```

**Delete** the placeholder:
```csharp
var deploymentName = "gpt-4o-mini";
```

### Step 1.3: Enable authentication

Find **STEP 1.3** and uncomment **one** of the authentication options based on your setup:
- **Option 1:** API Key authentication
- **Option 2:** Service Principal authentication  
- **Option 3:** Managed Identity / DefaultAzureCredential

**Delete** the placeholder:
```csharp
throw new NotImplementedException("Exercise 1 not completed...");
```

### Verify

Build the project:
```bash
cd lab/WorkflowLab
dotnet build
```

---

## Exercise 2: Build Sequential Workflow

**Objective:** Create a sequential workflow that processes support tickets through a linear AI pipeline.

### Step 2.1-2.3: Create Executors

Open `Sequential/Executors.cs` and uncomment the following classes:
- **STEP 2.1:** `TicketIntakeExecutor` - Receives and validates tickets
- **STEP 2.2:** `CategorizationBridgeExecutor` - Processes AI categorization output
- **STEP 2.3:** `ResponseBridgeExecutor` - Yields final response

**Delete** the placeholder classes at the bottom of the file.

### Step 2.4-2.8: Build the Workflow

Open `Sequential/SequentialWorkflowDemo.cs` and uncomment:
- **STEP 2.4:** Create Azure OpenAI client
- **STEP 2.5:** Create executors
- **STEP 2.6:** Create AI agents (CategorizationAgent, ResponseAgent)
- **STEP 2.7:** Build workflow using `WorkflowBuilder`
- **STEP 2.8:** Execute the workflow

**Delete** the placeholder message at the bottom.

### Verify

Run the application and select option **1** (Sequential Workflow):
```bash
dotnet run
# Enter choice: 1
```

You should see the ticket processed through categorization and response generation.

---

## Exercise 3: Build Concurrent Workflow

**Objective:** Create a concurrent workflow that fans out to multiple AI agents and combines their responses.

### Step 3.1-3.2: Create Executors

Open `Concurrent/Executors.cs` and uncomment:
- **STEP 3.1:** `ConcurrentStartExecutor` - Broadcasts to all agents
- **STEP 3.2:** `ConcurrentAggregationExecutor` - Aggregates responses

**Delete** the placeholder classes.

### Step 3.3-3.7: Build the Workflow

Open `Concurrent/ConcurrentWorkflowDemo.cs` and uncomment:
- **STEP 3.3:** Create Azure OpenAI client
- **STEP 3.4:** Create specialized AI agents (BillingExpert, TechnicalExpert)
- **STEP 3.5:** Create executors
- **STEP 3.6:** Build workflow with `AddFanOutEdge` and `AddFanInEdge`
- **STEP 3.7:** Execute in streaming mode

**Delete** the placeholder message.

### Verify

Run and select option **2** (Concurrent Workflow). You should see responses from both experts combined.
```bash
dotnet run
# Enter choice: 2
```

---

## Exercise 4: Build Human-in-the-Loop Workflow

**Objective:** Create a workflow that pauses for human approval before finalizing.

### Step 4.1-4.3: Create Executors

Open `HumanInTheLoop/Executors.cs` and uncomment:
- **STEP 4.1:** `HumanInTheLoopTicketIntakeExecutor`
- **STEP 4.2:** `DraftBridgeExecutor`
- **STEP 4.3:** `FinalizeExecutor`

**Delete** the placeholder classes.

### Step 4.4-4.8: Build the Workflow

Open `HumanInTheLoop/HumanInTheLoopWorkflowDemo.cs` and uncomment:
- **STEP 4.4:** Create Azure OpenAI client
- **STEP 4.5:** Build workflow
- **STEP 4.6:** Execute with `RequestInfoEvent` handling
- **STEP 4.7:** `BuildWorkflow` method with `RequestPort`
- **STEP 4.8:** `HandleSupervisorReview` method

**Delete** the placeholder message.

### Verify

Run and select option **3** (Human-in-the-Loop Workflow). You'll be prompted to approve, edit, or escalate the AI draft.
```bash
dotnet run
# Enter choice: 3
```

---

## Exercise 5: Enable All Demos

**Objective:** Enable all workflow demos in the main program.

Open `Program.cs` and uncomment:
- **STEP 5.1:** `SequentialWorkflowDemo.RunAsync()` (after Exercise 2)
- **STEP 5.2:** `ConcurrentWorkflowDemo.RunAsync()` (after Exercise 3)
- **STEP 5.3:** `HumanInTheLoopWorkflowDemo.RunAsync()` (after Exercise 4)

### Verify

Run the application and test all three workflow demos.

---

## Completed Solution

If you get stuck, refer to the complete working solution in `dotnet/lab4-workflow/solution/`.

---

## Summary

Congratulations! You've learned how to:

| Exercise | Concept |
|----------|---------|
| 1 | Configure Azure OpenAI client factory |
| 2 | Build sequential workflows with executors and AI agents |
| 3 | Build concurrent workflows with fan-out/fan-in patterns |
| 4 | Build human-in-the-loop workflows with approval gates |
| 5 | Integrate all workflows in a menu-driven application |

### Key Takeaways

- **Executors** process inputs and send messages to the next workflow step
- **WorkflowBuilder** chains executors and agents with edges
- **AddEdge** creates sequential connections
- **AddFanOutEdge/AddFanInEdge** create concurrent patterns
- **RequestPort** pauses workflows for external input (human review)
- **StreamingRun** enables event-driven execution

---

## Next Steps

- Add more specialized agents to the concurrent workflow
- Implement custom executor logic
- Add persistence for workflow state
- Explore conditional routing based on AI decisions
