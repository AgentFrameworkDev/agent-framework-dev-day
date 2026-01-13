"""
Workflow event handlers and utilities.
"""
from collections.abc import AsyncIterable
from typing import cast

from agent_framework import (
    ChatMessage,
    HandoffUserInputRequest,
    RequestInfoEvent,
    Role,
    WorkflowEvent,
    WorkflowOutputEvent,
    WorkflowRunState,
    WorkflowStatusEvent,
)


async def drain_events(stream: AsyncIterable[WorkflowEvent]) -> list[WorkflowEvent]:
    """
    Collect all events from an async stream into a list.
    
    This helper drains the workflow's event stream so we can process events
    synchronously after each workflow step completes.
    
    Args:
        stream: Async iterable of WorkflowEvent
        
    Returns:
        List of all events from the stream
    """
    return [event async for event in stream]


def print_agent_responses(request: HandoffUserInputRequest) -> None:
    """
    Display agent responses since the last user message in a handoff request.
    
    The HandoffUserInputRequest contains the full conversation history so far,
    allowing the user to see what's been discussed before providing their next input.
    
    Args:
        request: The user input request containing conversation and prompt
    """
    if not request.conversation:
        return
    
    # Reverse iterate to collect agent responses since last user message
    agent_responses: list[ChatMessage] = []
    for message in request.conversation[::-1]:
        if message.role == Role.USER:
            break
        agent_responses.append(message)
    
    # Print agent responses in original order
    agent_responses.reverse()
    for message in agent_responses:
        speaker = message.author_name or message.role.value
        print(f"  {speaker}: {message.text}")


def handle_workflow_events(events: list[WorkflowEvent], verbose: bool = True) -> list[RequestInfoEvent]:
    """
    Process workflow events and extract any pending user input requests.
    
    This function inspects each event type and:
    - Prints workflow status changes (IDLE, IDLE_WITH_PENDING_REQUESTS, etc.)
    - Displays final conversation snapshots when workflow completes
    - Prints user input request prompts
    - Collects all RequestInfoEvent instances for response handling
    
    Args:
        events: List of WorkflowEvent to process
        verbose: Whether to print detailed status information
        
    Returns:
        List of RequestInfoEvent representing pending user input requests
    """
    requests: list[RequestInfoEvent] = []
    
    for event in events:
        # WorkflowStatusEvent: Indicates workflow state changes
        if isinstance(event, WorkflowStatusEvent):
            if verbose and event.state in {
                WorkflowRunState.IDLE,
                WorkflowRunState.IDLE_WITH_PENDING_REQUESTS,
            }:
                print(f"\n[Workflow Status] {event.state.name}")
        
        # WorkflowOutputEvent: Contains the final conversation when workflow terminates
        elif isinstance(event, WorkflowOutputEvent):
            conversation = cast(list[ChatMessage], event.data)
            if isinstance(conversation, list) and verbose:
                print("\n" + "=" * 60)
                print("FINAL CONVERSATION")
                print("=" * 60)
                for message in conversation:
                    speaker = message.author_name or message.role.value
                    print(f"{speaker}: {message.text}")
                print("=" * 60)
        
        # RequestInfoEvent: Workflow is requesting user input
        elif isinstance(event, RequestInfoEvent):
            if isinstance(event.data, HandoffUserInputRequest):
                print_agent_responses(event.data)
            requests.append(event)
    
    return requests
