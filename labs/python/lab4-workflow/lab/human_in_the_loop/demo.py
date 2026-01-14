"""
Human-in-the-Loop Workflow Demo - Customer Support Ticket Review System

============================================================================
EXERCISE 4: Human-in-the-Loop Workflow Demo
============================================================================
This demonstrates an interactive workflow for customer support that:
1. Receives a customer support ticket
2. AI agent analyzes and drafts a response
3. Pauses to request human supervisor review/approval
4. Allows supervisor to approve, edit, or escalate the ticket
5. Finalizes and sends the response or escalates to management
============================================================================
"""

import asyncio
from typing import Callable
from openai import AzureOpenAI

from ..common import SupportTicket, TicketPriority, create_chat_client
from ..common.azure_openai_client_factory import get_deployment_name
from .models import SupervisorReviewRequest, SupervisorDecision, ReviewAction
from .executors import (
    HumanInTheLoopTicketIntakeExecutor,
    DraftBridgeExecutor,
    FinalizeExecutor,
    WorkflowEvent,
)


# ============================================================================
# STEP 4.4: Create DraftAgent
# AI agent for drafting responses.
# Uncomment the entire class below.
# ============================================================================
# class DraftAgent:
#     """AI agent for drafting responses."""
#     
#     INSTRUCTIONS = """
# You are an experienced customer support specialist. Your job is to:
# 1. Analyze the support ticket
# 2. Categorize it (BILLING, TECHNICAL, REFUND, GENERAL)
# 3. Draft a professional, empathetic response
# 
# For refund requests:
# - Acknowledge the customer's frustration
# - Explain the refund policy (14-day money-back guarantee)
# - Offer alternatives if applicable (troubleshooting, downgrade)
# - Be professional but empathetic
# 
# Keep your response to 3-5 sentences. Be concise but helpful.
# """
#     
#     def __init__(self, client: AzureOpenAI, deployment: str):
#         self.client = client
#         self.deployment = deployment
#         self.name = "DraftAgent"
#     
#     async def process(self, ticket_text: str) -> str:
#         """Generate a draft response for the ticket."""
#         response = self.client.chat.completions.create(
#             model=self.deployment,
#             messages=[
#                 {"role": "system", "content": self.INSTRUCTIONS},
#                 {"role": "user", "content": ticket_text}
#             ]
#         )
#         return response.choices[0].message.content


# ============================================================================
# STEP 4.5: Create HumanInTheLoopWorkflow
# Workflow that pauses for human review/approval.
# Uncomment the entire class below.
# ============================================================================
# class HumanInTheLoopWorkflow:
#     """Workflow that pauses for human review/approval."""
#     
#     def __init__(
#         self,
#         ticket_intake: HumanInTheLoopTicketIntakeExecutor,
#         draft_agent,  # DraftAgent
#         draft_bridge: DraftBridgeExecutor,
#         finalize: FinalizeExecutor,
#     ):
#         self.ticket_intake = ticket_intake
#         self.draft_agent = draft_agent
#         self.draft_bridge = draft_bridge
#         self.finalize = finalize
#     
#     async def run(
#         self,
#         ticket: SupportTicket,
#         supervisor_handler: Callable[[SupervisorReviewRequest], SupervisorDecision],
#     ) -> list[WorkflowEvent]:
#         """Execute the workflow with human review."""
#         events = []
#         
#         # Step 1: Ticket Intake
#         ticket_text, event = await self.ticket_intake.handle(ticket)
#         events.append(event)
#         
#         # Step 2: AI Draft Generation
#         draft_response = await self.draft_agent.process(ticket_text)
#         events.append(WorkflowEvent(self.draft_agent.name, draft_response))
#         
#         # Step 3: Create review request
#         review_request, event = await self.draft_bridge.handle(draft_response)
#         events.append(event)
#         
#         # Step 4: PAUSE - Get human supervisor decision
#         decision = supervisor_handler(review_request)
#         events.append(WorkflowEvent("SupervisorReview", decision))
#         
#         # Step 5: Finalize based on decision
#         final_message, event = await self.finalize.handle(decision)
#         events.append(event)
#         
#         return events


