# Workflow Lab Exercises (Python) - UPDATED

Welcome to the Workflow Lab! In this hands-on lab, you'll learn how to build AI-powered workflows using Python with Azure OpenAI:

- Configure Azure OpenAI for AI agents
- Build sequential workflows with linear pipelines
- Build concurrent workflows with fan-out/fan-in patterns
- Build human-in-the-loop workflows with approval gates
- Use executors and workflow events

## Prerequisites

- Python 3.10+ installed
- Azure OpenAI resource with deployment
- VS Code with Python extension
- Virtual environment activated

## Lab Structure

```
lab2-workflow/begin/
├── program.py                                    # Main entry point (Exercise 5)
├── EXERCISES.md                                  # Original instructions (outdated)
├── EXERCISES(NEW).md                             # This file - Updated instructions
├── workflow-concepts.ipynb                       # Educational notebook
├── foundry_client_factory.py                     # Azure AI Foundry client (optional)
├── common/
│   ├── __init__.py
│   ├── support_ticket.py                         # Shared data models
│   └── azure_openai_client_factory.py            # Exercise 1
├── sequential/
│   ├── __init__.py
│   ├── executors.py                              # Exercise 2 (Steps 2.1-2.3)
│   └── demo.py                                   # Exercise 2 (Steps 2.4-2.8)
├── concurrent_workflow/
│   ├── __init__.py
│   ├── executors.py                              # Exercise 3 (Steps 3.1-3.2)
│   └── demo.py                                   # Exercise 3 (Steps 3.3-3.7)
└── human_in_the_loop/
    ├── __init__.py
    ├── models.py                                 # Shared models
    ├── executors.py                              # Exercise 4 (Steps 4.1-4.3)
    └── demo.py                                   # Exercise 4 (Steps 4.4-4.8)

# Note: requirements.txt is located at labs/python/requirements.txt
# Note: .env file should be at labs/python/.env
```

---

## Setup

Before starting the exercises, set up your Python environment:

```bash
# Navigate to the python labs folder
cd labs/python

# Create virtual environment (if not already created)
python -m venv venv

# Activate (Windows)
venv\Scripts\activate

# Activate (Linux/Mac)
source venv/bin/activate

# Install dependencies (requirements.txt is in labs/python/)
pip install -r requirements.txt

# Navigate to the lab folder
cd lab2-workflow/begin
```

---

## Environment Configuration

### Create .env File

The lab uses the **shared .env file** at `labs/python/.env` in standard dotenv format.

Create or update `labs/python/.env`:

```bash
# Azure OpenAI Configuration
AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini

# Authentication - API Key (recommended)
AZURE_OPENAI_API_KEY=your-api-key-here
```

**Notes:**
- ✅ Comments are allowed (lines starting with `#`)
- ✅ Standard KEY=VALUE format
- ✅ No quotes needed unless value contains spaces
- ✅ `program.py` has built-in support for both JSON and standard format (auto-detects)

---

## Exercise 1: Configure Azure OpenAI Client

**Objective:** Set up the Azure OpenAI client factory to enable AI functionality.

### Current State
The file `common/azure_openai_client_factory.py` has:
- ⚠️ Endpoint reading code **uncommented** (lines 32-37)
- ⚠️ Deployment reading code **uncommented** (line 44)
- ✅ API Key authentication **uncommented** (lines 52-60)
- ❌ Other authentication options **commented out** (lines 62-103)

**If you already uncommented Exercise 1, skip to Exercise 2!**

### Step 1.1: Verify endpoint configuration

Open `common/azure_openai_client_factory.py` and check **STEP 1.1** (around line 32).

Ensure these lines are **uncommented**:
```python
endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
if not endpoint:
    raise ValueError(
        "Azure OpenAI endpoint is not configured. "
        "Set 'AZURE_OPENAI_ENDPOINT' environment variable."
    )
```

### Step 1.2: Verify deployment configuration

Check **STEP 1.2** (around line 44):

Ensure this is **uncommented**:
```python
deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
```

### Step 1.3: Verify authentication

Check **STEP 1.3** (around line 52):

