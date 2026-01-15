"""
Concurrent Workflow Demo - Multi-Agent Customer Support

This demonstrates a concurrent workflow that:
1. Takes a customer question as input
2. Sends the question to multiple specialist agents simultaneously
3. Collects and combines responses from all agents

Concepts covered:
- Fan-out (distribute to multiple agents)
- Fan-in (collect from multiple agents)
- AI Agent Integration
- Async parallel processing with asyncio.gather
"""

import asyncio
from openai import AzureOpenAI

from common import create_chat_client
from common.azure_openai_client_factory import get_deployment_name
from .executors import (
    ConcurrentStartExecutor,
    ConcurrentAggregationExecutor,
    WorkflowEvent,
)


class ChatClientAgent:
    """AI Agent with specific expertise."""
    
    def __init__(self, client: AzureOpenAI, deployment: str, name: str, instructions: str):
        self.client = client
        self.deployment = deployment
        self.name = name
        self.instructions = instructions
    
    async def process(self, message: str) -> str:
        """Process the message and return a response."""
        response = self.client.chat.completions.create(
            model=self.deployment,
            messages=[
                {"role": "system", "content": self.instructions},
                {"role": "user", "content": message}
            ],
            max_tokens=200
        )
        return response.choices[0].message.content


class ConcurrentWorkflow:
    """Workflow that executes agents concurrently (fan-out/fan-in pattern)."""
    
    def __init__(
        self,
        start_executor: ConcurrentStartExecutor,
        agents: list[ChatClientAgent],
        aggregation_executor: ConcurrentAggregationExecutor,
    ):
        self.start_executor = start_executor
        self.agents = agents
        self.aggregation_executor = aggregation_executor
    
    async def run(self, question: str) -> list[WorkflowEvent]:
        """Execute the concurrent workflow."""
        events = []
        
        # Step 1: Start executor broadcasts the question
        _, start_event = await self.start_executor.handle(question)
        events.append(start_event)
        
        # Step 2: Fan-out - run all agents concurrently
        async def run_agent(agent: ChatClientAgent) -> tuple[str, str]:
            result = await agent.process(question)
            return agent.name, result
        
        tasks = [run_agent(agent) for agent in self.agents]
        results = await asyncio.gather(*tasks)
        
        # Add individual agent events
        responses = {}
        for name, response in results:
            events.append(WorkflowEvent(name, response))
            responses[name] = response
        
        # Step 3: Fan-in - aggregate results
        combined, agg_event = await self.aggregation_executor.handle(responses)
        events.append(agg_event)
        
        return events


class ConcurrentWorkflowDemo:
    """Demo class for the concurrent workflow."""
    
    @staticmethod
    async def run_async():
        """Run the concurrent workflow demo."""
        print("=== Concurrent Workflow Demo - Multi-Agent Support ===")
        print()
        print("This workflow demonstrates parallel processing with multiple AI agents:")
        print("  Customer Question -> [Billing Expert + Technical Expert] -> Combined Response")
        print()
        
        # Set up the Azure OpenAI client
        client = create_chat_client()
        deployment = get_deployment_name()
        
        # Create specialized AI agents
        billing_expert = ChatClientAgent(
            client=client,
            deployment=deployment,
            name="BillingExpert",
            instructions="""
You are an expert in billing and subscription matters.
Analyze the customer's question from a billing perspective.
If the question is not billing-related, briefly acknowledge and defer to other specialists.
Keep responses concise (2-3 sentences).
"""
        )
        
        technical_expert = ChatClientAgent(
            client=client,
            deployment=deployment,
            name="TechnicalExpert",
            instructions="""
You are an expert in technical support and troubleshooting.
Analyze the customer's question from a technical perspective.
If the question is not technical, briefly acknowledge and defer to other specialists.
Keep responses concise (2-3 sentences).
"""
        )
        
        # Create executors
        start_executor = ConcurrentStartExecutor()
        aggregation_executor = ConcurrentAggregationExecutor()
        
        # Build the workflow
        workflow = ConcurrentWorkflow(
            start_executor=start_executor,
            agents=[billing_expert, technical_expert],
            aggregation_executor=aggregation_executor,
        )
        
        # Sample customer question
        customer_question = (
            "My subscription was charged twice this month and the app keeps "
            "crashing when I try to view my invoice."
        )
        
        print("Customer Question:")
        print(f'   "{customer_question}"')
        print()
        print("Sending question to Billing Expert and Technical Expert simultaneously...")
        print()
        
        # Execute the workflow
        events = await workflow.run(customer_question)
        
        # Display final output
        print("=== Combined Expert Responses ===")
        final_event = events[-1]
        print(final_event.data)
        print()
        print("Concurrent workflow completed!")


async def main():
    """Entry point for running the demo."""
    await ConcurrentWorkflowDemo.run_async()


if __name__ == "__main__":
    asyncio.run(main())
