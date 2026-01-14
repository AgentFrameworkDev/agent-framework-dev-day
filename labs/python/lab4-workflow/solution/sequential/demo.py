"""
Sequential Workflow Demo - Customer Support Ticket System

This demonstrates a sequential AI-powered workflow that processes customer support tickets:
1. Ticket Intake Executor: Receives and validates the incoming support ticket
2. AI Categorization Agent: Analyzes and categorizes the ticket (billing, technical, general)
3. AI Response Agent: Generates an appropriate response based on the category

Concepts covered:
- Executors (function-based and class-based)
- Sequential processing pipeline
- AI Agent Integration with Azure OpenAI
"""

import asyncio
from openai import AzureOpenAI

from ..common import SupportTicket, TicketPriority, create_chat_client
from ..common.azure_openai_client_factory import get_deployment_name
from .executors import (
    TicketIntakeExecutor,
    CategorizationBridgeExecutor,
    ResponseBridgeExecutor,
    WorkflowEvent,
)


class CategorizationAgent:
    """AI Categorization Agent - categorizes the ticket."""
    
    INSTRUCTIONS = """
You are a customer support ticket categorization specialist.
Analyze the incoming support ticket and categorize it into one of these categories:
- BILLING: Payment issues, invoices, subscription, refunds
- TECHNICAL: Software bugs, errors, performance issues, how-to questions
- GENERAL: Account inquiries, feedback, general questions

Respond with a JSON object in this exact format:
{"category": "CATEGORY_NAME", "priority": "HIGH|MEDIUM|LOW", "summary": "brief summary"}

Keep your response concise and only output the JSON.
"""
    
    def __init__(self, client: AzureOpenAI, deployment: str):
        self.client = client
        self.deployment = deployment
        self.name = "CategorizationAgent"
    
    async def process(self, ticket_text: str) -> str:
        """Categorize the ticket using AI."""
        response = self.client.chat.completions.create(
            model=self.deployment,
            messages=[
                {"role": "system", "content": self.INSTRUCTIONS},
                {"role": "user", "content": ticket_text}
            ]
        )
        return response.choices[0].message.content


class ResponseAgent:
    """AI Response Agent - generates the customer response."""
    
    INSTRUCTIONS = """
You are a friendly and professional customer support representative.
Based on the ticket category and details provided, generate a helpful response to the customer.

Guidelines:
- Be empathetic and professional
- Acknowledge the customer's issue
- Provide relevant next steps or solutions
- Keep the response concise (3-4 sentences)
- Include a reference ticket number format: TKT-XXXXX
"""
    
    def __init__(self, client: AzureOpenAI, deployment: str):
        self.client = client
        self.deployment = deployment
        self.name = "ResponseAgent"
    
    async def process(self, prompt: str) -> str:
        """Generate customer response using AI."""
        response = self.client.chat.completions.create(
            model=self.deployment,
            messages=[
                {"role": "system", "content": self.INSTRUCTIONS},
                {"role": "user", "content": prompt}
            ]
        )
        return response.choices[0].message.content


class SequentialWorkflow:
    """Sequential workflow that chains executors together."""
    
    def __init__(
        self,
        ticket_intake: TicketIntakeExecutor,
        categorization_agent: CategorizationAgent,
        categorization_bridge: CategorizationBridgeExecutor,
        response_agent: ResponseAgent,
        response_bridge: ResponseBridgeExecutor,
    ):
        self.ticket_intake = ticket_intake
        self.categorization_agent = categorization_agent
        self.categorization_bridge = categorization_bridge
        self.response_agent = response_agent
        self.response_bridge = response_bridge
    
    async def run(self, ticket: SupportTicket) -> list[WorkflowEvent]:
        """Execute the sequential workflow."""
        events = []
        
        # Step 1: Ticket Intake
        ticket_text, event = await self.ticket_intake.handle(ticket)
        events.append(event)
        
        # Step 2: AI Categorization
        categorization = await self.categorization_agent.process(ticket_text)
        events.append(WorkflowEvent(self.categorization_agent.name, categorization))
        
        # Step 3: Categorization Bridge
        response_prompt, event = await self.categorization_bridge.handle(categorization)
        events.append(event)
        
        # Step 4: AI Response Generation
        response = await self.response_agent.process(response_prompt)
        events.append(WorkflowEvent(self.response_agent.name, response))
        
        # Step 5: Response Bridge (final output)
        final_response, event = await self.response_bridge.handle(response)
        events.append(event)
        
        return events


class SequentialWorkflowDemo:
    """Demo class for the sequential workflow."""
    
    @staticmethod
    async def run_async():
        """Run the sequential workflow demo."""
        print("=== Sequential Workflow Demo - Customer Support Ticket System ===")
        print()
        print("This workflow demonstrates sequential processing of support tickets:")
        print("  1. Ticket Intake -> 2. AI Categorization -> 3. AI Response Generation")
        print()
        
        # Set up the Azure OpenAI client
        client = create_chat_client()
        deployment = get_deployment_name()
        
        # Create executors
        ticket_intake = TicketIntakeExecutor()
        categorization_bridge = CategorizationBridgeExecutor()
        response_bridge = ResponseBridgeExecutor()
        
        # Create AI agents
        categorization_agent = CategorizationAgent(client, deployment)
        response_agent = ResponseAgent(client, deployment)
        
        # Build the sequential workflow
        workflow = SequentialWorkflow(
            ticket_intake=ticket_intake,
            categorization_agent=categorization_agent,
            categorization_bridge=categorization_bridge,
            response_agent=response_agent,
            response_bridge=response_bridge,
        )
        
        # Sample customer support ticket
        sample_ticket = SupportTicket(
            ticket_id="TKT-12345",
            customer_id="CUST-12345",
            customer_name="John Smith",
            subject="Unable to access my account after password reset",
            description=(
                "I tried to reset my password yesterday but now I cannot log in. "
                "I've tried multiple times and keep getting an 'invalid credentials' error. "
                "This is urgent as I need to access my billing information."
            ),
            priority=TicketPriority.HIGH,
        )
        
        print("Incoming Support Ticket:")
        print(f"   Ticket ID: {sample_ticket.ticket_id}")
        print(f"   Customer: {sample_ticket.customer_name} ({sample_ticket.customer_id})")
        print(f"   Priority: {sample_ticket.priority.value}")
        print(f"   Subject: {sample_ticket.subject}")
        print(f"   Description: {sample_ticket.description}")
        print()
        print("Processing through sequential workflow...")
        print()
        
        # Execute the workflow
        events = await workflow.run(sample_ticket)
        
        # Display final output
        print()
        print("=== Workflow Output ===")
        print()
        final_event = events[-1]
        print(f"Final Response:\n{final_event.data}")
        print()
        print("Sequential workflow completed!")


async def main():
    """Entry point for running the demo."""
    await SequentialWorkflowDemo.run_async()


if __name__ == "__main__":
    asyncio.run(main())
