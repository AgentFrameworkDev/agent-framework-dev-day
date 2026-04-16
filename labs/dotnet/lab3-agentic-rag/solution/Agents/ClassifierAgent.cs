using Lab3.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

/// <summary>
/// Classifier agent for routing queries to specialized search agents.
/// Uses structured output (ResponseFormat) to guarantee typed JSON classification.
/// </summary>
public static class ClassifierAgent
{
    private const string Instructions = """
        You are a query classification system for an IT support ticket database.
        Classify the incoming user question into exactly one category and return
        a JSON object with "category" and "reasoning" fields.

        ## Database Schema
        The database contains IT support tickets with these fields:
        - Id: unique identifier
        - Create_Date: date the ticket was created
        - Subject: ticket subject
        - Body: ticket question/description
        - Answer: ticket response/solution
        - Type: ticket type (values: "Incident", "Request", "Problem", "Change")
        - Queue: department name (values: "Human Resources", "IT", "Finance", "Operations", "Sales", "Marketing", "Engineering", "Support")
        - Priority: priority level (values: "high", "medium", "low")
        - Language: ticket language
        - Business_Type: business category
        - Tags: categorization tags

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
        """;

    public static AIAgent Create(ChatClient chatClient)
    {
        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            ChatOptions = new()
            {
                Instructions = Instructions,
                ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<ClassifyResult>()
            }
        });
    }
}
