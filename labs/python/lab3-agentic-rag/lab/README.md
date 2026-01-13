# Agentic RAG Hands on Exercise

This exercise assumes you have already ran the code in the notebooks. If you have not yet done so, please work through them first:
- [Ingestion Phrase](./../01-ingestion-phase.ipynb)
- [Simple RAG](./../02-simple-rag.ipynb)
- [Advanced RAG](./../03-advanced-rag.ipynb)
- [Agentic RAG](./../04-agentic-rag.ipynb)

## Goal

As mentioned in the presentation, RAG applications are often easy to get started with but it doesn't take long before you find question types that are not able to be answered. The goal for this exercise is to be able to answer all the question types we mentioned:

![Question Types](./../assets/question-types.png)

## Learning Objectives
- Create an application that uses the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework)
- Utilize the [Handoff](https://learn.microsoft.com/en-us/agent-framework/user-guide/workflows/orchestrations/handoff?pivots=programming-language-python) workflow orchestration
- Write specialized agents that perform specific search capabilities with [Azure AI Search](https://azure.microsoft.com/en-us/products/ai-services/ai-search/)

## Prerequisites

Please install the software ahead of the workshop:
- [VS Code](https://code.visualstudio.com/download)
- [Python 3.13 and PIP](https://www.python.org/downloads/), also recommend installing the [VS Code Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python) - this will allow you to debug and step through code later
- [Git](https://git-scm.com/downloads) and [Github login](https://github.com) - these will make working with the workshop easier on you
- [Azure subscription](https://azure.microsoft.com/en-us/pricing/purchase-options/azure-account/search) - in order to use Azure AI Search you'll need a subscription (unless you are in the workshop on January 16, 2026 - see note below)

// Jason TODO: for after Jan 16, 2026 workshop -> add instructions to load Azure AI Search index with Kaggle data

> NOTE: For those of you in the workshop on January 16, 2026 - I will be providing you with the api keys to use predeployed Azure resources **for the day only**.

## Steps

| Step                                        | Link to step page  |
|---------------------------------------------|--------------------|
| Step 1 - Getting started                    | [Link](./step1.md) |
| Step 2 - Yes/No Search Agent                | [Link](./step2.md) |
| Step 3 - Count Search Agent                 | [Link](./step3.md) |
| Step 4 - Remaining Agents                   | [Link](./step4.md) |

