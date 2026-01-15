"""
Human-in-the-Loop Workflow Executors

Contains executor classes for the human-in-the-loop workflow pattern.
"""

from dataclasses import dataclass
from typing import Any, Optional

from common import SupportTicket
from .models import SupervisorReviewRequest, SupervisorDecision, ReviewAction


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


class HumanInTheLoopTicketIntakeExecutor(Executor):
    """
    Executor that handles ticket intake and sends to AI agent.
    """
    
    current_ticket: Optional[SupportTicket] = None
    
    def __init__(self):
        super().__init__("TicketIntake")
    
    async def handle(self, ticket: SupportTicket) -> tuple[str, WorkflowEvent]:
        """Handle incoming ticket and prepare for AI draft."""
        HumanInTheLoopTicketIntakeExecutor.current_ticket = ticket
        
        print("Processing ticket...")
        print()
        
        ticket_text = f"""
Support Ticket Analysis Request:

Ticket ID: {ticket.ticket_id}
Customer: {ticket.customer_name} (ID: {ticket.customer_id})
Priority: {ticket.priority.value}
Subject: {ticket.subject}

Customer Message:
{ticket.description}

Please analyze this ticket and draft an appropriate response.
"""
        
        event = WorkflowEvent(executor_id=self.name, data=ticket_text)
        return ticket_text, event


class DraftBridgeExecutor(Executor):
    """
    Bridge executor that processes AI draft and requests supervisor review.
    """
    
    def __init__(self):
        super().__init__("DraftBridge")
    
    async def handle(self, draft_response: str) -> tuple[SupervisorReviewRequest, WorkflowEvent]:
        """Process draft and prepare review request."""
        ticket = HumanInTheLoopTicketIntakeExecutor.current_ticket
        
        # Display truncated draft
        display_draft = draft_response[:100] + "..." if len(draft_response) > 100 else draft_response
        print(f"AI Draft Generated:")
        print(f"   {display_draft}")
        print()
        
        # Determine category based on ticket content
        subject_lower = ticket.subject.lower()
        if "refund" in subject_lower:
            category = "REFUND"
        elif "technical" in subject_lower:
            category = "TECHNICAL"
        elif "billing" in subject_lower:
            category = "BILLING"
        else:
            category = "GENERAL"
        
        # Create review request
        review_request = SupervisorReviewRequest(
            ticket_id=ticket.ticket_id,
            category=category,
            priority=ticket.priority.value,
            draft_response=draft_response,
        )
        
        event = WorkflowEvent(executor_id=self.name, data=review_request)
        return review_request, event


class FinalizeExecutor(Executor):
    """
    Executor that finalizes the response based on supervisor decision.
    """
    
    def __init__(self):
        super().__init__("Finalize")
    
    async def handle(self, decision: SupervisorDecision) -> tuple[str, WorkflowEvent]:
        """Finalize the workflow based on supervisor decision."""
        ticket = HumanInTheLoopTicketIntakeExecutor.current_ticket
        
        if decision.action == ReviewAction.APPROVE:
            final_message = (
                f"Response approved and sent to customer {ticket.customer_name} "
                f"for ticket {ticket.ticket_id}. Status: RESOLVED. Notes: {decision.notes}"
            )
        elif decision.action == ReviewAction.EDIT:
            final_message = (
                f"Modified response sent to customer {ticket.customer_name} "
                f"for ticket {ticket.ticket_id}. Status: RESOLVED. Notes: {decision.notes}"
            )
        elif decision.action == ReviewAction.ESCALATE:
            final_message = (
                f"Ticket {ticket.ticket_id} has been escalated to management. "
                f"Customer: {ticket.customer_name}. Reason: {decision.notes}. Status: ESCALATED"
            )
        else:
            final_message = "Unknown action taken."
        
        event = WorkflowEvent(executor_id=self.name, data=final_message)
        return final_message, event
