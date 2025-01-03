using System.CommandLine;
using Microsoft.Extensions.Logging;
using RemoteLlama.CommandHandlers;
using RemoteLlama.Helpers;

namespace RemoteLlama;

/// <summary>
/// Manages the creation and configuration of CLI commands for RemoteLlama.
/// This class is responsible for setting up all available commands and their handlers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the CommandManager class.
/// </remarks>
/// <param name="logger">The logger instance for recording operations and errors</param>
/// <param name="consoleHelper">The console helper instance for user interaction</param>
public class CommandManager(ILogger logger, ConsoleHelper consoleHelper)
{
    private readonly ILogger _logger = logger;
    private readonly ConsoleHelper _consoleHelper = consoleHelper;

    /// <summary>
    /// Creates and configures the root command with all available subcommands.
    /// </summary>
    /// <returns>A configured RootCommand instance ready for CLI parsing</returns>
    public RootCommand CreateRootCommand()
    {
        var rootCommand = new RootCommand("RemoteLlama - Manage local LLM models");
        
        // Add all available subcommands to the root command
        rootCommand.AddCommand(CreatePullCommand());
        rootCommand.AddCommand(CreateRemoveCommand());
        rootCommand.AddCommand(CreateSetUrlCommand());
        rootCommand.AddCommand(CreateServeCommand());
        
        return rootCommand;
    }

    /// <summary>
    /// Creates the 'pull' command for downloading models from Hugging Face.
    /// </summary>
    /// <returns>A configured Command instance for the pull operation</returns>
    private Command CreatePullCommand()
    {
        // Define the command and its required model ID argument
        var command = new Command("pull", "Pull a model from Hugging Face");
        var argument = new Argument<string>("model", "The model ID to pull");
        command.AddArgument(argument);

        // Set up the command handler that will execute when this command is invoked
        command.SetHandler(async (model) =>
        {
            var handler = new PullCommandHandler(model, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, argument);

        return command;
    }

    /// <summary>
    /// Creates the 'rm' command for removing downloaded models.
    /// </summary>
    /// <returns>A configured Command instance for the remove operation</returns>
    private Command CreateRemoveCommand()
    {
        // Define the command and its required model ID argument
        var command = new Command("rm", "Remove a downloaded model");
        var argument = new Argument<string>("model", "The model ID to remove");
        command.AddArgument(argument);

        // Set up the command handler that will execute when this command is invoked
        command.SetHandler(async (model) =>
        {
            var handler = new RemoveCommandHandler(model, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, argument);

        return command;
    }

    /// <summary>
    /// Creates the 'set-url' command for configuring the API URL.
    /// </summary>
    /// <returns>A configured Command instance for the set-url operation</returns>
    private Command CreateSetUrlCommand()
    {
        // Define the command and its required URL argument
        var command = new Command("set-url", "Set the API URL");
        var argument = new Argument<string>("url", "The URL to set");
        command.AddArgument(argument);

        // Set up the command handler that will execute when this command is invoked
        command.SetHandler(async (url) =>
        {
            var handler = new SetUrlCommandHandler(url, _logger);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, argument);

        return command;
    }

    /// <summary>
    /// Creates the 'serve' command for starting the RemoteLlama proxy server.
    /// </summary>
    private Command CreateServeCommand()
    {
        var command = new Command("serve", "Start the RemoteLlama proxy server");
        command.SetHandler(async () =>
        {
            var handler = new ServeCommandHandler(_logger);
            await handler.ExecuteAsync().ConfigureAwait(false);
        });

        return command;
    }
} 