// Copyright (c) Microsoft. All rights reserved.
// MCP Bridge Server
// An MCP server that wraps REST API backend (same as Python mcp_bridge)

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register HttpClient for REST API calls
builder.Services.AddHttpClient();

// Register MCP Server with HTTP/SSE transport and discover tools from assembly
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", server = "MCP Bridge Server" }));

// MCP endpoint at /mcp path
app.MapMcp("/mcp");

var port = args.FirstOrDefault(a => a.StartsWith("--port="))?.Split('=')[1] ?? "5070";
var url = $"http://localhost:{port}";

Console.WriteLine("===============================================================================");
Console.WriteLine("              MCP Bridge Server (HTTP/SSE Transport)                          ");
Console.WriteLine("===============================================================================");
Console.WriteLine();
Console.WriteLine($"Server URL:      {url}");
Console.WriteLine($"MCP Endpoint:    {url}/mcp");
Console.WriteLine($"Health Check:    {url}/health");
Console.WriteLine();
Console.WriteLine("Available MCP Tools (calls REST API at :5060):");
Console.WriteLine("   - GetAllTickets : Gets all support tickets from REST API");
Console.WriteLine("   - GetTicket     : Gets a support ticket by ID");
Console.WriteLine("   - UpdateTicket  : Updates a support ticket status");
Console.WriteLine();
Console.WriteLine("MCP Server ready! Connect your MCP client to {0}/mcp", url);

app.Run(url);
