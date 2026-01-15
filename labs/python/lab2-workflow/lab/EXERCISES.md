# Workflow Lab Exercises (Python)

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

## Lab Structure

```
lab/
├── program.py                                    # Main entry point (Exercise 5)
├── EXERCISES.md                                  # This file
├── workflow-concepts.ipynb                       # Educational notebook
└── workflow_lab/
    ├── __init__.py
    ├── common/
    │   ├── __init__.py
    │   ├── support_ticket.py                     # Shared models
    │   └── azure_openai_client_factory.py        # Exercise 1
    ├── sequential/
    │   ├── __init__.py
    │   ├── executors.py                          # Exercise 2 (Steps 2.1-2.3)
    │   └── demo.py                               # Exercise 2 (Steps 2.4-2.8)
    ├── concurrent/
    │   ├── __init__.py
    │   ├── executors.py                          # Exercise 3 (Steps 3.1-3.2)
    │   └── demo.py                               # Exercise 3 (Steps 3.3-3.7)
    └── human_in_the_loop/
        ├── __init__.py
        ├── models.py                             # Shared models
        ├── executors.py                          # Exercise 4 (Steps 4.1-4.3)
        └── demo.py                               # Exercise 4 (Steps 4.4-4.8)

# Note: requirements.txt is located at labs/python/requirements.txt
```

---

## Setup

Before starting the exercises, set up your Python environment:

```bash
# Navigate to the python labs folder (parent of lab4-workflow)
cd labs/python

# Create virtual environment (if not already created)
python -m venv .venv

# Activate (Windows)
.venv\Scripts\activate

# Activate (Linux/Mac)
source .venv/bin/activate

# Install dependencies (requirements.txt is in labs/python/)
pip install -r requirements.txt

# Navigate to the lab folder
cd lab4-workflow/lab
```

Set environment variables:

**Windows (PowerShell):**
```powershell
$env:AZURE_OPENAI_ENDPOINT = "https://YOUR-RESOURCE.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY = "your-api-key"  # or use Azure CLI auth
```

**Linux/Mac:**
```bash
export AZURE_OPENAI_ENDPOINT="https://YOUR-RESOURCE.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"  # or use Azure CLI auth
```

---

## Exercise 1: Configure Azure OpenAI Client

**Objective:** Set up the Azure OpenAI client factory to enable AI functionality.

### Step 1.1: Configure the endpoint

Open `workflow_lab/common/azure_openai_client_factory.py` and find **STEP 1.1**. Uncomment:

```python
# endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
# if not endpoint:
#     raise ValueError(...)
```

**Delete** the placeholder:
```python
endpoint = "https://YOUR-RESOURCE.openai.azure.com/"
```

### Step 1.2: Configure the deployment name

Find **STEP 1.2** and uncomment:

```python
# deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
```

**Delete** the placeholder:
```python
deployment = "gpt-4o-mini"
```

### Step 1.3: Enable authentication

Find **STEP 1.3** and uncomment **one** of the authentication options based on your setup:
- **Option 1:** API Key authentication
- **Option 2:** Service Principal authentication
- **Option 3:** Azure CLI credential
- **Option 4:** DefaultAzureCredential

**Delete** the placeholder:
```python
raise NotImplementedError("Exercise 1 not completed...")
```

### Verify

Run the program:
```bash
cd lab
python program.py
```

---

## Exercise 2: Build Sequential Workflow

**Objective:** Create a sequential workflow that processes support tickets through a linear AI pipeline.

### Step 2.1-2.3: Create Executors

Open `workflow_lab/sequential/executors.py` and uncomment:
- **STEP 2.1:** `TicketIntakeExecutor` class
- **STEP 2.2:** `CategorizationBridgeExecutor` class
- **STEP 2.3:** `ResponseBridgeExecutor` class

**Delete** the placeholder classes at the bottom of the file.

### Step 2.4-2.8: Build the Workflow

Open `workflow_lab/sequential/demo.py` and uncomment:
- **STEP 2.4:** `CategorizationAgent` class
- **STEP 2.5:** `ResponseAgent` class
- **STEP 2.6:** `SequentialWorkflow` class
- **STEP 2.7:** Workflow setup code in `run_async()`
- **STEP 2.8:** Workflow execution code

**Delete** the placeholder message at the bottom.

### Verify

Run the application and select option **1** (Sequential Workflow):
```bash
python program.py
# Enter choice: 1
```

---

## Exercise 3: Build Concurrent Workflow

**Objective:** Create a concurrent workflow that fans out to multiple AI agents and combines their responses.

### Step 3.1-3.2: Create Executors

Open `workflow_lab/concurrent/executors.py` and uncomment:
- **STEP 3.1:** `ConcurrentStartExecutor` class
- **STEP 3.2:** `ConcurrentAggregationExecutor` class

**Delete** the placeholder classes.

### Step 3.3-3.7: Build the Workflow

Open `workflow_lab/concurrent/demo.py` and uncomment:
- **STEP 3.3:** `ChatClientAgent` class
- **STEP 3.4:** `ConcurrentWorkflow` class
- **STEP 3.5:** AI agents setup (BillingExpert, TechnicalExpert)
- **STEP 3.6:** Executors and workflow creation
- **STEP 3.7:** Workflow execution code

**Delete** the placeholder message.

### Verify

Run and select option **2** (Concurrent Workflow). You should see responses from both experts combined.
```bash
python program.py
# Enter choice: 2
```

---

## Exercise 4: Build Human-in-the-Loop Workflow

**Objective:** Create a workflow that pauses for human approval before finalizing.

### Step 4.1-4.3: Create Executors

Open `workflow_lab/human_in_the_loop/executors.py` and uncomment:
- **STEP 4.1:** `HumanInTheLoopTicketIntakeExecutor` class
- **STEP 4.2:** `DraftBridgeExecutor` class
- **STEP 4.3:** `FinalizeExecutor` class

**Delete** the placeholder classes.

### Step 4.4-4.8: Build the Workflow

Open `workflow_lab/human_in_the_loop/demo.py` and uncomment:
- **STEP 4.4:** `DraftAgent` class
- **STEP 4.5:** `HumanInTheLoopWorkflow` class
- **STEP 4.6:** `handle_supervisor_review` function
- **STEP 4.7:** Workflow setup code in `run_async()`
- **STEP 4.8:** Workflow execution code

**Delete** the placeholder message.

### Verify

Run and select option **3** (Human-in-the-Loop Workflow). You'll be prompted to approve, edit, or escalate the AI draft.
```bash
python program.py
# Enter choice: 3
```

---

## Exercise 5: Enable All Demos

**Objective:** Enable all workflow demos in the main program.

Open `program.py` and uncomment:
- **STEP 5.1:** `await SequentialWorkflowDemo.run_async()` (after Exercise 2)
- **STEP 5.2:** `await ConcurrentWorkflowDemo.run_async()` (after Exercise 3)
- **STEP 5.3:** `await HumanInTheLoopWorkflowDemo.run_async()` (after Exercise 4)

### Verify

Run the application and test all three workflow demos.

---

## Completed Solution

If you get stuck, refer to the complete working solution in `python/lab4-workflow/solution/`.

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

- **Executors** process inputs and return results with workflow events
- **Sequential workflows** chain executors in a linear pipeline
- **Concurrent workflows** use `asyncio.gather` for fan-out/fan-in
- **Human-in-the-loop** workflows pause for external input
- **AI Agents** integrate Azure OpenAI for intelligent processing

---

## Next Steps

- Add more specialized agents to the concurrent workflow
- Implement custom executor logic
- Add persistence for workflow state
- Explore conditional routing based on AI decisions
