# Lab 2: AI Workflow Patterns - Python

This lab demonstrates three key workflow patterns for building AI applications using Python with Azure OpenAI.

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
├── README.md                     # This file
├── begin/                        # Lab exercises (incomplete code)
│   ├── EXERCISES.md              # Step-by-step exercises
│   ├── program.py                # Main entry point
│   ├── workflow-concepts.ipynb   # Interactive notebook with concepts
│   ├── common/                   # Shared components
│   │   ├── __init__.py
│   │   ├── support_ticket.py     # SupportTicket model
│   │   └── azure_openai_client_factory.py  # Azure OpenAI client
│   ├── sequential/               # Sequential workflow
│   ├── concurrent_workflow/      # Concurrent workflow
│   └── human_in_the_loop/        # Human-in-the-loop workflow
└── solution/                     # Complete working solution
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
- Python 3.13 or later
- Azure OpenAI resource with deployed model

### Installation

```bash
# Navigate to the python labs folder
cd labs/python

# Install dependencies (if not already done)
pip install -r requirements.txt

# Navigate to the lab
cd lab2-workflow
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

### Main Menu (Interactive)
```bash
cd begin
python program.py
```

## 📚 Core Components

### SupportTicket
```python
from workflow_lab.common import SupportTicket, TicketPriority

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
from workflow_lab.common import create_chat_client

# Automatically uses appropriate authentication method
client = create_chat_client()
```

### Executors
```python
from workflow_lab.sequential.executors import TicketIntakeExecutor

executor = TicketIntakeExecutor()
result, event = await executor.handle(ticket)
```

## 📝 Lab Exercises

For hands-on exercises, see **[begin/EXERCISES.md](begin/EXERCISES.md)**.

## 📖 Additional Resources

- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Python asyncio Documentation](https://docs.python.org/3/library/asyncio.html)
- Review `workflow-concepts.ipynb` for interactive examples

