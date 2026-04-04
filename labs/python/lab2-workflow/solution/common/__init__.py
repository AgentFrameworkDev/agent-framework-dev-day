"""
Common module containing shared components for the workflow lab.
"""

from .support_ticket import SupportTicket, TicketPriority
from .azure_openai_client_factory import create_chat_client
from .ticket_loader import (
    load_all_tickets,
    get_ticket_by_id,
    get_random_ticket,
    get_ticket_by_index,
    display_available_tickets,
)

__all__ = [
    "SupportTicket",
    "TicketPriority",
    "create_chat_client",
    "load_all_tickets",
    "get_ticket_by_id",
    "get_random_ticket",
    "get_ticket_by_index",
    "display_available_tickets",
]
