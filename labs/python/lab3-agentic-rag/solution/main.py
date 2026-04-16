"""
Agentic RAG application for IT support ticket search.

This application uses the Microsoft Agent Framework with a WorkflowBuilder
pattern using structured output, executors, and switch-case routing to
route user questions to specialized search agents based on query type.
"""
import asyncio

from agent_framework import WorkflowBuilder, Case, Default
from agent_framework.openai import OpenAIChatClient

from config import AzureConfig
from services import SearchService
from agents import AgentFactory
from agents.classifier_agent import (
    ClassifiedQuery,
    extract_category,
    QueryBridge,
)
from workflows import handle_workflow_result


def build_workflow(agents):
    """
    Build a WorkflowBuilder workflow with switch-case routing.

    Pipeline:
        classifier_agent -> extract_category -> [switch_case] -> bridge -> specialist_agent
    """
    classifier = agents["classifier"]

    # Create bridge executors (one per specialist) to convert
    # ClassifiedQuery -> str for the downstream agent executors.
    bridges = {
        name: QueryBridge(id=f"{name}_bridge")
        for name in [
            "yes_no", "semantic_search", "count",
            "difference", "intersection", "multi_hop", "comparative",
        ]
    }

    builder = (
        WorkflowBuilder(name="agentic_rag_workflow", start_executor=classifier)
        .add_edge(classifier, extract_category)
        .add_switch_case_edge_group(
            extract_category,
            [
                Case(condition=lambda r: isinstance(r, ClassifiedQuery) and r.category == "yes_no",
                     target=bridges["yes_no"]),
                Case(condition=lambda r: isinstance(r, ClassifiedQuery) and r.category == "count",
                     target=bridges["count"]),
                Case(condition=lambda r: isinstance(r, ClassifiedQuery) and r.category == "difference",
                     target=bridges["difference"]),
                Case(condition=lambda r: isinstance(r, ClassifiedQuery) and r.category == "intersection",
                     target=bridges["intersection"]),
                Case(condition=lambda r: isinstance(r, ClassifiedQuery) and r.category == "multi_hop",
                     target=bridges["multi_hop"]),
                Case(condition=lambda r: isinstance(r, ClassifiedQuery) and r.category == "comparative",
                     target=bridges["comparative"]),
                Default(target=bridges["semantic_search"]),
            ],
        )
    )

    # Wire each bridge to its specialist agent
    for name, bridge in bridges.items():
        builder = builder.add_edge(bridge, agents[name])

    return builder.build()


async def main():
    """Main execution function for the Agentic RAG system."""

    print("=" * 60)
    print("AGENTIC RAG - IT SUPPORT TICKET SEARCH")
    print("=" * 60)

    # Load and validate configuration
    print("\n[1/5] Loading configuration...")
    config = AzureConfig.from_env()
    try:
        config.validate()
        print("Configuration loaded successfully")
    except ValueError as e:
        print(f"Configuration error: {e}")
        return

    # Initialize Azure OpenAI chat client
    print("\n[2/5] Initializing Azure OpenAI client...")
    chat_client = OpenAIChatClient(
        model=config.chat_model,
        credential=config.credential
    )
    print("Chat client initialized")

    # Initialize search service
    print("\n[3/5] Initializing Azure AI Search service...")
    search_service = SearchService(config, chat_client)
    print("Search service initialized")

    # Create agents
    print("\n[4/5] Creating agents...")
    agent_factory = AgentFactory(chat_client, search_service)
    agents = agent_factory.create_all_agents()
    print(f"Created {len(agents)} agents: {', '.join(agents.keys())}")

    # Build workflow with switch-case routing
    print("\n[5/5] Building workflow...")
    workflow = build_workflow(agents)
    print("Workflow built successfully")

    # Example questions to test
    test_questions = [
        "What problems are there with Surface devices?",                                  # Semantic search
        "Are there any issues for Dell XPS laptops?",                                     # Yes/No
        "How many tickets were logged and Incidents for Human Resources and low priority?", # Count
        "Do we have more issues with MacBook Air computers or Dell XPS laptops?",          # Comparative
        "Which Dell XPS issue does not mention Windows?",                                  # Difference
        "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?", # Intersection
        "What department had consultants with Login Issues?",                              # Multi-hop
    ]

    print("\n" + "=" * 60)
    print("RUNNING TEST QUERIES")
    print("=" * 60)

    for i, question in enumerate(test_questions, 1):
        print(f"\n--- Query {i}/{len(test_questions)} ---")
        print(f"User: {question}\n")

        # Each run is independent -- WorkflowBuilder workflows are stateless per run
        result = await workflow.run(question)
        handle_workflow_result(result)

    print("\n" + "=" * 60)
    print("DEMO COMPLETE")
    print("=" * 60)


async def interactive_mode():
    """Run the application in interactive mode for user queries."""

    print("=" * 60)
    print("AGENTIC RAG - INTERACTIVE MODE")
    print("=" * 60)
    print("\nType 'quit' or 'exit' to end the session\n")

    # Initialize system
    config = AzureConfig.from_env()
    config.validate()

    chat_client = OpenAIChatClient(
        model=config.chat_model,
        credential=config.credential,
    )
    search_service = SearchService(config, chat_client)
    agent_factory = AgentFactory(chat_client, search_service)
    agents = agent_factory.create_all_agents()

    workflow = build_workflow(agents)

    print("System ready\n")

    # Interactive loop
    while True:
        try:
            user_input = input("You: ").strip()

            if user_input.lower() in ["quit", "exit", "q"]:
                print("\nGoodbye!")
                break

            if not user_input:
                continue

            print()
            result = await workflow.run(user_input)
            handle_workflow_result(result)
            print()

        except KeyboardInterrupt:
            print("\n\nGoodbye!")
            break
        except Exception as e:
            print(f"\nError: {e}\n")


if __name__ == "__main__":
    import sys

    # Check for interactive mode flag
    if "--interactive" in sys.argv or "-i" in sys.argv:
        asyncio.run(interactive_mode())
    else:
        asyncio.run(main())
