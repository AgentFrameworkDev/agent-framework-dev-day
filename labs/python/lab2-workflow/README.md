# Lab 2: AI Workflow Patterns - Python

This lab demonstrates three key workflow patterns for building AI applications using Python with Azure OpenAI and the Agent Framework RC4 packages.

## 📓 Interactive Notebook

To explore workflow concepts interactively, open and run the Jupyter notebook:
```bash
cd begin
jupyter notebook workflow-concepts.ipynb
```
Or open `begin/workflow-concepts.ipynb` directly in VS Code.

## 📝 Lab Exercises

For hands-on exercises, see **[begin/EXERCISES.md](begin/EXERCISES.md)**.

## 📁 Project Structure

```
lab2-workflow/
├── README.md
├── begin/                          # Lab exercises (incomplete code)
│   ├── EXERCISES.md              # Step-by-step exercises
│   └── ...                       # Code to complete
└── solution/                     # Complete working solution
    ├── start_all.cmd             # Windows: launch application
    ├── start_all.sh              # macOS/Linux: launch application
    ├── program.py                # Main entry point
    ├── common/                   # Shared components
    │   ├── support_ticket.py     # SupportTicket model
    │   └── azure_openai_client_factory.py  # Azure OpenAI client
    ├── sequential/               # Sequential workflow
    │   ├── executors.py          # Executor classes
    │   └── demo.py               # SequentialWorkflowDemo
    ├── concurrent_workflow/      # Concurrent workflow
    │   ├── executors.py          # Executor classes
    │   └── demo.py               # ConcurrentWorkflowDemo
    └── human_in_the_loop/        # Human-in-the-loop workflow
        ├── models.py             # Review models
        ├── executors.py          # Executor classes
        └── demo.py               # HumanInTheLoopWorkflowDemo
```

## 🎯 Workflow Patterns

### 1. Sequential Workflow
A linear pipeline where each step processes data and passes it to the next:

```
┌─────────────┐    ┌──────────────────┐    ┌───────────────────┐
│   Ticket    │───►│  Categorization  │───►│    Response       │
│   Intake    │    │   AI Agent       │    │    AI Agent       │
└─────────────┘    └──────────────────┘    └───────────────────┘
```

**Use Cases:**
- Document processing pipelines
- Multi-stage content generation
- Step-by-step data transformation

### 2. Concurrent Workflow (Fan-out/Fan-in)
Distributes work to multiple agents simultaneously and aggregates results:

```
                    ┌──────────────────┐
               ┌───►│  Billing Expert  │───┐
┌──────────┐   │    └──────────────────┘   │    ┌─────────────┐
│ Question │───┤                           ├───►│  Combined   │
└──────────┘   │    ┌──────────────────┐   │    │  Response   │
               └───►│ Technical Expert │───┘    └─────────────┘
                    └──────────────────┘
```

**Use Cases:**
- Multi-expert analysis
- Parallel data processing
- Consensus building

### 3. Human-in-the-Loop Workflow
Pauses execution for human input, approval, or oversight:

```
┌─────────────┐    ┌──────────────┐    ┌────────────────┐    ┌──────────────┐
│   Ticket    │───►│   AI Draft   │───►│  Supervisor    │───►│   Finalize   │
│   Intake    │    │    Agent     │    │   Review       │    │   Response   │
└─────────────┘    └──────────────┘    └────────────────┘    └──────────────┘
                                              │
                                              ▼
                                     ┌──────────────┐
                                     │  - Approve   │
                                     │  - Edit      │
                                     │  - Escalate  │
                                     └──────────────┘
```

**Use Cases:**
- Content approval workflows
- Financial authorization
- Quality control checkpoints

## 🚀 Getting Started

### Prerequisites
- Python 3.10 or later
- Azure OpenAI resource with deployed model

### Installation

```bash
# Navigate to the python labs folder
cd labs/python

# Install dependencies (if not already done)
pip install -r requirements.txt

# Navigate to the solution lab
cd lab2-workflow/solution
```

### Configuration

Set the following environment variables:

```bash
# Required
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com"

# Optional (default: gpt-4o-mini)
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o-mini"
```

#### Authentication Options

**Option 1: API Key**
```bash
export AZURE_OPENAI_API_KEY="your-api-key"
```

**Option 2: Azure CLI**
```bash
az login
# No additional environment variables needed
```

**Option 3: Service Principal**
```bash
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
```

**Option 4: Managed Identity**
No additional configuration needed when running in Azure.

## 🏃 Running the Lab

### Quick Start (Windows)

```cmd
cd solution
start_all.cmd
```

### Quick Start (macOS / Linux)

```bash
cd solution
chmod +x start_all.sh   # Make executable (first time only)
./start_all.sh
```

### Manual Start

```bash
cd solution
python program.py
```

### Individual Demos
```bash
cd solution

# Sequential Workflow
python -m sequential.demo

# Concurrent Workflow
python -m concurrent_workflow.demo

# Human-in-the-Loop Workflow
python -m human_in_the_loop.demo
```

## 📚 Core Components

### SupportTicket
```python
from common.support_ticket import SupportTicket, TicketPriority

ticket = SupportTicket(
    ticket_id="TKT-12345",
    customer_id="CUST-12345",
    customer_name="John Smith",
    subject="Account access issue",
    description="Cannot login after password reset",
    priority=TicketPriority.HIGH
)
```

### Azure OpenAI Client Factory
```python
from common.azure_openai_client_factory import create_chat_client

# Automatically uses appropriate authentication method
client = create_chat_client()
```

### Executors
```python
from sequential.executors import TicketIntakeExecutor

executor = TicketIntakeExecutor()
result, event = await executor.handle(ticket)
```

## 🧪 Lab Exercises

### Exercise 1: Add a New Agent
Add a "Sentiment Analysis Agent" to the sequential workflow that analyzes customer sentiment before categorization.

### Exercise 2: Add More Experts
Extend the concurrent workflow with additional specialist agents (e.g., Security Expert, Account Expert).

### Exercise 3: Custom Review Actions
Add new supervisor actions to the human-in-the-loop workflow (e.g., "Request More Info", "Auto-Reply").

### Exercise 4: Workflow Composition
Create a new workflow that combines sequential and concurrent patterns.

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `agent-framework-core` | 1.0.0rc4 | Agent Framework Core |
| `agent-framework-azure-ai` | 1.0.0rc4 | Agent Framework Azure AI |
| `openai` | >=1.50.0 | Azure OpenAI client |
| `azure-identity` | >=1.17.0 | Azure authentication |

## 📖 Additional Resources

- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Python asyncio Documentation](https://docs.python.org/3/library/asyncio.html)
- Review `begin/workflow-concepts.ipynb` for interactive examples