# ============================================================================
# STEP 4.6: Create handle_supervisor_review function
# Console-based supervisor review handler.
# Uncomment the entire function below.
# ============================================================================
# def handle_supervisor_review(review_request: SupervisorReviewRequest) -> SupervisorDecision:
#     """Console-based supervisor review handler."""
#     print()
#     print("=====================================================================")
#     print("            SUPERVISOR REVIEW REQUIRED                               ")
#     print("=====================================================================")
#     print()
#     print(f"Ticket: {review_request.ticket_id}")
#     print(f"Category: {review_request.category}")
#     print(f"Priority: {review_request.priority}")
#     print()
#     print("AI-Generated Draft Response:")
#     print("---------------------------------------------------------------------")
#     print(review_request.draft_response)
#     print("---------------------------------------------------------------------")
#     print()
#     print("Actions:")
#     print("  [1] Approve - Send this response to the customer")
#     print("  [2] Edit - Modify the response before sending")
#     print("  [3] Escalate - Escalate to management for review")
#     print()
#     
#     while True:
#         choice = input("Enter your choice (1-3): ").strip()
#         
#         if choice == "1":
#             print()
#             print("Response approved. Sending to customer...")
#             return SupervisorDecision(
#                 action=ReviewAction.APPROVE,
#                 modified_response=None,
#                 notes="Approved as-is",
#             )
#         
#         elif choice == "2":
#             print()
#             print("Enter your modified response (press Enter twice to finish):")
#             lines = []
#             while True:
#                 line = input()
#                 if line == "":
#                     break
#                 lines.append(line)
#             modified_response = "\n".join(lines)
#             print("Modified response saved. Sending to customer...")
#             return SupervisorDecision(
#                 action=ReviewAction.EDIT,
#                 modified_response=modified_response,
#                 notes="Edited by supervisor",
#             )
#         
#         elif choice == "3":
#             print()
#             reason = input("Enter escalation reason: ")
#             print("Ticket escalated to management.")
#             return SupervisorDecision(
#                 action=ReviewAction.ESCALATE,
#                 modified_response=None,
#                 notes=reason or "Escalated by supervisor",
#             )
#         
#         else:
#             print("Invalid choice. Please enter 1, 2, or 3.")


class HumanInTheLoopWorkflowDemo:
    """Demo class for the human-in-the-loop workflow."""
    
    @staticmethod
    async def run_async():
        """Run the human-in-the-loop workflow demo."""
        print("=== Human-in-the-Loop Workflow Demo ===")
        print("=== Customer Support Ticket Review System ===")
        print()
        print("This workflow demonstrates AI-assisted ticket handling with human oversight:")
        print("  Ticket -> AI Draft -> [Human Review] -> Final Response")
        print()
        print("A supervisor reviews AI-generated responses before they are sent to customers.")
        print()
        
        # ============================================================================
        # STEP 4.7: Set up the workflow
        # Uncomment the lines below
        # ============================================================================
        # # Set up the Azure OpenAI client
        # client = create_chat_client()
        # deployment = get_deployment_name()
        # 
        # # Create executors
        # ticket_intake = HumanInTheLoopTicketIntakeExecutor()
        # draft_bridge = DraftBridgeExecutor()
        # finalize = FinalizeExecutor()
        # 
        # # Create AI agent
        # draft_agent = DraftAgent(client, deployment)
        # 
        # # Create the workflow
        # workflow = HumanInTheLoopWorkflow(
        #     ticket_intake=ticket_intake,
        #     draft_agent=draft_agent,
        #     draft_bridge=draft_bridge,
        #     finalize=finalize,
        # )
        
        # Sample support ticket
        sample_ticket = SupportTicket(
            ticket_id="TKT-78542",
            customer_id="CUST-12345",
            customer_name="Sarah Johnson",
            subject="Request for full refund on subscription",
            description="I signed up for the annual premium plan last week but the features don't work as advertised. "
                       "The video conferencing keeps dropping and the file storage is extremely slow. "
                       "I want a full refund and to cancel my subscription immediately.",
            priority=TicketPriority.HIGH
        )
        
        print("Incoming Support Ticket:")
        print(f"   Ticket ID: {sample_ticket.ticket_id}")
        print(f"   Customer: {sample_ticket.customer_name} ({sample_ticket.customer_id})")
        print(f"   Priority: {sample_ticket.priority.value}")
        print(f"   Subject: {sample_ticket.subject}")
        print(f"   Description: {sample_ticket.description}")
        print()
        
        # ============================================================================
        # STEP 4.8: Execute the workflow with human review
        # Uncomment the lines below and REMOVE the placeholder
        # ============================================================================
        # events = await workflow.run(sample_ticket, handle_supervisor_review)
        # 
        # # Display the final result
        # final_message = events[-1].data
        # print()
        # print(final_message)
        # print()
        # print("Human-in-the-loop workflow completed!")
        
        # Placeholder - REMOVE after uncommenting above
        print("Exercise 4 not completed. Please uncomment the code in demo.py and executors.py")
