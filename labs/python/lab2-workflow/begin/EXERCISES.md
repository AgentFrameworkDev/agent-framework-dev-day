# Lab 2: Workflow Patterns - Exercises

## Overview

In this lab, you'll build three AI workflow patterns for a **Customer Support Ticket System** using Azure OpenAI:

1. **Exercise 1** - Azure OpenAI Client Factory (shared component)
2. **Exercise 2** - Sequential Workflow (linear pipeline)
3. **Exercise 3** - Concurrent Workflow (fan-out/fan-in)
4. **Exercise 4** - Human-in-the-Loop Workflow (AI + human oversight)
5. **Exercise 5** - Main Program Entry Point

## Prerequisites

- Python 3.10+
- Azure OpenAI resource with a deployed model
- Environment configured (`.env` file in the `python/` folder)

## Project Structure

```
begin/
├── program.py                        # Exercise 5: Main entry point
├── EXERCISES.md                      # This file
├── common/
│   ├── __init__.py
│   ├── support_ticket.py             # Pre-completed: SupportTicket model
│   ├── azure_openai_client_factory.py # Exercise 1: Azure OpenAI client
│   └── ticket_loader.py             # Pre-completed: Load tickets from JSON
├── sequential/
│   ├── __init__.py
│   ├── executors.py                  # Exercise 2: Sequential executors
│   └── demo.py                       # Exercise 2: Sequential demo
├── concurrent_workflow/
│   ├── __init__.py
│   ├── executors.py                  # Exercise 3: Concurrent executors
│   └── demo.py                       # Exercise 3: Concurrent demo
└── human_in_the_loop/
    ├── __init__.py
    ├── models.py                     # Pre-completed: Review models
    ├── executors.py                  # Exercise 4: HITL executors
    └── demo.py                       # Exercise 4: HITL demo
```

## Pre-completed Files

These files are already complete and do not need modification:

- **`common/support_ticket.py`** - `SupportTicket` dataclass and `TicketPriority` enum
- **`common/ticket_loader.py`** - Functions to load tickets from `assets/tickets.json`
- **`human_in_the_loop/models.py`** - `ReviewAction`, `SupervisorReviewRequest`, `SupervisorDecision`

---

## Exercise 1: Azure OpenAI Client Factory

**File:** `common/azure_openai_client_factory.py`

Create a factory that supports multiple authentication methods for Azure OpenAI.

### Step 1.1: Create `_try_api_key_auth`
Look for `STEP 1.1` in the file. Uncomment the function that:
- Reads `AZURE_OPENAI_API_KEY` from environment
- Returns an `AzureOpenAI` client with API key authentication
- Returns `None` if the key is not set

### Step 1.2: Create `_try_service_principal_auth`
Look for `STEP 1.2`. Uncomment the function that:
- Reads `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`
- Uses `ClientSecretCredential` for authentication
- Returns `None` if credentials are not set

### Step 1.3: Create `create_chat_client`
Look for `STEP 1.3`. Uncomment the function that:
- Tries API key → Service Principal → Azure CLI → DefaultAzureCredential
- Returns the first successful client

### Verification
```python
from common import create_chat_client
client = create_chat_client()
print(type(client))  # Should print <class 'openai.lib.azure.AzureOpenAI'>
```

---

## Exercise 2: Sequential Workflow

**Files:** `sequential/executors.py`, `sequential/demo.py`

Build a linear pipeline: **Ticket Intake → AI Categorization → AI Response**

### Step 2.1: Create `TicketIntakeExecutor`
In `executors.py`, look for `STEP 2.1`. Uncomment the class that:
- Formats a `SupportTicket` into text for AI processing
- Emits a `WorkflowEvent` with the formatted text

### Step 2.2: Create `CategorizationBridgeExecutor`
Look for `STEP 2.2`. Uncomment the class that:
- Receives the AI categorization result
- Passes it along to the next stage with context

### Step 2.3: Create `ResponseBridgeExecutor`
Look for `STEP 2.3`. Uncomment the class that:
- Receives the AI response
- Formats the final output

### Step 2.4: Create `CategorizationAgent`
In `demo.py`, look for `STEP 2.4`. Uncomment the AI agent class that:
- Has system instructions for categorizing tickets
- Uses Azure OpenAI to analyze tickets

### Step 2.5: Create `ResponseAgent`
Look for `STEP 2.5`. Uncomment the AI agent class that:
- Has system instructions for drafting responses
- Uses Azure OpenAI to generate responses

### Step 2.6: Create `SequentialWorkflow`
Look for `STEP 2.6`. Uncomment the workflow class that:
- Chains: intake → categorization agent → bridge → response agent → bridge
- Returns a list of `WorkflowEvent`s

### Step 2.7: Set up the workflow
Look for `STEP 2.7`. Uncomment the setup code that:
- Creates the Azure OpenAI client
- Instantiates agents and executors
- Builds the workflow

### Step 2.8: Execute the workflow
Look for `STEP 2.8`. Uncomment the execution code and remove the placeholder.

### Verification
```bash
cd begin
python -m sequential.demo
```
Select a ticket and verify that categorization and response are generated.

