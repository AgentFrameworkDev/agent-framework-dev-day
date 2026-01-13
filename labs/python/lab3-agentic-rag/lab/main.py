"""
Agentic RAG application for IT support ticket search.

This application uses the Microsoft Agent Framework with a Handoff orchestration
pattern to route user questions to specialized search agents based on query type.
"""
import asyncio
from agent_framework import HandoffBuilder
from agent_framework.azure import AzureOpenAIChatClient

from config import AzureConfig
from services import SearchService
from agents import AgentFactory
from workflows import drain_events, handle_workflow_events

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
        print("✓ Configuration loaded successfully")
    except ValueError as e:
        print(f"✗ Configuration error: {e}")
        return
    
    # Initialize Azure OpenAI chat client
    print("\n[2/5] Initializing Azure OpenAI client...")
    chat_client = AzureOpenAIChatClient(
        deployment_name=config.chat_model, 
        api_key=config.openai_api_key, 
        endpoint=config.openai_endpoiint
    )
    print("✓ Chat client initialized")
    
    # Initialize search service
    print("\n[3/5] Initializing Azure AI Search service...")
    search_service = SearchService(config, chat_client)
    print("✓ Search service initialized")
    
    # Create agents
    print("\n[4/5] Creating agents...")
    agent_factory = AgentFactory(chat_client, search_service)
    agents = agent_factory.create_all_agents()
    print(f"✓ Created {len(agents)} agents: {', '.join(agents.keys())}")
    
    # Build workflow with handoff orchestration
    print("\n[5/5] Building workflow...")
    workflow = (
        HandoffBuilder(
            name="agentic_rag_workflow",
            participants=[agents["classifier"], agents["semantic_search"]],
        )
        .set_coordinator(agents["classifier"])
        .build()
    )
    print("✓ Workflow built successfully")
    
    # Example questions to test
    test_questions = [
        "What problems are there with Surface devices?", #  (Simple question) 
        "Are there any issues for Dell XPS laptops?", # (Yes/No)
        "How many tickets were logged and Incidents for Human Resources and low priority?", #  (Count)
        "Do we have more issues with MacBook Air computers or Dell XPS laptops?", # (Comparative)
        "Which Dell XPS issue does not mention Windows?", # (Difference)
        "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?", # (Intersection)
        "What department had consultants with Login Issues?",  # (Multi-hop)
    ]
    
    print("\n" + "=" * 60)
    print("RUNNING TEST QUERIES")
    print("=" * 60)
    
    # Run first question
    if test_questions:
        first_question = test_questions[0]
        remaining_questions = test_questions[1:]
        
        print(f"\n--- Query 1/{len(test_questions)} ---")
        print(f"User: {first_question}")
        print()
        
        events = await drain_events(workflow.run_stream(first_question))
        pending_requests = handle_workflow_events(events, verbose=False)
        
        # Process remaining questions by responding to pending requests
        for i, question in enumerate(remaining_questions, 2):
            if pending_requests:
                print(f"\n--- Query {i}/{len(test_questions)} ---")
                print(f"User: {question}")
                print()
                
                # Send the next question as a response to pending requests
                responses = {req.request_id: question for req in pending_requests}
                events = await drain_events(workflow.send_responses_streaming(responses))
                pending_requests = handle_workflow_events(events, verbose=False)
            else:
                # No pending requests, workflow conversation ended
                print(f"\n[Note: No pending requests - workflow conversation ended]")
                break
        
        print()
    
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
    
    chat_client = AzureOpenAIChatClient(credential=config.credential)
    search_service = SearchService(config, chat_client)
    agent_factory = AgentFactory(chat_client, search_service)
    agents = agent_factory.create_all_agents()
    
    workflow = (
        HandoffBuilder(
            name="yes_no_rag_workflow",
            participants=[agents["classifier"], agents["semantic_search"]],
        )
        .set_coordinator(agents["classifier"])
        .build()
    )
    
    print("✓ System ready\n")
    
    # Interactive loop - each query starts fresh
    while True:
        try:
            user_input = input("You: ").strip()
            
            if user_input.lower() in ["quit", "exit", "q"]:
                print("\nGoodbye!")
                break
            
            if not user_input:
                continue
            
            # Create a fresh workflow for each query to avoid conversation history
            workflow = (
                HandoffBuilder(
                    name="agentic_rag_workflow",
                    participants=[agents["classifier"], agents["semantic_search"], agents["yes_no"]],
                )
                .set_coordinator(agents["classifier"])
                .build()
            )
            
            print()
            events = await drain_events(workflow.run_stream(user_input))
            handle_workflow_events(events, verbose=False)
            print()
            
        except KeyboardInterrupt:
            print("\n\nGoodbye!")
            break
        except Exception as e:
            print(f"\n✗ Error: {e}\n")


if __name__ == "__main__":
    import sys
    
    # Check for interactive mode flag
    if "--interactive" in sys.argv or "-i" in sys.argv:
        asyncio.run(interactive_mode())
    else:
        asyncio.run(main())
