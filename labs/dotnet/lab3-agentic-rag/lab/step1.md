# Step 1: Get Started

## Learning Objectives
1. Get your development environment configured
2. Get your Azure resources setup
3. Walkthrough the starter code
4. Understand the challenge

## Prerequisites
1. Git
2. Visual Studio Code
3. .NET 10

## Setup your development environment
I assume you have already gone through the notebooks, so your environment should already have the dependencies installed.

## Configure the Azure resources
As long as you are in the workshop on Jan 16 reading this ... your appsettings.Local.json file should contain all the necessary resource settings.

## Overview of the starter code

In order for this workshop to be more challenging than my past RAG workshops, I have provided you with the application code that will perform one of the 7 types of questions. 

### Starter application overview

**[Program.cs](./Program.cs)**: Console app entrypoint that loads config values, spins up the Azure OpenAI chat client, wires up the SearchService, builds agents via [AgentFactory](./Agents/AgentFactory.cs), and assembles the Handoff workflow (classifier + semantic_search) with a small set of demo questions. It can also run in interactive mode.

**[AzureConfig.cs](./config/AzureConfig.cs))**: is a loader/validator for Azure AI Search and Azure OpenAI settings (endpoints, keys, deployments). Uses DefaultAzureCredential and enforces required configuration settings before running.

**[SearchService.cs](./services/SearchService.cs)**: is a thin abstraction over Azure AI Search. Generates embeddings with Azure OpenAI, runs hybrid (keyword + vector) queries, supports optional OData filters, and normalizes ticket fields (id, subject, body, answer, type, department, priority, tags).

**Agents folder**: Factory plus, a classifier and one specialist to start.
- **[AgentFactory.cs](./Agents/AgentFactory.cs)**: Creates all the agents and injects shared dependencies (chat client + search service).

- **[ClassifierAgent.cs](./Agents/ClassifierAgent.cs)**: The routing agent with detailed instructions for detecting question type and handing off to specialists.

- **[SemanticSearchAgent.py](./Agents/SemanticSearchAgent.cs)**: Uses a tool-wrapped semantic search function to fetch tickets (top 10) and build evidence-rich responses.

**[WorkflowHandlers.cs](./Workflows/WorkflowHandlers.cs)**: Helpers to print agent responses.

### Architecture at a Glance

#### Pattern: 
Handoff orchestration — classifier coordinates, specialists handle work; easy to add more agents later (count, yes/no, difference, intersection, multi-hop, comparative).

#### Data flow:
User question → classifier routes → specialist agent tool calls SearchService → results returned to agent → response streamed back via workflow events.

#### Dependencies: 
- Microsoft Agent Framework for agents/workflows
- Azure OpenAI for chat + embeddings
- Azure AI Search for retrieval.

#### Extension points: 
Add new agent modules under Agents, register them in `AgentFactory.CreateAllAgents()`, and include them as participants when building the workflow in Program.cs.

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
Add the useage the create_date field so you can answer these questions:
- What was the last ticket entered for Human Resources? (Ordinal)
- What is the oldest high priority ticket? (Superlative)

## [Go to Step 2: Create the Yes/No Agent >](step2.md)