Ensure **API Key authentication** is **uncommented**:
```python
api_key = os.environ.get("AZURE_OPENAI_API_KEY")
if api_key:
    print("Using API Key authentication")
    return AzureOpenAI(
        azure_endpoint=endpoint,
        api_key=api_key,
        api_version="2024-02-15-preview"
    )
```

**Optional:** If not using API key, uncomment one of the other authentication options:
- **Option 2:** Service Principal (lines 62-76)
- **Option 3:** Azure CLI (lines 78-90)
- **Option 4:** DefaultAzureCredential/Managed Identity (lines 92-100)

### Verify

Test the configuration:
```bash
cd lab2-workflow/begin
python program.py
```

Expected output:
```
Loading configuration from: C:\...\labs\python\.env
Loaded 3 environment variables:
--------------------------------------------------
  AZURE_OPENAI_ENDPOINT: https://...
  AZURE_OPENAI_DEPLOYMENT_NAME: gpt-4.1
  AZURE_OPENAI_API_KEY: 2Sb8***jJQJ
--------------------------------------------------
Using API Key authentication
```

---

## Exercise 2: Build Sequential Workflow

**Objective:** Create a sequential workflow that processes support tickets through a linear AI pipeline.

**Architecture:**
```
Customer Question
      ↓
[Ticket Intake] → TicketIntakeExecutor
      ↓
[AI Categorization] → CategorizationBridgeExecutor → CategorizationAgent
      ↓
[AI Response] → ResponseBridgeExecutor → ResponseAgent
      ↓
Final Response
```

### Step 2.1-2.3: Create Executors

Open `sequential/executors.py` and uncomment these three classes:

**STEP 2.1:** `TicketIntakeExecutor` class (starting around line 15)
- Accepts customer questions and creates support tickets
- Returns `TicketCreatedEvent`

**STEP 2.2:** `CategorizationBridgeExecutor` class (around line 45)
- Bridges workflow to AI categorization agent
- Returns `TicketCategorizedEvent`

**STEP 2.3:** `ResponseBridgeExecutor` class (around line 75)
- Bridges workflow to AI response agent
- Returns `ResponseGeneratedEvent`

**Delete** the placeholder classes at the bottom of the file.

### Step 2.4-2.8: Build the Workflow

Open `sequential/demo.py` and uncomment:

**STEP 2.4:** `CategorizationAgent` class (around line 30)
- Uses Azure OpenAI to categorize tickets into Billing/Technical/Account

**STEP 2.5:** `ResponseAgent` class (around line 75)
- Uses Azure OpenAI to generate responses based on category

**STEP 2.6:** `SequentialWorkflow` class (around line 120)
- Chains executors together in sequence

**STEP 2.7:** Workflow setup code in `run_async()` (around line 170)
- Creates agents and executors
- Builds the workflow pipeline

**STEP 2.8:** Workflow execution code (around line 210)
- Runs multiple test tickets through the workflow

**Delete** the placeholder message: `print("Exercise 2 not completed...")`

### Verify

Run the application and select option **1**:
```bash
python program.py
# Enter choice: 1
```

You should see:
- Tickets being created
- AI categorizing each ticket
- AI generating appropriate responses
- Different responses based on category (Billing/Technical/Account)

---

## Exercise 3: Build Concurrent Workflow

**Objective:** Create a concurrent workflow that fans out to multiple AI agents simultaneously and aggregates results.

**Architecture:**
```
Customer Question
      ↓
[Start Executor] → ConcurrentStartExecutor
      ↓
   Fan-Out (asyncio.gather)
      ├── [Billing Expert Agent]
      └── [Technical Expert Agent]
      ↓
   Fan-In (collect responses)
      ↓
[Aggregation Executor] → ConcurrentAggregationExecutor
      ↓
Combined Response
```

### Step 3.1-3.2: Create Executors

Open `concurrent_workflow/executors.py` and uncomment:

**STEP 3.1:** `ConcurrentStartExecutor` class (around line 15)
- Initiates concurrent execution
- Returns `QuestionReceivedEvent`

**STEP 3.2:** `ConcurrentAggregationExecutor` class (around line 45)
- Collects and combines responses from multiple agents
- Returns `ResponsesAggregatedEvent`

