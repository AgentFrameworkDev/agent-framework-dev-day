// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - MCP Bridge Server
// This is a dedicated MCP server (HTTP/SSE) that calls the REST API backend

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add HttpClient factory for calling REST API
builder.Services.AddHttpClient("RestApi", client =>
{
    // Configure base URL from settings (defaults to localhost:5060)
    var baseUrl = builder.Configuration.GetValue<string>("RestApi:BaseUrl") ?? "http://localhost:5060";
    client.BaseAddress = new Uri(baseUrl);
});

// Register MCP Server with HTTP/SSE transport and discover tools from assembly
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", server = "MCP Bridge Server" }));

// MCP endpoint using SSE (Server-Sent Events) transport
app.MapMcp();

var port = args.FirstOrDefault(a => a.StartsWith("--port="))?.Split('=')[1] ?? "5070";
var restApiUrl = builder.Configuration.GetValue<string>("RestApi:BaseUrl") ?? "http://localhost:5060";
var url = $"http://localhost:{port}";

Console.WriteLine("===============================================================================");
Console.WriteLine("              MCP Workshop - MCP Bridge Server (HTTP/SSE)                     ");
Console.WriteLine("===============================================================================");
Console.WriteLine();
Console.WriteLine("ARCHITECTURE:");
Console.WriteLine("   +--------------+   HTTP/SSE    +--------------+   HTTP/REST    +--------------+");
Console.WriteLine("   |  AI Agent /  | ------------> |  MCP Bridge  | -------------> |  REST API    |");
Console.WriteLine("   |  Your App    |  (MCP)        |  (This)      |                |  (Backend)   |");
Console.WriteLine("   +--------------+               +--------------+                +--------------+");
Console.WriteLine();
Console.WriteLine($"This MCP Bridge: {url}");
Console.WriteLine($"MCP Endpoint:    {url}/sse");
Console.WriteLine($"REST API Target: {restApiUrl}");
Console.WriteLine();
Console.WriteLine("MCP Tools (call REST API):");
Console.WriteLine("   - GetTicket    -> GET  {REST_API}/api/tickets[/{id}]");
Console.WriteLine("   - UpdateTicket -> PUT  {REST_API}/api/tickets/{id}");
Console.WriteLine();
Console.WriteLine("Health Check: {0}/health", url);
Console.WriteLine();
Console.WriteLine("MCP Bridge ready! Connect your AI Agent to {0}/sse", url);

app.Run(url);
