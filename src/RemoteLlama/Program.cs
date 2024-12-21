using System.CommandLine;
using Microsoft.Extensions.Logging;
using RemoteLlama;
using RemoteLlama.Helpers;

// Create logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<CommandManager>();
var consoleHelper = new ConsoleHelper();
var commandManager = new CommandManager(logger, consoleHelper);
var rootCommand = commandManager.CreateRootCommand();

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
