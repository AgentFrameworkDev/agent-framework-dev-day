using WorkflowLab.Sequential;
using WorkflowLab.Concurrent;
using WorkflowLab.HumanInTheLoop;

/// <summary>
/// Workflow Lab - Main Entry Point
/// 
/// This lab demonstrates three key workflow patterns using Microsoft.Agents.AI:
/// 
/// 1. Sequential Workflow: Process tickets through a linear pipeline
///    - Ticket Intake -> AI Categorization -> AI Response Generation
/// 
/// 2. Concurrent Workflow: Fan-out to multiple agents simultaneously
///    - Question -> [Billing Expert + Technical Expert] -> Combined Response
/// 
/// 3. Human-in-the-Loop Workflow: AI assistance with human oversight
///    - Ticket -> AI Draft -> [Human Review/Approval] -> Final Response
/// 
/// All demos use a Customer Support Ticket System as the example scenario.
/// </summary>

Console.WriteLine("=====================================================================");
Console.WriteLine("                        WORKFLOW LAB                                 ");
Console.WriteLine("            Microsoft.Agents.AI Workflow Patterns                    ");
Console.WriteLine("=====================================================================");
Console.WriteLine();
Console.WriteLine("This lab demonstrates three workflow patterns using a");
Console.WriteLine("Customer Support Ticket System as the example scenario.");
Console.WriteLine();
Console.WriteLine("Configuration (appsettings.Local.json or environment variables):");
Console.WriteLine("  - AZURE_AI_PROJECT_ENDPOINT (required)");
Console.WriteLine("  - AZURE_AI_MODEL_DEPLOYMENT_NAME (required)");
Console.WriteLine("  - Authentication:");
Console.WriteLine("    - AZURE_TENANT_ID + AZURE_CLIENT_ID + AZURE_CLIENT_SECRET (Service Principal)");
Console.WriteLine("    - Or use DefaultAzureCredential (Managed Identity/CLI)");
Console.WriteLine();
Console.WriteLine("=====================================================================");
Console.WriteLine();
Console.WriteLine("Select a workflow demo to run:");
Console.WriteLine();
Console.WriteLine("  [1] Sequential Workflow");
Console.WriteLine("      Process tickets through a linear AI pipeline");
Console.WriteLine("      (Intake -> Categorization -> Response)");
Console.WriteLine();
Console.WriteLine("  [2] Concurrent Workflow");
Console.WriteLine("      Fan-out questions to multiple specialist agents");
Console.WriteLine("      (Question -> [Billing + Technical Experts] -> Combined)");
Console.WriteLine();
Console.WriteLine("  [3] Human-in-the-Loop Workflow");
Console.WriteLine("      AI-assisted responses with human supervisor review");
Console.WriteLine("      (Ticket -> AI Draft -> Human Review -> Final Response)");
Console.WriteLine();
Console.WriteLine("  [Q] Exit");
Console.WriteLine();

while (true)
{
    Console.Write("Enter your choice (1-3 or Q): ");
    var choice = Console.ReadLine()?.Trim().ToUpperInvariant();

    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "1":
                await SequentialWorkflowDemo.RunAsync();
                break;

            case "2":
                await ConcurrentWorkflowDemo.RunAsync();
                break;

            case "3":
                await HumanInTheLoopWorkflowDemo.RunAsync();
                break;

            case "Q":
                Console.WriteLine("Thank you for completing the Workflow Lab!");
                return;

            default:
                Console.WriteLine("Invalid choice. Please enter 1, 2, 3, or Q.");
                continue;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine();
        Console.WriteLine("Make sure your environment variables are configured correctly.");
    }

    Console.WriteLine();
    Console.WriteLine("=====================================================================");
    Console.WriteLine();
    Console.WriteLine("Run another demo? (1-3 or Q to exit)");
}