---

## Exercise 3: Concurrent Workflow

**Files:** `concurrent_workflow/executors.py`, `concurrent_workflow/demo.py`

Build a fan-out/fan-in pattern: **Question → [Billing + Technical Experts] → Combined Response**

### Step 3.1: Create `ConcurrentStartExecutor`
In `executors.py`, look for `STEP 3.1`. Uncomment the class that:
- Takes a question and prepares it for multiple agents

### Step 3.2: Create `ConcurrentAggregationExecutor`
Look for `STEP 3.2`. Uncomment the class that:
- Receives results from all agents
- Combines them into a formatted summary

### Step 3.3: Create `ChatClientAgent`
In `demo.py`, look for `STEP 3.3`. Uncomment the agent class that:
- Wraps Azure OpenAI with a specific role/instructions
- Processes questions and returns responses

### Step 3.4: Create `ConcurrentWorkflow`
Look for `STEP 3.4`. Uncomment the workflow class that:
- Uses `asyncio.gather` for parallel agent execution
- Aggregates results from all agents

### Step 3.5: Set up specialist agents
Look for `STEP 3.5`. Uncomment the agent creation code:
- Billing Expert and Technical Expert agents with specific instructions

### Step 3.6: Build and execute the workflow
Look for `STEP 3.6`. Uncomment the workflow setup code.

### Step 3.7: Execute the workflow
Look for `STEP 3.7`. Uncomment the execution code and remove the placeholder.

### Verification
```bash
cd begin
python -m concurrent_workflow.demo
```
Select a ticket and verify that both expert responses and a combined summary appear.

---

## Exercise 4: Human-in-the-Loop Workflow

**Files:** `human_in_the_loop/executors.py`, `human_in_the_loop/demo.py`

Build an interactive workflow: **Ticket → AI Draft → Human Review → Final Response**

### Step 4.1: Create `HumanInTheLoopTicketIntakeExecutor`
In `executors.py`, look for `STEP 4.1`. Uncomment the class that:
- Formats a ticket for AI draft generation
- Stores the current ticket as a class variable

### Step 4.2: Create `DraftBridgeExecutor`
Look for `STEP 4.2`. Uncomment the class that:
- Processes the AI draft
- Determines category from ticket subject
- Creates a `SupervisorReviewRequest`

### Step 4.3: Create `FinalizeExecutor`
Look for `STEP 4.3`. Uncomment the class that:
- Handles `SupervisorDecision` (Approve/Edit/Escalate)
- Generates the final status message

### Step 4.4: Create `DraftAgent`
In `demo.py`, look for `STEP 4.4`. Uncomment the AI agent class for drafting responses.

### Step 4.5: Create `HumanInTheLoopWorkflow`
Look for `STEP 4.5`. Uncomment the workflow class that:
- Chains: intake → AI draft → bridge → supervisor review → finalize
- Pauses execution to call `supervisor_handler`

### Step 4.6: Create `handle_supervisor_review`
Look for `STEP 4.6`. Uncomment the console-based review function with options:
- `[1] Approve` - Send response as-is
- `[2] Edit` - Modify the response
- `[3] Escalate` - Escalate to management

### Step 4.7: Set up the workflow
Look for `STEP 4.7`. Uncomment the setup code.

### Step 4.8: Execute the workflow
Look for `STEP 4.8`. Uncomment the execution code and remove the placeholder.

### Verification
```bash
cd begin
python -m human_in_the_loop.demo
```
Select a ticket, review the AI draft, and try each supervisor action (Approve, Edit, Escalate).

---

## Exercise 5: Main Program Entry Point

**File:** `program.py`

Wire up all three demos into a menu-based application.

### Step 5.1: Import the workflow demos
Look for `STEP 5.1`. Uncomment the three import statements.

### Step 5.2: Add configuration validation
Look for `STEP 5.2`. Uncomment the configuration display that shows:
- Azure OpenAI endpoint, deployment, API key status
- Service principal status
- Authentication warnings

### Step 5.3: Wire up demo choices
Look for `STEP 5.3`. Uncomment the if/elif block and remove the placeholder.

### Verification
```bash
cd begin
python program.py
```
The menu should display. Try running each workflow demo (1, 2, 3) and quit with Q.

---

## Troubleshooting

### "AZURE_OPENAI_ENDPOINT is required"
Create a `.env` file in the `python/` folder:
```json
{
  "AZURE_OPENAI_ENDPOINT": "https://your-resource.openai.azure.com",
  "AZURE_OPENAI_API_KEY": "your-key",
  "AZURE_OPENAI_DEPLOYMENT_NAME": "gpt-4o-mini"
}
```

### "Exercise X not completed"
You still have placeholder code. Find the corresponding `STEP X.Y` comments, uncomment the code, and remove the placeholder class/message below it.

### Import errors
Make sure you're running from the `begin/` directory:
```bash
cd labs/python/lab2-workflow/begin
python program.py
```

### "tickets.json not found"
Ensure the `assets/tickets.json` file exists in the `labs/assets/` directory.
