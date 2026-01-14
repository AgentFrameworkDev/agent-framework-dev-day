"""
Support Ticket Models

Represents a customer support ticket and related enums.
"""

from dataclasses import dataclass
from enum import Enum


class TicketPriority(Enum):
    """Ticket priority levels."""
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"
    CRITICAL = "Critical"


@dataclass
class SupportTicket:
    """Represents a customer support ticket."""
    ticket_id: str
    customer_id: str
    customer_name: str
    subject: str
    description: str
    priority: TicketPriority = TicketPriority.MEDIUM
