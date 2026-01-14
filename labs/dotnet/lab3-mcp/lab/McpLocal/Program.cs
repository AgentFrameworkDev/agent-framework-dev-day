// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - Local MCP Server (.NET EXE with STDIO transport)
// This demonstrates a local MCP server that runs as a subprocess

// ============================================================================
// EXERCISE 2: Create the MCP Server Host
// ============================================================================
// In this exercise, you will set up a .NET host application that runs an MCP
// server using STDIO transport. The server will expose tools for configuration
// and ticket management.
//
// TODO: Uncomment the code below step by step as instructed in EXERCISES.md
// ============================================================================

using McpLocal.Services;
using McpLocal.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.Error.WriteLine("Starting Local MCP Server (STDIO)...");
Console.Error.WriteLine("MCP Tools exposed:");
Console.Error.WriteLine("   - GetConfig / UpdateConfig (Configuration)");
Console.Error.WriteLine("   - GetTicket / UpdateTicket (Support Tickets)");

// ============================================================================
// STEP 2.1: Create the host builder
// Uncomment the line below to create an empty application builder
// ============================================================================
// HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);

// ============================================================================
// STEP 2.2: Configure logging to stderr
// MCP uses stdout for JSON-RPC protocol, so we log to stderr
// Uncomment the lines below to configure logging
// ============================================================================
// builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
// builder.Logging.SetMinimumLevel(LogLevel.Information);

// ============================================================================
// STEP 2.3: Register the in-memory stores
// These stores hold our configuration and ticket data
// Uncomment the lines below to register services
// ============================================================================
// builder.Services.AddSingleton<ConfigurationStore>();
// builder.Services.AddSingleton<TicketStore>();

// ============================================================================
// STEP 2.4: Register MCP Server with STDIO transport
// This is the key part - configuring MCP server with tools discovery
// Uncomment the lines below to register MCP server
// ============================================================================
// builder.Services
//     .AddMcpServer()
//     .WithStdioServerTransport()
//     .WithToolsFromAssembly();

Console.Error.WriteLine("MCP Server initialized with STDIO transport");
Console.Error.WriteLine("Available tools: GetConfig, UpdateConfig, GetTicket, UpdateTicket");

// ============================================================================
// STEP 2.5: Build and run the host
// Uncomment the line below to start the MCP server
// ============================================================================
// await builder.Build().RunAsync();

// Placeholder to prevent compile error - REMOVE after uncommenting above
Console.Error.WriteLine("Exercise not completed. Please uncomment the code above.");
await Task.Delay(1000);