**Delete** the placeholder classes.

### Step 3.3-3.7: Build the Workflow

Open `concurrent_workflow/demo.py` and uncomment:

**STEP 3.3:** `ChatClientAgent` class (around line 25)
- Generic AI agent with customizable system prompt
- Used for both specialist agents

**STEP 3.4:** `ConcurrentWorkflow` class (around line 70)
- Manages concurrent execution with `asyncio.gather`
- Collects responses from multiple agents

**STEP 3.5:** AI agents setup (around line 130)
```python
billing_expert = ChatClientAgent(
    name="BillingExpert",
    system_prompt="You are a billing specialist..."
)
technical_expert = ChatClientAgent(
    name="TechnicalExpert",
    system_prompt="You are a technical support specialist..."
)
```

**STEP 3.6:** Executors and workflow creation (around line 155)

**STEP 3.7:** Workflow execution code (around line 180)
- Tests with sample questions

**Delete** the placeholder message.

### Verify

Run and select option **2**:
```bash
python program.py
# Enter choice: 2
```

You should see:
- Both experts responding simultaneously
- Billing expert focusing on payment/charges
- Technical expert focusing on technical issues
- Combined response aggregating both perspectives

---

## Exercise 4: Build Human-in-the-Loop Workflow

**Objective:** Create a workflow that pauses for human approval before finalizing responses.

**Architecture:**
```
Support Ticket
      ↓
[Ticket Intake] → HumanInTheLoopTicketIntakeExecutor
      ↓
[AI Draft] → DraftBridgeExecutor → DraftAgent
      ↓
[Human Review] → Supervisor reviews draft
      ├── Approve → Continue
      ├── Edit → Human modifies response
      └── Escalate → Escalate to manager
      ↓
[Finalize] → FinalizeExecutor
      ↓
Final Response
```

### Step 4.1-4.3: Create Executors

Open `human_in_the_loop/executors.py` and uncomment:

**STEP 4.1:** `HumanInTheLoopTicketIntakeExecutor` class (around line 20)
- Similar to sequential but for HITL workflow
- Returns `TicketCreatedEvent`

**STEP 4.2:** `DraftBridgeExecutor` class (around line 50)
- Bridges workflow to draft generation agent
- Returns `DraftGeneratedEvent`

**STEP 4.3:** `FinalizeExecutor` class (around line 80)
- Handles supervisor decision (approve/edit/escalate)
- Returns `ResponseFinalizedEvent`

**Delete** the placeholder classes.

### Step 4.4-4.8: Build the Workflow

Open `human_in_the_loop/demo.py` and uncomment:

**STEP 4.4:** `DraftAgent` class (around line 30)
- Uses Azure OpenAI to generate draft responses
- Includes reasoning for response

**STEP 4.5:** `HumanInTheLoopWorkflow` class (around line 75)
- Manages workflow with human approval gate
- Pauses execution for human input

**STEP 4.6:** `handle_supervisor_review` function (around line 130)
- Prompts supervisor for approval/edit/escalate
- Returns `SupervisorDecision` object

**STEP 4.7:** Workflow setup code in `run_async()` (around line 180)
- Creates agent and executors
- Builds the workflow

**STEP 4.8:** Workflow execution code (around line 220)
- Runs sample tickets with human review

**Delete** the placeholder message.

### Verify

Run and select option **3**:
```bash
python program.py
# Enter choice: 3
```

You should see:
- AI generates draft response
- Supervisor review prompt appears
- Options: [A]pprove, [E]dit, [S]calate
- Test each option to see different outcomes

---

## Exercise 5: Enable All Demos

**Objective:** Enable all workflow demos in the main program menu.

### Current State
The file `program.py` has all demo calls **commented out** (lines 133, 142, 151).

### Steps

Open `program.py` and uncomment the following lines:

**STEP 5.1:** Enable Sequential Workflow Demo (around line 133)
```python
await SequentialWorkflowDemo.run_async()
```

**Delete the placeholder:**
```python
print("Exercise 2 not completed. Uncomment the SequentialWorkflowDemo.run_async() call.")
```

