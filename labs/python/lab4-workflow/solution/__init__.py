"""
Workflow Lab - Python Implementation

This lab demonstrates three key workflow patterns:

1. Sequential Workflow: Process tickets through a linear pipeline
   - Ticket Intake -> AI Categorization -> AI Response Generation

2. Concurrent Workflow: Fan-out to multiple agents simultaneously
   - Question -> [Billing Expert + Technical Expert] -> Combined Response

3. Human-in-the-Loop Workflow: AI assistance with human oversight
   - Ticket -> AI Draft -> [Human Review/Approval] -> Final Response

All demos use a Customer Support Ticket System as the example scenario.
"""

from .common.support_ticket import SupportTicket, TicketPriority
from .common.azure_openai_client_factory import create_chat_client

__all__ = [
    "SupportTicket",
    "TicketPriority",
    "create_chat_client",
]
