using Microsoft.Extensions.Configuration;
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

// Load and validate configuration at startup
var configPath = FindConfigPath(AppContext.BaseDirectory);
var configFile = Path.Combine(configPath, "appsettings.Local.json");

var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
    .SetBasePath(configPath)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine("=====================================================================");
Console.WriteLine("                        WORKFLOW LAB                                 ");
Console.WriteLine("            Microsoft.Agents.AI Workflow Patterns                    ");
Console.WriteLine("=====================================================================");
Console.WriteLine();
Console.WriteLine("This lab demonstrates three workflow patterns using a");
Console.WriteLine("Customer Support Ticket System as the example scenario.");
Console.WriteLine();

// Validate and display loaded configuration values
Console.WriteLine($"📁 Config path: {configPath}");
Console.WriteLine($"📄 Config file: {configFile}");
Console.WriteLine($"   File exists: {(File.Exists(configFile) ? "✅ Yes" : "❌ No")}");
Console.WriteLine();

var cfgEndpoint = configuration["AZURE_OPENAI_ENDPOINT"];
var cfgDeployment = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
    ?? configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"]
    ?? "gpt-4o-mini";
var cfgApiKey = configuration["AZURE_OPENAI_API_KEY"];
var cfgTenantId = configuration["AZURE_TENANT_ID"];
var cfgClientId = configuration["AZURE_CLIENT_ID"];
var cfgClientSecret = configuration["AZURE_CLIENT_SECRET"];

static string Mask(string? val) =>
    string.IsNullOrEmpty(val) ? "" : (val.Length > 8 ? $"{val[..4]}****{val[^4..]}" : "****");

Console.WriteLine("Configuration Values:");
Console.WriteLine($"  AZURE_OPENAI_ENDPOINT:         {(string.IsNullOrEmpty(cfgEndpoint) ? "❌ NOT SET" : $"✅ {cfgEndpoint}")}");
Console.WriteLine($"  AZURE_OPENAI_DEPLOYMENT_NAME:  ✅ {cfgDeployment}");
Console.WriteLine($"  AZURE_OPENAI_API_KEY:          {(string.IsNullOrEmpty(cfgApiKey) ? "⚠️  not set" : $"✅ {Mask(cfgApiKey)}")}");
Console.WriteLine($"  AZURE_TENANT_ID:               {(string.IsNullOrEmpty(cfgTenantId) ? "⚠️  not set" : $"✅ {cfgTenantId}")}");
Console.WriteLine($"  AZURE_CLIENT_ID:               {(string.IsNullOrEmpty(cfgClientId) ? "⚠️  not set" : $"✅ {cfgClientId}")}");
Console.WriteLine($"  AZURE_CLIENT_SECRET:           {(string.IsNullOrEmpty(cfgClientSecret) ? "⚠️  not set" : "✅ ********")}");
Console.WriteLine();

if (string.IsNullOrEmpty(cfgEndpoint))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERROR: AZURE_OPENAI_ENDPOINT is required. Set it in: {configFile}");
    Console.ResetColor();
    return;
}

if (string.IsNullOrEmpty(cfgApiKey) && (string.IsNullOrEmpty(cfgTenantId) || string.IsNullOrEmpty(cfgClientId) || string.IsNullOrEmpty(cfgClientSecret)))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("WARNING: No API Key or Service Principal configured. Will fall back to DefaultAzureCredential.");
    Console.ResetColor();
}
Console.WriteLine();

// ========================================================================
// STEP 5: Create the menu system to run workflow demos
// ========================================================================
// TODO: Implement a menu loop that lets users select and run workflow demos
//
// Hints:
// - Display menu options: [1] Sequential, [2] Concurrent, [3] Human-in-the-Loop, [Q] Exit
// - Read user choice and call the appropriate demo's RunAsync() method
// - Wrap in a while loop to allow running multiple demos
// - Handle exceptions gracefully
//
// Console.WriteLine("=====================================================================");
// Console.WriteLine();
// Console.WriteLine("Select a workflow demo to run:");
// Console.WriteLine();
// Console.WriteLine("  [1] Sequential Workflow");
// Console.WriteLine("      Process tickets through a linear AI pipeline");
// Console.WriteLine("      (Intake -> Categorization -> Response)");
// Console.WriteLine();
// Console.WriteLine("  [2] Concurrent Workflow");
// Console.WriteLine("      Fan-out questions to multiple specialist agents");
// Console.WriteLine("      (Question -> [Billing + Technical Experts] -> Combined)");
// Console.WriteLine();
// Console.WriteLine("  [3] Human-in-the-Loop Workflow");
// Console.WriteLine("      AI-assisted responses with human supervisor review");
// Console.WriteLine("      (Ticket -> AI Draft -> Human Review -> Final Response)");
// Console.WriteLine();
// Console.WriteLine("  [Q] Exit");
// Console.WriteLine();
//
// while (true)
// {
//     Console.Write("Enter your choice (1-3 or Q): ");
//     var choice = Console.ReadLine()?.Trim().ToUpperInvariant();
//
//     Console.WriteLine();
//
//     try
//     {
//         switch (choice)
//         {
//             case "1":
//                 await SequentialWorkflowDemo.RunAsync();
//                 break;
//
//             case "2":
//                 await ConcurrentWorkflowDemo.RunAsync();
//                 break;
//
//             case "3":
//                 await HumanInTheLoopWorkflowDemo.RunAsync();
//                 break;
//
//             case "Q":
//                 Console.WriteLine("Thank you for completing the Workflow Lab!");
//                 return;
//
//             default:
//                 Console.WriteLine("Invalid choice. Please enter 1, 2, 3, or Q.");
//                 continue;
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine();
//         Console.WriteLine($"Error: {ex.Message}");
//         Console.WriteLine();
//         Console.WriteLine("Make sure your environment variables are configured correctly.");
//     }
//
//     Console.WriteLine();
//     Console.WriteLine("=====================================================================");
//     Console.WriteLine();
//     Console.WriteLine("Run another demo? (1-3 or Q to exit)");
// }
// ========================================================================
throw new NotImplementedException("STEP 5: Create the menu system to run workflow demos");

static string FindConfigPath(string startPath)
{
    var currentDir = new DirectoryInfo(startPath);

    // Traverse up to find the 'dotnet' folder
    while (currentDir != null)
    {
        if (currentDir.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return currentDir.FullName;
        }
        currentDir = currentDir.Parent;
    }

    // Fallback to start path if dotnet folder not found
    return startPath;
}
