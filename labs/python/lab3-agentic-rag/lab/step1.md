# Step 1: Get Started

## Learning Objectives
1. Get your development environment configured
2. Get your Azure resources setup
3. Walkthrough the starter code
4. Understand the challenge

## Prerequisites
1. Git
2. Python 3.13 or later
3. Visual Studio Code with the Python extension installed
4. PIP

## Setup your development environment
I assume you have already gone through the notebooks, so your virtual environment should already have the dependencies installed.

// Jason TODO: for after Jan 16, 2026 workshop -> specify needed settings

However, we need to make sure you are setup to use the terminal with the virtual enviornment.

1. Open a new Terminal
2. Activate the virtual environment by running the following command:

*On Windows*
```
.\.venv\Scripts\activate
```

*On MAC or Linux*
```
source .venv/bin/activate
```

## Configure the Azure resources
I assume you have already gone through the notebooks, so your .env file should contain all the necessary resource settings.

// Jason TODO: for after Jan 16, 2026 workshop -> specify needed settings

## Overview of the starter code

In order for this workshop to be more challenging than my past RAG workshops, I have provided you with the application code that will perform one of the 7 types of qustions. 

### Starter application overview

**[main.py](main.py)**: CLI entrypoint that loads config values, spins up the Azure OpenAI chat client, wires up the SearchService, builds agents via [AgentFactory](./agents/agent_factory.py), and assembles the Handoff workflow (classifier as coordinator; classifier + semantic_search participants) with a small set of demo questions plus an interactive mode.

**[azure_config.py](./config/azure_config.py))**: is a Dataclass loader/validator for Azure AI Search and Azure OpenAI settings (endpoints, keys, deployments). Uses DefaultAzureCredential and enforces required env vars before running.

**[search_service.py](./services/search_service.py)**: is a thin abstraction over Azure AI Search. Generates embeddings with Azure OpenAI, runs hybrid (keyword + vector) queries, supports optional OData filters, and normalizes ticket fields (id, subject, body, answer, type, department, priority, tags).

**agents folder**: Factory plus, a classifier and one specialist to start.
- **[agent_factory.py](./agents/agent_factory.py)**: Creates all the agents and injects shared dependencies (chat client + search service).

- **[classifier_agent.py](./agents/classifier_agent.py)**: The routing agent with detailed instructions for detecting question type and handing off to specialists.

- **[semantic_search_agent.py](./agents/semantic_search_agent.py)**: Uses a tool-wrapped semantic search function to fetch tickets (top 10) and build evidence-rich responses.

**[workflow_handlers.py](./workflows/workflow_handlers.py)**: Helpers to drain async workflow event streams, print agent responses, and surface pending user input requests during handoffs.

### Architecture at a Glance

#### Pattern: 
Handoff orchestration — classifier coordinates, specialists handle work; easy to add more agents later (count, yes/no, difference, intersection, multi-hop, comparative).

#### Data flow:
User question → classifier routes → specialist agent tool calls SearchService → results returned to agent → response streamed back via workflow events.

#### Dependencies: 
- Microsoft Agent Framework for agents/workflows
-Azure OpenAI for chat + embeddings
- Azure AI Search for retrieval.

#### Extension points: 
Add new agent modules under agents, register them in `agent_factory.create_all_agents()`, and include them as participants when building the workflow in main.py.

## The Challenge

Using the Azure AI Search index that is preloaded with the [Customer IT Support - Ticket Dataset](https://www.kaggle.com/datasets/tobiasbueck/multilingual-customer-support-tickets) *(english items only)*, create an Agentic RAG application using [Microsoft Agent Framework](https://github.com/microsoft/agent-framework) that can correctly answer these questions:
- What issues do we have with dell xps laptops? (Simple question) -> **DONE**
- Are there any issues logged for Dell XPS laptops? (Yes/No)
- How many tickets were logged and Incidents for Human Resources and low priority? (Count)
- Do we have more issues with MacBook Air computers or Dell XPS laptops? (Comparative)
- What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B? (Intersection)
- Which Dell XPS issue does not mention Windows? (Difference)
- What department had consultants with Login Issues? (Multi-hop)

I'll provide the stpes to create and add the agents to answer the Yes/No and Count question types, and then describe the ideas for you to create the others on your own.

### Bonus Challenge
Add a date field and populate it with sample data so you can answer these questions
- What was the last ticket entered for Human Resources? (Ordinal)
- What is the oldest high priority ticket? (Superlative)

## [Go to Step 2: Create the Yes/No Agent >](step2.md)