**STEP 5.2:** Enable Concurrent Workflow Demo (around line 142)
```python
await ConcurrentWorkflowDemo.run_async()
```

**Delete the placeholder:**
```python
print("Exercise 3 not completed. Uncomment the ConcurrentWorkflowDemo.run_async() call.")
```

**STEP 5.3:** Enable Human-in-the-Loop Workflow Demo (around line 151)
```python
await HumanInTheLoopWorkflowDemo.run_async()
```

**Delete the placeholder:**
```python
print("Exercise 4 not completed. Uncomment the HumanInTheLoopWorkflowDemo.run_async() call.")
```

### Verify

Run the application and test all three workflow demos:
```bash
python program.py
```

Menu should show:
```
Select a workflow demo to run:

  [1] Sequential Workflow
  [2] Concurrent Workflow
  [3] Human-in-the-Loop Workflow
  [Q] Exit
```

Test each option to verify all workflows are working.

---

## Completed Solution

If you get stuck, refer to the complete working solution in:
```
labs/python/lab2-workflow/solution/
```

---

## Summary

Congratulations! You've learned how to:

| Exercise | Concept | Status |
|----------|---------|--------|
| 1 | Configure Azure OpenAI client factory | ⚠️ May be already done |
| 2 | Build sequential workflows with executors and AI agents | ❌ Needs uncommenting |
| 3 | Build concurrent workflows with fan-out/fan-in patterns | ❌ Needs uncommenting |
| 4 | Build human-in-the-loop workflows with approval gates | ❌ Needs uncommenting |
| 5 | Integrate all workflows in a menu-driven application | ❌ Needs uncommenting |

### Key Takeaways

- **Executors** are async functions that process inputs and return results with workflow events
- **Sequential workflows** chain executors in a linear pipeline for step-by-step processing
- **Concurrent workflows** use `asyncio.gather()` for fan-out/fan-in parallel execution
- **Human-in-the-loop workflows** pause execution and wait for human input before proceeding
- **AI Agents** integrate Azure OpenAI for intelligent decision-making and response generation
- **Workflow Events** carry data between executors (e.g., `TicketCreatedEvent`, `DraftGeneratedEvent`)
- **Standard dotenv** is used for configuration with automatic fallback to JSON format

### Workflow Patterns Implemented

```
Sequential:
Question → Intake → Categorization → Response → Output

Concurrent (Fan-Out/Fan-In):
Question → Start → [Expert1, Expert2] → Aggregate → Output

Human-in-the-Loop:
Ticket → Intake → Draft → [Human Review] → Finalize → Output
```

---

## Troubleshooting

### .env File Not Found
**Error:** `Warning: .env file not found at ...`

**Solution:** Create `.env` at `labs/python/.env`:
```bash
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini
AZURE_OPENAI_API_KEY=your-api-key
```

### Import Errors
**Error:** `ModuleNotFoundError: No module named 'sequential'`

**Solution:** Make sure you're in the correct directory:
```bash
cd labs/python/lab2-workflow/begin
python program.py
```

### Authentication Errors
**Error:** Azure tenant mismatch or authentication failure

**Solution:** Use API key authentication (recommended):
1. Add `AZURE_OPENAI_API_KEY` to your `.env` file
2. Verify API key auth is uncommented in `azure_openai_client_factory.py` (line 52)

### Workflow Not Running
**Error:** "Exercise X not completed" message appears

**Solution:** 
1. Check that you uncommented the demo function in the respective file
2. Check that you uncommented the demo call in `program.py` (Exercise 5)
3. Verify no syntax errors from uncommenting

---

## Next Steps

- Add more specialized agents to the concurrent workflow (e.g., Security Expert)
- Implement custom executor logic for business rules
- Add persistence to save workflow state to database
- Explore conditional routing based on AI classification decisions
- Add error handling and retry logic to workflows
- Implement workflow telemetry and monitoring

---

## Additional Resources

- Review `workflow-concepts.ipynb` for conceptual understanding
- Check the solution folder for complete working examples
- Explore Azure OpenAI documentation for advanced features
