// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - REST API Server (Backend Service)
// This is a pure REST API server - no MCP here
// The MCP Bridge (separate project) calls this API

using RemoteServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add API controllers (REST API endpoints)
builder.Services.AddControllers();

// Register the in-memory stores (used by REST API controllers)
builder.Services.AddSingleton<TicketStore>();

var app = builder.Build();

// Map REST API controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", server = "REST API Server" }));

var port = args.FirstOrDefault(a => a.StartsWith("--port="))?.Split('=')[1] ?? "5060";
var url = $"http://localhost:{port}";

Console.WriteLine("===============================================================================");
Console.WriteLine("              MCP Workshop - REST API Server (Backend)                        ");
Console.WriteLine("===============================================================================");
Console.WriteLine();
Console.WriteLine("This is the backend REST API that the MCP Bridge calls:");
Console.WriteLine();
Console.WriteLine("   +--------------+   HTTP/SSE    +--------------+   HTTP/REST    +--------------+");
Console.WriteLine("   |  AI Agent    | ------------> |  MCP Bridge  | -------------> |  REST API    |");
Console.WriteLine("   |              |               |  (:5070)     |                |  (This:5060) |");
Console.WriteLine("   +--------------+               +--------------+                +--------------+");
Console.WriteLine();
Console.WriteLine($"REST API URL: {url}");
Console.WriteLine();
Console.WriteLine("REST API Endpoints:");
Console.WriteLine($"   GET    {url}/api/tickets             -> List all tickets");
Console.WriteLine($"   GET    {url}/api/tickets?status=Open -> Filter by status");
Console.WriteLine($"   GET    {url}/api/tickets/{{id}}        -> Get ticket by ID");
Console.WriteLine($"   PUT    {url}/api/tickets/{{id}}        -> Update ticket");
Console.WriteLine();
Console.WriteLine("Health Check: {0}/health", url);
Console.WriteLine();
Console.WriteLine("REST API ready! Start the MCP Bridge to expose via MCP.");

app.Run(url);
