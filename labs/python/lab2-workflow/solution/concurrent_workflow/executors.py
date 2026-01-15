"""
Concurrent Workflow Executors

Contains executor classes for the concurrent workflow pattern.
"""

from dataclasses import dataclass
from typing import Any


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


class ConcurrentStartExecutor(Executor):
    """
    Executor that starts the concurrent processing by broadcasting messages to all connected agents.
    """
    
    def __init__(self):
        super().__init__("ConcurrentStart")
    
    async def handle(self, message: str) -> tuple[str, WorkflowEvent]:
        """Broadcast message to start concurrent processing."""
        print("Broadcasting question to all experts...")
        print()
        event = WorkflowEvent(executor_id=self.name, data=message)
        return message, event


class ConcurrentAggregationExecutor(Executor):
    """
    Executor that aggregates the results from multiple concurrent agents.
    """
    
    def __init__(self):
        super().__init__("ConcurrentAggregation")
        self._messages: list[tuple[str, str]] = []
    
    def add_response(self, agent_name: str, response: str):
        """Add a response from an agent."""
        self._messages.append((agent_name, response))
    
    async def handle(self, responses: dict[str, str]) -> tuple[str, WorkflowEvent]:
        """Aggregate responses from all agents."""
        formatted_messages = "\n\n".join(
            f"[{name}]: {response}"
            for name, response in responses.items()
        )
        event = WorkflowEvent(executor_id=self.name, data=formatted_messages)
        return formatted_messages, event
