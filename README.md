# Agent Framework Dev Day

Agent Framework Dev Day is a full-day, instructor-led series of labs that showcase the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) and adjacent tooling across both .NET and Python ecosystems.

## Agenda

- Part 1 – Overview, first access (Lab 0), observability (OTel), safety, hosted agents
- Part 2 – Workflow patterns and Model Context Protocol (MCP)
- Part 3 – Retrieval-Augmented Generation (RAG) and Agentic RAG

## Prerequisites

### Core workshop setup
- [Git](https://git-scm.com/downloads) and [Github login](https://github.com)
- [Visual Studio Code](https://code.visualstudio.com/download) with the recommended extensions listed in [VSCode-Extensions.md](VSCode-Extensions.md)
- Azure subscription with access to Azure OpenAI and Azure AI Search resources, plus any API keys issued by the instructors
- Stable internet connection for package restores and Azure access

### .NET track prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download) (labs rely on .NET 10 features and templates)
- Latest [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) or [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) plus the .NET runtime debugging workload in VS Code
- Follow the environment notes in [labs/dotnet/README.md](labs/dotnet/README.md) for per-lab debugging tips

### Python track prerequisites
- [Python 3.13 and PIP](https://www.python.org/downloads/) (exact version or newer point releases) with `pip` and `venv`
- Create and activate a virtual environment under `labs/python`, then install shared dependencies via `pip install -r labs/python/requirements.txt`
- Review [labs/python/README.md](labs/python/README.md) for detailed shell-specific setup steps

## Repository layout

- [labs/dotnet](labs/dotnet) – C# solutions covering hello-world onboarding through MCP, workflows, and Agentic RAG. Every lab folder contains both `/begin` (exercise) and `/solution` (reference) subfolders.
- [labs/python](labs/python) – Python equivalents for MCP, workflow, and RAG scenarios, plus notebooks to reinforce concepts.
- [tools](tools) – Utility projects (for example, LabKey encrypters) that support the hands-on exercises.
- [VSCode-Extensions.md](VSCode-Extensions.md) – Canonical list of extensions, themes, and settings the instructors will reference.

## .NET lab index

| Lab | Focus | Supplemental docs |
| --- | ------ | ------------------ |
| lab0-hello-world | Validate SDK install, run first Agent Framework project, explore solution vs. begin folders | [labs/dotnet/lab0-hello-world/README.md](labs/dotnet/lab0-hello-world/README.md) |
| lab1-basic-training | Core agent patterns and incremental exercises (instructions inline in `begin/`; no README yet) | _README not provided_ |
| lab2-mcp | Build local/remote MCP servers, run `mcp-concepts.ipynb`, wire up `McpAgentClient` | [labs/dotnet/lab2-mcp/README.md](labs/dotnet/lab2-mcp/README.md) |
| lab2-workflow | Implement sequential, concurrent, and human-in-the-loop workflows with `Microsoft.Agents.AI.Workflows` | [labs/dotnet/lab2-workflow/README.md](labs/dotnet/lab2-workflow/README.md) |
| lab3-agentic-rag | Agentic RAG scenarios with Azure AI Search and Agent Framework orchestration | [labs/dotnet/lab3-agentic-rag/begin/README.md](labs/dotnet/lab3-agentic-rag/begin/README.md) |

> Each .NET lab also includes notebooks (for example, `mcp-concepts.ipynb`, `workflow-concepts.ipynb`) that can be launched directly from VS Code for guided exploration.

## Python lab index

| Lab | Focus | Supplemental docs |
| --- | ------ | ------------------ |
| lab0-hello-world | Environment smoke test and first Python agent (instructions inside `begin/`) | _README not provided_ |
| lab1-basic-training | Progressive exercises reinforcing Agent Framework primitives (see `begin/` folder) | _README not provided_ |
| lab2-mcp | Python MCP servers/clients plus `mcp-concepts.ipynb` | [labs/python/lab2-mcp/README.md](labs/python/lab2-mcp/README.md) |
| lab2-workflow | Workflow patterns implemented with Python executors and notebooks | [labs/python/lab2-workflow/README.md](labs/python/lab2-workflow/README.md) |
| lab3-agentic-rag | Agentic RAG build-out leveraging Azure AI Search tooling | [labs/python/lab3-agentic-rag/begin/README.md](labs/python/lab3-agentic-rag/begin/README.md) |

## Additional references

- Keep the [VSCode-Extensions.md](VSCode-Extensions.md) list close by when setting up your editor profiles.
- Most labs provide `EXERCISES.md` within the `begin/` folder; switch to the `solution/` folder whenever you need a working reference.
- Interactive notebooks live alongside the labs (for example, `labs/dotnet/lab2-mcp/begin/mcp-concepts.ipynb`). Run them in VS Code or Jupyter to validate your environment before coding.

## Presented by

- [Bill Wilder](https://www.linkedin.com/in/billwilder/)
- [Jason Haley](https://www.linkedin.com/in/jason-a-haley/)
- [Udaiappa Ramachandran](https://www.linkedin.com/in/udair/)

## Sponsored by

| Date | Location | Sponsors |
| --- | --- | --- |
| 16-Jan-2026 | Burlington | Microsoft, Akumina |
