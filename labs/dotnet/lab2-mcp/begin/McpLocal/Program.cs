// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - Local MCP Server (.NET EXE with STDIO transport)
// This demonstrates a local MCP server that runs as a subprocess
// Same tools as McpBridge for consistency

using McpLocal.Services;
using McpLocal.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.Error.WriteLine("Starting Local MCP Server (STDIO)...");
Console.Error.WriteLine("MCP Tools exposed (same as McpBridge):");
Console.Error.WriteLine("   - GetAllTickets (Support Tickets)");
Console.Error.WriteLine("   - GetTicket     (Support Tickets)");
Console.Error.WriteLine("   - UpdateTicket  (Support Tickets)");

// ========================================================================
// STEP 2: Create the host with MCP server configured for STDIO transport
// ========================================================================
// TODO: Build a .NET host with MCP server using STDIO transport
//
// Hints:
// - Use Host.CreateEmptyApplicationBuilder(settings: null)
// - Configure logging: builder.Logging.AddConsole(...) with LogToStandardErrorThreshold
// - Register TicketStore as singleton: builder.Services.AddSingleton<TicketStore>()
// - Register MCP Server: builder.Services.AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()
// - Build and run: await builder.Build().RunAsync()
//
// HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);
//
// // Configure logging to stderr (MCP uses stdout for JSON-RPC protocol)
// builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
// builder.Logging.SetMinimumLevel(LogLevel.Information);
//
// // Register the in-memory store
// builder.Services.AddSingleton<TicketStore>();
//
// // Register MCP Server with STDIO transport and discover tools from assembly
// builder.Services
//     .AddMcpServer()
//     .WithStdioServerTransport()
//     .WithToolsFromAssembly();
//
// Console.Error.WriteLine("MCP Server initialized with STDIO transport");
// Console.Error.WriteLine("Available tools: GetAllTickets, GetTicket, UpdateTicket");
//
// await builder.Build().RunAsync();
// ========================================================================
throw new NotImplementedException("STEP 2: Create the host with MCP server configured for STDIO transport");
