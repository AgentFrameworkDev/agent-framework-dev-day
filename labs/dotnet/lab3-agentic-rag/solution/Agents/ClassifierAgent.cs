using Microsoft.Agents.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

/// <summary>
/// Classifier agent for routing queries to specialized search agents.
/// </summary>
public static class ClassifierAgent
{
    private const string Instructions = """
        You are a query classification system for an IT support ticket database. 
        Your task is to route questions to specialist search agents based on the user question.

        ## Database Schema
        The database contains IT support tickets with these fields:
        - Id: unique identifier
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

        ## Types of Searches Agents can do:

        **DIFFERENCE_AGENT**: Questions asking for items that match one criterion but EXCLUDE/NOT another
           - **CRITICAL**: Look for negation words combined with "which", "what", "find", "show", "list"
           - **Negation indicators**: "not", "don't", "doesn't", "without", "excluding", "except", "no", "never"
           - **Exclusion phrases**: "does not mention", "does not involve", "don't mention", "don't involve", "not related to", "not about"
           - **Pattern**: [Which/What/Find] [TOPIC] [NEGATION] [EXCLUSION_TERM]
           - Examples:
             - "Which Dell XPS Issue does not mention Windows?" ✓ DIFFERENCE_AGENT
             - "What Surface problems don't involve the battery?" ✓ DIFFERENCE_AGENT
             - "Find tickets without high priority" ✓ DIFFERENCE_AGENT

        **INTERSECTION_AGENT**: Questions asking for items that match MULTIPLE criteria (AND logic)
           - **Only use when "and" combines SEARCH TOPICS, not database field filters**
           - Keywords: "and", "both", "that also", "with", "plus", "as well as"
           - Pattern: [What/Which/Find] [SEARCH_TOPIC_A] [AND] [SEARCH_TOPIC_B]
           - Examples:
             - "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?" ✓ INTERSECTION_AGENT

        **MULTI_HOP_AGENT**: Questions requiring multi-step reasoning (find X, then extract Y from X)
           - **Pattern**: "What [FIELD] had/has [CONDITION]?" or "Which [FIELD] [CONDITION]?"
           - Examples:
             - "What department had consultants with Login Issues?" ✓ MULTI_HOP_AGENT

        **COMPARATIVE_AGENT**: Questions comparing multiple items (more/less, vs, or)
           - **Keywords**: "more", "less", "vs", "versus", "or", "compared to", "better", "worse"
           - Examples:
             - "Do we have more issues with MacBook Air computers or Dell XPS laptops?" ✓ COMPARATIVE_AGENT

        **YES_NO_AGENT**: Simple yes/no questions
           - Keywords: "is", "are", "can", "does", "do", "will", "should", "any"
           - Examples:
             - "Are there any issues for Dell XPS laptops?"

        **COUNT_AGENT**: Counting questions
           - Keywords: "how many", "number of", "count of", "total"
           - Examples:
             - "How many tickets were logged for Human Resources?"

        **SEMANTIC_SEARCH_AGENT**: Queries looking for similar issues, solutions, or general information
           - Keywords: "how to", "why", "what causes", "solve", "fix", "issue with", "problem with"
           - Examples:
             - "What problems are there with Surface devices?"
        """;

    public static AIAgent Create(ChatClient chatClient)
    {
        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "classifier_agent"
        );
    }
}
