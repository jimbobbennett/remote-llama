using System.CommandLine;
using Microsoft.Extensions.Logging;
using RemoteLlama;
using RemoteLlama.CommandHandlers;
using RemoteLlama.Helpers;

// Create logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        // .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<CommandManager>();
var consoleHelper = new ConsoleHelper();

// Hack to catch --version or -v as the RootCommand hard codes this to show the csproj version
if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v"))
{
    // Call the version endpoint
    var versionCommandHandler = new VersionCommandHandler(logger, consoleHelper);
    await versionCommandHandler.ExecuteAsync().ConfigureAwait(false);
    
    return 0;
}

var commandManager = new CommandManager(logger, consoleHelper);
var rootCommand = commandManager.CreateRootCommand();

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);
