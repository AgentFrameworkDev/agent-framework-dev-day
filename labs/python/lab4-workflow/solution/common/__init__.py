"""
Common module containing shared components for the workflow lab.
"""

from .support_ticket import SupportTicket, TicketPriority
from .azure_openai_client_factory import create_chat_client

__all__ = [
    "SupportTicket",
    "TicketPriority",
    "create_chat_client",
]
