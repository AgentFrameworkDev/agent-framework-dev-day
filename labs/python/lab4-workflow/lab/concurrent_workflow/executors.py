"""
Concurrent Workflow Executors

============================================================================
EXERCISE 3: Create Concurrent Workflow Executors
============================================================================
These executors handle fan-out (broadcasting to multiple agents) and
fan-in (aggregating results from multiple agents).
============================================================================
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


# ============================================================================
# STEP 3.1: Create ConcurrentStartExecutor
# This executor broadcasts messages to all connected agents.
# Uncomment the entire class below.
# ============================================================================
# class ConcurrentStartExecutor(Executor):
#     """
#     Executor that starts the concurrent processing by broadcasting messages to all connected agents.
#     """
#     
#     def __init__(self):
#         super().__init__("ConcurrentStart")
#     
#     async def handle(self, message: str) -> tuple[str, WorkflowEvent]:
#         """Broadcast message to start concurrent processing."""
#         print("Broadcasting question to all experts...")
#         print()
#         event = WorkflowEvent(executor_id=self.name, data=message)
#         return message, event


# ============================================================================
# STEP 3.2: Create ConcurrentAggregationExecutor
# This executor aggregates results from multiple concurrent agents.
# Uncomment the entire class below.
# ============================================================================
# class ConcurrentAggregationExecutor(Executor):
#     """
#     Executor that aggregates the results from multiple concurrent agents.
#     """
#     
#     def __init__(self):
#         super().__init__("ConcurrentAggregation")
#     
#     async def handle(self, responses: dict[str, str]) -> tuple[str, WorkflowEvent]:
#         """Aggregate responses from all agents."""
#         formatted_messages = "\n\n".join(
#             f"[{name}]: {response}"
#             for name, response in responses.items()
#         )
#         event = WorkflowEvent(executor_id=self.name, data=formatted_messages)
#         return formatted_messages, event


# Placeholder classes - REMOVE after uncommenting above
class ConcurrentStartExecutor(Executor):
    def __init__(self):
        super().__init__("ConcurrentStart")
    
    async def handle(self, message: str) -> tuple[str, WorkflowEvent]:
        raise NotImplementedError("Exercise 3.1 not completed.")


class ConcurrentAggregationExecutor(Executor):
    def __init__(self):
        super().__init__("ConcurrentAggregation")
    
    async def handle(self, responses: dict[str, str]) -> tuple[str, WorkflowEvent]:
        raise NotImplementedError("Exercise 3.2 not completed.")
