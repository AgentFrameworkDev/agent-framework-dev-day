"""
Human-in-the-Loop Models

Contains data models for the human-in-the-loop workflow.
"""

from dataclasses import dataclass
from enum import Enum
from typing import Optional


class ReviewAction(Enum):
    """Actions a supervisor can take."""
    APPROVE = "Approve"
    EDIT = "Edit"
    ESCALATE = "Escalate"


@dataclass
class SupervisorReviewRequest:
    """Request sent to supervisor for review."""
    ticket_id: str
    category: str
    priority: str
    draft_response: str


@dataclass
class SupervisorDecision:
    """Supervisor's decision after reviewing the draft."""
    action: ReviewAction
    modified_response: Optional[str]
    notes: str
