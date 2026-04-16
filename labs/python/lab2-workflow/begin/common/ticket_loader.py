"""
Ticket Loader

Utility for loading tickets from the external JSON data file.
Supports loading all tickets, querying by ID, or selecting a random ticket.
"""

import json
import os
import random
from pathlib import Path
from typing import Optional

from .support_ticket import SupportTicket, TicketPriority


def _get_tickets_path() -> Path:
    """
    Get the tickets path. First checks TICKETS_PATH environment variable.
    If not set, traverses up the directory tree to find the 'assets' folder containing tickets.json.
    
    Returns:
        Path to the tickets JSON file.
        
    Raises:
        ValueError: If tickets.json cannot be found.
    """
    tickets_path = os.environ.get("TICKETS_PATH")
    
    if tickets_path:
        path = Path(tickets_path)
        if not path.is_absolute():
            python_folder = Path(__file__).parent.parent
            path = python_folder / path
        return path.resolve()
    
    # Auto-discover: traverse up from current file to find assets/tickets.json
    current = Path(__file__).resolve().parent
    while current != current.parent:
        candidate = current / "assets" / "tickets.json"
        if candidate.exists():
            return candidate
        current = current.parent
    
    raise ValueError(
        "Could not find tickets.json. "
        "Either set 'TICKETS_PATH' as an environment variable, "
        "or ensure an 'assets' folder containing 'tickets.json' exists in a parent directory."
    )


def _map_priority(priority_str: Optional[str]) -> TicketPriority:
    """Map priority string to TicketPriority enum."""
    if not priority_str:
        return TicketPriority.MEDIUM
    
    priority_upper = priority_str.upper()
    mapping = {
        "LOW": TicketPriority.LOW,
        "MEDIUM": TicketPriority.MEDIUM,
        "HIGH": TicketPriority.HIGH,
        "CRITICAL": TicketPriority.CRITICAL,
    }
    return mapping.get(priority_upper, TicketPriority.MEDIUM)


def _map_to_support_ticket(dto: dict) -> SupportTicket:
    """Map a dictionary to a SupportTicket."""
    return SupportTicket(
        ticket_id=dto.get("id", "UNKNOWN"),
        customer_id=dto.get("customerId", "UNKNOWN"),
        customer_name=dto.get("customerName", "Unknown Customer"),
        subject=dto.get("subject", "No Subject"),
        description=dto.get("description", "No Description"),
        priority=_map_priority(dto.get("priority")),
    )


def load_all_tickets() -> list[SupportTicket]:
    """
    Load all tickets from the JSON file.
    
    Returns:
        List of SupportTicket objects.
        
    Raises:
        ValueError: If TICKETS_PATH is not configured.
        FileNotFoundError: If the tickets file is not found.
    """
    resolved_path = _get_tickets_path()
    
    if not resolved_path.exists():
        raise FileNotFoundError(f"Tickets file not found at: {resolved_path}")
    
    with open(resolved_path, "r", encoding="utf-8") as f:
        ticket_dtos = json.load(f)
    
    return [_map_to_support_ticket(dto) for dto in ticket_dtos]


def get_ticket_by_id(ticket_id: str) -> Optional[SupportTicket]:
    """
    Get a ticket by its ID.
    
    Args:
        ticket_id: The ticket ID to search for.
        
    Returns:
        The matching SupportTicket or None if not found.
    """
    tickets = load_all_tickets()
    for ticket in tickets:
        if ticket.ticket_id.lower() == ticket_id.lower():
            return ticket
    return None


def get_random_ticket() -> SupportTicket:
    """
    Get a random ticket from the collection.
        
    Returns:
        A randomly selected SupportTicket.
        
    Raises:
        ValueError: If no tickets are found.
    """
    tickets = load_all_tickets()
    if not tickets:
        raise ValueError("No tickets found in the data file.")
    
    return random.choice(tickets)


def get_ticket_by_index(index: int) -> SupportTicket:
    """
    Get a ticket by index (1-based for user friendliness).
    
    Args:
        index: 1-based index of the ticket.
        
    Returns:
        The SupportTicket at the given index.
        
    Raises:
        IndexError: If the index is out of range.
    """
    tickets = load_all_tickets()
    if index < 1 or index > len(tickets):
        raise IndexError(f"Index must be between 1 and {len(tickets)}.")
    
    return tickets[index - 1]


def display_available_tickets() -> None:
    """
    Display all available tickets for selection.
    """
    tickets = load_all_tickets()
    print("Available tickets:")
    print("-" * 60)
    for i, t in enumerate(tickets, 1):
        print(f"  [{i}] {t.ticket_id} - {t.subject} ({t.priority.value})")
    print("-" * 60)
