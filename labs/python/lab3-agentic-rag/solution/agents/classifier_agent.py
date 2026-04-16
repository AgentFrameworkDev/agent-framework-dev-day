"""
Classifier agent for routing queries to specialized search agents.

Uses structured output (response_format) to guarantee typed classification,
an @executor converter to extract the result, and a QueryBridge executor
to forward the user question to the appropriate specialist agent.
"""
from typing import Literal

from pydantic import BaseModel

from agent_framework import (
    Agent,
    AgentExecutorResponse,
    Executor,
    WorkflowContext,
    executor,
    handler,
)
from agent_framework.openai import OpenAIChatClient


# ---------------------------------------------------------------------------
# Pydantic models for structured output & routing
# ---------------------------------------------------------------------------

CATEGORY_LITERAL = Literal[
    "yes_no",
    "semantic_search",
    "count",
    "difference",
    "intersection",
    "multi_hop",
    "comparative",
]


class ClassifyResult(BaseModel):
    """Structured classification result returned by the classifier agent."""

    category: CATEGORY_LITERAL
    reasoning: str


class ClassifiedQuery(BaseModel):
    """Carries both the classification and original user question for routing."""

    category: CATEGORY_LITERAL
    user_question: str


# ---------------------------------------------------------------------------
# Executor: extract the structured classification and forward it
# ---------------------------------------------------------------------------

@executor(id="extract_category")
async def extract_category(
    response: AgentExecutorResponse,
    ctx: WorkflowContext[ClassifiedQuery],
) -> None:
    """Parse the classifier's structured JSON output and send it downstream."""
    text = response.agent_response.text.strip()

    # Strip markdown code fences if present
    if text.startswith("```"):
        text = text.split("\n", 1)[-1].rsplit("```", 1)[0].strip()

    # Extract the first complete JSON object in case of trailing characters
    brace_depth = 0
    json_end = -1
    for i, ch in enumerate(text):
        if ch == "{":
            brace_depth += 1
        elif ch == "}":
            brace_depth -= 1
            if brace_depth == 0:
                json_end = i + 1
                break
    if json_end > 0:
        text = text[:json_end]

    result = ClassifyResult.model_validate_json(text)
    print(f"  [Classified as: {result.category} -- {result.reasoning}]")

    # Retrieve the original user question from the conversation history
    user_question = next(
        (m.text for m in response.full_conversation if m.role == "user" and m.text),
        "",
    )
    await ctx.send_message(
        ClassifiedQuery(category=result.category, user_question=user_question)
    )


# ---------------------------------------------------------------------------
# Bridge executor: convert ClassifiedQuery -> str for specialist agents
# ---------------------------------------------------------------------------

class QueryBridge(Executor):
    """Converts a ClassifiedQuery into a plain string for a downstream agent."""

    @handler
    async def forward(
        self, query: ClassifiedQuery, ctx: WorkflowContext[str]
    ) -> None:
        await ctx.send_message(query.user_question)


CLASSIFIER_AGENT_INSTRUCTIONS = """
You are a query classification system for an IT support ticket database.
Classify the incoming user question into exactly one category and return
a JSON object with "category" and "reasoning" fields.

## Database Schema
The database contains IT support tickets with these fields:
- Id, Subject, Body, Answer
- Type: "Incident", "Request", "Problem", "Change"
- Queue: "Human Resources", "IT", "Finance", "Operations", "Sales", "Marketing", "Engineering", "Support"
- Priority: "high", "medium", "low"
- Language, Business_Type, Tags

**IMPORTANT**: When "and" combines field values (Type, Queue, Priority), these are FILTERS for counting/searching, NOT separate items to compare.

## Categories (use these exact values):

**difference**: Questions with negation/exclusion ("not", "don't", "without", "excluding").
  - "Which Dell XPS Issue does not mention Windows?" -> difference
  - "Find tickets without high priority" -> difference

**intersection**: Questions combining multiple SEARCH TOPICS with "and", "both", "that also".
  - "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?" -> intersection
  - NOT when "and" combines field filters like Priority/Queue/Type.

**multi_hop**: Questions asking for a different attribute than what's searched (find X, extract Y).
  - "What department had consultants with Login Issues?" -> multi_hop
  - "Which queue handles password reset requests?" -> multi_hop

**comparative**: Questions comparing multiple items ("more", "less", "vs", "or").
  - "Do we have more issues with MacBook Air or Dell XPS?" -> comparative

**yes_no**: Explicit yes/no questions expecting a boolean answer.
  - "Are there any issues for Dell XPS laptops?" -> yes_no

**count**: Counting questions ("how many", "count", "total", "number of").
  - "How many Incidents for Human Resources and low priority?" -> count (all filters!)

**semantic_search**: General queries about issues, solutions, how-to (everything else).
  - "What problems are there with Surface devices?" -> semantic_search

## Classification Priority
1. COUNTING first: "how many", "count", "total" -> count
2. NEGATION: "not", "don't", "without" -> difference
3. COMPARISON: "more", "less", "vs", "or" comparing items -> comparative
4. INTERSECTION: multiple search topics with "and" (NOT field filters) -> intersection
5. MULTI-HOP: "What [FIELD] had [CONDITION]" -> multi_hop
6. YES/NO: explicit boolean questions -> yes_no
7. Everything else -> semantic_search

## Key Rules
- Field values (Priority, Queue, Type) are FILTERS, not search topics
- "How many X and Y and Z?" = count (filters). "What X and Y?" = intersection (topics)
- "Which X does not mention Y?" = difference, NOT count
"""


def create_classifier_agent(chat_client: OpenAIChatClient) -> Agent:
    """
    Create the classifier agent that routes queries to specialists.

    Uses response_format to enforce structured JSON output matching
    the ClassifyResult schema.

    Args:
        chat_client: Azure OpenAI chat client

    Returns:
        Configured classifier Agent
    """
    return chat_client.as_agent(
        instructions=CLASSIFIER_AGENT_INSTRUCTIONS,
        name="classifier_agent",
        default_options={"response_format": ClassifyResult},
        require_per_service_call_history_persistence=True,
    )
