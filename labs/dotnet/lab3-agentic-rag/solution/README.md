# Agentic RAG - .NET Solution

This is a C# translation of the Python Agentic RAG application for IT support ticket search.

## Architecture

The application uses the **Microsoft Agent Framework** with a **Handoff orchestration pattern** to create a specialized RAG system.

## Project Structure

```
solution/dotnet/
├── Program.cs                 # Application entry point
├── Lab3Solution.csproj        # Project file
├── appsettings.json           # Configuration file
├── Config/
│   └── AzureConfig.cs        # Azure service configuration
├── Models/
│   └── SupportTicket.cs      # Ticket data model
├── Services/
│   └── SearchService.cs      # Azure AI Search integration
├── Agents/
│   ├── AgentInstructions.cs  # System prompts for all agents
│   ├── AgentFunctions.cs     # AI functions for agent tools
│   └── AgentFactory.cs       # Agent creation and configuration
└── Workflows/
    └── WorkflowHandlers.cs   # Event processing and workflow logic
```

## Agent Types

1. **Classifier Agent** - Routes queries to specialized agents
2. **Yes/No Agent** - Answers binary questions
3. **Semantic Search Agent** - General semantic search
4. **Count Agent** - Counting and filtering queries
5. **Comparative Agent** - Compares multiple items
6. **Difference Agent** - Finds items with exclusions
7. **Intersection Agent** - Finds items matching all criteria
8. **Multi-Hop Agent** - Multi-step reasoning queries

## Configuration

Configure the application using one of these methods:

1. **appsettings.Local.json** (recommended for local development)
2. **User Secrets** (for sensitive data)
3. **Environment Variables**

Required settings:
- `AZURE_SEARCH_ENDPOINT`
- `AZURE_SEARCH_API_KEY`
- `AZURE_SEARCH_INDEX_NAME`
- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_API_VERSION`
- `AZURE_OPENAI_CHAT_DEPLOYMENT_NAME`
- `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`

## Running the Application

### Demo Mode (Default)
```bash
dotnet run
```

### Interactive Mode
```bash
dotnet run --interactive
```

## Key Features

- **Hybrid Search**: Combines keyword and semantic vector search
- **OData Filtering**: Dynamic filter generation for precise queries
- **Multi-Agent Orchestration**: Specialized agents for different query types
- **Structured Responses**: Consistent formatting with citations

## Dependencies

- Azure.AI.Projects
- Azure.Identity
- Azure.Search.Documents
- Microsoft.Agents.AI.AzureAI
- Microsoft.Extensions.Configuration.*

## Notes

This translation maintains the same structure and logic as the Python version while following C# best practices:
- Async/await patterns
- Proper exception handling
- Strong typing with nullable reference types
- Dependency injection ready
- Configuration management with IConfiguration
