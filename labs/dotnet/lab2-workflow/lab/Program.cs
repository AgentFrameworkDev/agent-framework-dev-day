// Copyright (c) Microsoft. All rights reserved.
// Workflow Lab - Main Entry Point

// ============================================================================
// EXERCISE 5: Enable All Workflow Demos
// ============================================================================
// This is the main entry point that provides a menu to run different workflow demos.
// After completing Exercises 1-4, uncomment the demo calls below.
// ============================================================================

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
Console.WriteLine("Environment Variables Required:");
Console.WriteLine("  - AZURE_OPENAI_ENDPOINT (required)");
Console.WriteLine("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, default: gpt-4o-mini)");
Console.WriteLine("  - Authentication (one of the following):");
Console.WriteLine("    - AZURE_OPENAI_API_KEY (API Key auth)");
Console.WriteLine("    - AZURE_TENANT_ID + AZURE_CLIENT_ID + AZURE_CLIENT_SECRET (Service Principal)");
Console.WriteLine("    - None (uses DefaultAzureCredential/Managed Identity)");
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
                // ============================================================================
                // STEP 5.1: Enable Sequential Workflow Demo
                // Uncomment the line below after completing Exercise 2
                // ============================================================================
                // await SequentialWorkflowDemo.RunAsync();
                Console.WriteLine("Exercise 2 not completed. Uncomment the SequentialWorkflowDemo.RunAsync() call.");
                break;

            case "2":
                // ============================================================================
                // STEP 5.2: Enable Concurrent Workflow Demo
                // Uncomment the line below after completing Exercise 3
                // ============================================================================
                // await ConcurrentWorkflowDemo.RunAsync();
                Console.WriteLine("Exercise 3 not completed. Uncomment the ConcurrentWorkflowDemo.RunAsync() call.");
                break;

            case "3":
                // ============================================================================
                // STEP 5.3: Enable Human-in-the-Loop Workflow Demo
                // Uncomment the line below after completing Exercise 4
                // ============================================================================
                // await HumanInTheLoopWorkflowDemo.RunAsync();
                Console.WriteLine("Exercise 4 not completed. Uncomment the HumanInTheLoopWorkflowDemo.RunAsync() call.");
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
