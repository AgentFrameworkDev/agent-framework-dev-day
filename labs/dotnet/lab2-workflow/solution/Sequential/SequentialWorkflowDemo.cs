using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowLab.Common;

namespace WorkflowLab.Sequential;

/// <summary>
/// Sequential Workflow Demo - Customer Support Ticket System
/// 
/// This demonstrates a sequential AI-powered workflow that processes customer support tickets:
/// 1. Ticket Intake Executor: Receives and validates the incoming support ticket
/// 2. AI Categorization Agent: Analyzes and categorizes the ticket (billing, technical, general)
/// 3. AI Response Agent: Generates an appropriate response based on the category
/// 
/// Concepts covered:
/// - Executors (function-based and class-based)
/// - Direct Edges
/// - Workflow Builder
/// - Events
/// - AI Agent Integration with ChatClientAgent
/// </summary>
public static class SequentialWorkflowDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Sequential Workflow Demo - Customer Support Ticket System ===");
        Console.WriteLine();
        Console.WriteLine("This workflow demonstrates sequential processing of support tickets:");
        Console.WriteLine("  1. Ticket Intake -> 2. AI Categorization -> 3. AI Response Generation");
        Console.WriteLine();

        // Set up the Azure OpenAI client
        var chatClient = AzureOpenAIClientFactory.CreateChatClient();

        // Create executors
        var ticketIntake = new TicketIntakeExecutor();
        var categorizationBridge = new CategorizationBridgeExecutor();
        var responseBridge = new ResponseBridgeExecutor();

        // AI Categorization Agent - categorizes the ticket
        ChatClientAgent categorizationAgent = new(
            chatClient,
            name: "CategorizationAgent",
            instructions: """
                You are a customer support ticket categorization specialist.
                Analyze the incoming support ticket and categorize it into one of these categories:
                - BILLING: Payment issues, invoices, subscription, refunds
                - TECHNICAL: Software bugs, errors, performance issues, how-to questions
                - GENERAL: Account inquiries, feedback, general questions
                
                Respond with a JSON object in this exact format:
                {"category": "CATEGORY_NAME", "priority": "HIGH|MEDIUM|LOW", "summary": "brief summary"}
                
                Keep your response concise and only output the JSON.
                """
        );

        // AI Response Agent - generates the customer response
        ChatClientAgent responseAgent = new(
            chatClient,
            name: "ResponseAgent",
            instructions: """
                You are a friendly and professional customer support representative.
                Based on the ticket category and details provided, generate a helpful response to the customer.
                
                Guidelines:
                - Be empathetic and professional
                - Acknowledge the customer's issue
                - Provide relevant next steps or solutions
                - Keep the response concise (3-4 sentences)
                - Include a reference ticket number format: TKT-XXXXX
                """
        );

        // Build the sequential workflow
        var workflow = new WorkflowBuilder(ticketIntake)
            .AddEdge(ticketIntake, categorizationAgent)
            .AddEdge(categorizationAgent, categorizationBridge)
            .AddEdge(categorizationBridge, responseAgent)
            .AddEdge(responseAgent, responseBridge)
            .WithOutputFrom(responseBridge)
            .Build();

        // Sample customer support ticket
        var sampleTicket = new SupportTicket(
            TicketId: "TKT-12345",
            CustomerId: "CUST-12345",
            CustomerName: "John Smith",
            Subject: "Unable to access my account after password reset",
            Description: "I tried to reset my password yesterday but now I cannot log in. " +
                         "I've tried multiple times and keep getting an 'invalid credentials' error. " +
                         "This is urgent as I need to access my billing information.",
            Priority: TicketPriority.High
        );

        Console.WriteLine("Incoming Support Ticket:");
        Console.WriteLine($"   Ticket ID: {sampleTicket.TicketId}");
        Console.WriteLine($"   Customer: {sampleTicket.CustomerName} ({sampleTicket.CustomerId})");
        Console.WriteLine($"   Priority: {sampleTicket.Priority}");
        Console.WriteLine($"   Subject: {sampleTicket.Subject}");
        Console.WriteLine($"   Description: {sampleTicket.Description}");
        Console.WriteLine();
        Console.WriteLine("Processing ticket through workflow...");
        Console.WriteLine();

        // Execute the workflow with the support ticket
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, sampleTicket);
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case ExecutorCompletedEvent executorComplete:
                    Console.WriteLine($"[{executorComplete.ExecutorId}] completed");
                    if (executorComplete.Data is string data && !string.IsNullOrWhiteSpace(data))
                    {
                        Console.WriteLine($"   Output: {data}");
                    }
                    Console.WriteLine();
                    break;

                case WorkflowOutputEvent output:
                    Console.WriteLine("=== Final Customer Response ===");
                    Console.WriteLine(output.Data);
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Sequential workflow completed!");
    }
}
