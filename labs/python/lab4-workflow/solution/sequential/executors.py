"""
Sequential Workflow Executors

Contains executor classes for the sequential workflow pattern.
"""

from dataclasses import dataclass
from typing import Any, Optional


@dataclass
class WorkflowEvent:
    """Event emitted during workflow execution."""
    executor_id: str
    data: Any


class Executor:
    """Base class for workflow executors."""
    
    def __init__(self, name: str):
        self.name = name
    
    async def handle(self, input_data: Any) -> tuple[Any, WorkflowEvent]:
        """Execute the function and return result with event."""
        raise NotImplementedError


class TicketIntakeExecutor(Executor):
    """
    Executor that handles ticket intake and validation.
    Validates the ticket and formats it for the AI categorization agent.
    """
    
    def __init__(self):
        super().__init__("TicketIntake")
    
    async def handle(self, ticket) -> tuple[str, WorkflowEvent]:
        """Handle incoming ticket and prepare for categorization."""
        # Validate the ticket
        if not ticket.subject or not ticket.description:
            raise ValueError("Support ticket must have a subject and description.")
        
        # Format the ticket for the AI categorization agent
        ticket_text = f"""
Ticket ID: {ticket.ticket_id}
Customer ID: {ticket.customer_id}
Customer Name: {ticket.customer_name}
Priority: {ticket.priority.value}
Subject: {ticket.subject}
Description: {ticket.description}
"""
        
        event = WorkflowEvent(executor_id=self.name, data=ticket_text)
        return ticket_text, event


class CategorizationBridgeExecutor(Executor):
    """
    Bridge executor that processes categorization output and prepares for response generation.
    """
    
    def __init__(self):
        super().__init__("CategorizationBridge")
    
    async def handle(self, categorization_result: str) -> tuple[str, WorkflowEvent]:
        """Process categorization and prepare prompt for response agent."""
        print(f"   Categorization: {categorization_result}")
        
        # Prepare prompt for response agent with categorization context
        response_prompt = f"""
Based on the following ticket categorization, generate a customer response:

Categorization Result: {categorization_result}

Please generate an appropriate customer support response.
"""
        
        event = WorkflowEvent(executor_id=self.name, data=response_prompt)
        return response_prompt, event


class ResponseBridgeExecutor(Executor):
    """
    Bridge executor that processes the final response from the AI agent.
    """
    
    def __init__(self):
        super().__init__("ResponseBridge")
    
    async def handle(self, response: str) -> tuple[str, WorkflowEvent]:
        """Process final response and yield output."""
        event = WorkflowEvent(executor_id=self.name, data=response)
        return response, event
