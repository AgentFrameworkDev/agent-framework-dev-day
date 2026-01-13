# Agentic RAG Architecture

## Overview

This implementation uses the **Microsoft Agent Framework** with a **Handoff orchestration pattern** to create a specialized RAG system for IT support ticket search.

## Architecture Diagram

```
User Query
    ↓
Classifier Agent (Coordinator)
    ↓
    ├─→ YES/NO Agent ────→ Azure AI Search ────→ Response
    ├─→ COUNT Agent ──────→ Azure AI Search ────→ Response
    └─→ SEMANTIC Agent ───→ Azure AI Search ────→ Response
```

## Module Structure

### 1. **config.py**
- **Purpose**: Centralized configuration management
- **Key Components**:
  - `AzureConfig`: Dataclass for Azure service settings
  - Environment variable loading with validation
  - DefaultAzureCredential integration

### 2. **search_service.py**
- **Purpose**: Azure AI Search abstraction layer
- **Key Components**:
  - `SearchService`: Handles all search operations
  - `get_embedding()`: Generates embeddings for queries
  - `search_tickets()`: Hybrid search (keyword + vector)
  - `search_tickets_json()`: Returns formatted JSON results

### 3. **agent_functions.py**
- **Purpose**: AI functions for specialized agents
- **Key Components**:
  - `create_yes_no_search_function()`: Factory for yes/no search
  - Uses `@ai_function` decorator for agent tool integration
  - Formats search results for LLM analysis

### 4. **agent_prompts.py**
- **Purpose**: System prompts and instructions
- **Key Components**:
  - `CLASSIFIER_AGENT_INSTRUCTIONS`: Routing logic
  - `YES_NO_AGENT_INSTRUCTIONS`: Response formatting
  - Clear examples for each agent type

### 5. **agents.py**
- **Purpose**: Agent creation and configuration
- **Key Components**:
  - `AgentFactory`: Creates all specialized agents
  - Connects agents to their tools/functions
  - Returns configured ChatAgent instances

### 6. **workflow_handlers.py**
- **Purpose**: Workflow event processing
- **Key Components**:
  - `drain_events()`: Collects async events
  - `print_agent_responses()`: Displays conversation
  - `handle_workflow_events()`: Routes event types

### 7. **main.py**
- **Purpose**: Application entry point
- **Key Components**:
  - `main()`: Demo mode with test queries
  - `interactive_mode()`: Interactive CLI
  - Workflow orchestration setup

## Data Flow: Yes/No Question

1. **User asks**: "Are there any issues for Dell XPS laptops?"

2. **Classifier Agent** analyzes the question:
   - Detects "Are there any" → YES/NO pattern
   - Routes to YES_NO_AGENT

3. **YES/NO Agent** receives handoff:
   - Calls `yes_or_no_search()` AI function
   - Function triggers Azure AI Search

4. **Search Service**:
   - Generates embedding for "Dell XPS laptops issues"
   - Performs hybrid search (keyword + semantic)
   - Returns top 5 relevant tickets

5. **YES/NO Agent** analyzes results:
   - Receives formatted ticket data
   - LLM analyzes evidence
   - Generates structured response:
     - Answer: YES/NO
     - Explanation: Why
     - Examples: Specific tickets

6. **User receives** final response

## Key Design Patterns

### Factory Pattern
- `AgentFactory` creates agents with proper dependencies
- `create_yes_no_search_function()` creates closures over SearchService

### Separation of Concerns
- Configuration isolated in `config.py`
- Search logic in `search_service.py`
- Agent logic in `agents.py`
- Prompts in `agent_prompts.py`

### Hybrid Search Strategy
- **Keyword search**: Matches exact terms ("Dell XPS")
- **Vector search**: Semantic understanding (similar issues)
- **Combined**: Best of both approaches

### Handoff Orchestration
- Single coordinator (Classifier) routes all queries
- Specialists handle specific question types
- Easy to add new specialists without modifying existing code

## Extension Points

### Adding New Agent Types

1. Add prompt to `agent_prompts.py`:
```python
COUNT_AGENT_INSTRUCTIONS = """..."""
```

2. Create AI function in `agent_functions.py`:
```python
def create_count_search_function(search_service):
    @ai_function
    def count_search(query: str) -> str:
        # Implementation
    return count_search
```

3. Add factory method in `agents.py`:
```python
def create_count_agent(self) -> ChatAgent:
    count_fn = create_count_search_function(self.search_service)
    return self.chat_client.create_agent(
        instructions=COUNT_AGENT_INSTRUCTIONS,
        name="count_agent",
        tools=[count_fn]
    )
```

4. Register in workflow (in `main.py`):
```python
agents = agent_factory.create_all_agents()
workflow = HandoffBuilder(
    participants=[agents["classifier"], agents["yes_no"], agents["count"]],
)
```

## Running the Application

### Demo Mode (Default)
```bash
python main.py
```
Runs predefined test queries

### Interactive Mode
```bash
python main.py --interactive
```
Allows real-time user queries

## Dependencies

- `agent-framework`: Microsoft Agent Framework
- `azure-identity`: Azure authentication
- `azure-search-documents`: Azure AI Search SDK
- `python-dotenv`: Environment variable management

## Benefits of This Architecture

1. **Modular**: Each module has single responsibility
2. **Testable**: Easy to unit test each component
3. **Extensible**: Add new agents without modifying core
4. **Maintainable**: Clear separation of concerns
5. **Reusable**: SearchService can be used by all agents
