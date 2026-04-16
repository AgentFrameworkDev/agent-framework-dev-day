"""
Workflow result handlers for WorkflowBuilder-based workflows.
"""
from agent_framework import AgentResponse, WorkflowRunResult


def handle_workflow_result(result: WorkflowRunResult, verbose: bool = True) -> None:
    """
    Process a WorkflowRunResult and print the outputs.

    Iterates over the workflow outputs and prints agent responses.

    Args:
        result: WorkflowRunResult returned by workflow.run()
        verbose: Whether to print detailed information
    """
    outputs = result.get_outputs()

    if not outputs:
        print("[No output produced by the workflow]")
        return

    for output in outputs:
        if isinstance(output, AgentResponse):
            for message in output.messages:
                if not message.text:
                    continue
                speaker = message.author_name or message.role
                print(f"  {speaker}: {message.text}")
        elif isinstance(output, str):
            print(f"  {output}")
        else:
            print(f"  {output}")
