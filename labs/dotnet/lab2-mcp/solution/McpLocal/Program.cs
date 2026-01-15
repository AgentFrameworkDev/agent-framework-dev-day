// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - Local MCP Server (.NET EXE with STDIO transport)
// This demonstrates a local MCP server that runs as a subprocess

using McpLocal.Services;
using McpLocal.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.Error.WriteLine("Starting Local MCP Server (STDIO)...");
Console.Error.WriteLine("MCP Tools exposed:");
Console.Error.WriteLine("   - GetConfig / UpdateConfig (Configuration)");
Console.Error.WriteLine("   - GetTicket / UpdateTicket (Support Tickets)");

// Create the host with MCP server configured for STDIO transport
HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Configure logging to stderr (MCP uses stdout for JSON-RPC protocol)
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register the in-memory stores
builder.Services.AddSingleton<ConfigurationStore>();
builder.Services.AddSingleton<TicketStore>();

// Register MCP Server with STDIO transport and discover tools from assembly
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

Console.Error.WriteLine("MCP Server initialized with STDIO transport");
Console.Error.WriteLine("Available tools: GetConfig, UpdateConfig, GetTicket, UpdateTicket");

await builder.Build().RunAsync();
