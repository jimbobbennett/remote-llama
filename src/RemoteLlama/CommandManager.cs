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
internal class CommandManager(ILogger logger, IConsoleHelper consoleHelper)
{
    private readonly ILogger _logger = logger;
    private readonly IConsoleHelper _consoleHelper = consoleHelper;

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
        rootCommand.AddCommand(CreateCreateCommand());
        rootCommand.AddCommand(CreateCopyCommand());
        rootCommand.AddCommand(CreateShowCommand());
        rootCommand.AddCommand(CreateListCommand());
        rootCommand.AddCommand(CreatePsCommand());
        rootCommand.AddCommand(CreateStopCommand());

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
        var modelArgument = new Argument<string>("model", "The model ID to pull");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for pull");
        var insecureFlag = new Option<bool>("--insecure", "Use an insecure registry");

        command.AddArgument(modelArgument);
        command.AddOption(helpFlag);
        command.AddOption(insecureFlag);

        // Set up the command handler that will execute when this command is invoked
        command.SetHandler(async (model, help, insecure) =>
        {
            if (help)
            {
                return;
            }

            var handler = new PullCommandHandler(model, insecure, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, modelArgument, helpFlag, insecureFlag);

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
        var modelArgument = new Argument<string[]>("model", "The model ID to remove")
        {
            Arity = ArgumentArity.OneOrMore
        };
        var helpFlag = new Option<bool>(["-h", "--help"], "help for rm");

        command.AddArgument(modelArgument);
        command.AddOption(helpFlag);

        // Set up the command handler that will execute when this command is invoked
        command.SetHandler(async (models, help) =>
        {
            if (help)
            {
                return;
            }

            foreach (var model in models)
            {
                var handler = new RemoveCommandHandler(model, _logger, _consoleHelper);
                await handler.ExecuteAsync().ConfigureAwait(false);
            }
        }, modelArgument, helpFlag);

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
            var handler = new SetUrlCommandHandler(url, _logger, _consoleHelper);
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
            var handler = new ServeCommandHandler(_logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        });

        return command;
    }

    /// <summary>
    /// Creates the 'create' command for creating a model from a Modelfile.
    /// </summary>
    /// <returns></returns>
    private Command CreateCreateCommand()
    {
        var command = new Command("create", "Create a model from a Modelfile");
        var modelArgument = new Argument<string>("model", "The model to create");
        var fileFlag = new Option<string>(["-f", "--file"], "Name of the Modelfile");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for create");
        var quantizeFlag = new Option<string>(["-q", "--quantize"], "Quantize model to this level (e.g. q4_0)");

        command.AddArgument(modelArgument);
        command.AddOption(fileFlag);
        command.AddOption(helpFlag);
        command.AddOption(quantizeFlag);

        command.SetHandler(async (model, file, help, quantize) =>
        {
            if (help)
            {
                return;
            }

            var handler = new CreateCommandHandler(model, file, quantize, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, modelArgument, fileFlag, helpFlag, quantizeFlag);

        return command;
    }

    /// <summary>
    /// Creates the 'cp' command for copying a model.
    /// </summary>
    /// <returns></returns>
    private Command CreateCopyCommand()
    {
        var command = new Command("cp", "Copy a model");
        var sourceArgument = new Argument<string>("source", "The model source");
        var destinationArgument = new Argument<string>("destination", "The model destination");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for cp");

        command.AddArgument(sourceArgument);
        command.AddArgument(destinationArgument);
        command.AddOption(helpFlag);

        command.SetHandler(async (source, destination, help) =>
        {
            if (help)
            {
                return;
            }

            var handler = new CopyCommandHandler(source, destination, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, sourceArgument, destinationArgument, helpFlag);

        return command;
    }

    /// <summary>
    /// Creates the 'show' command for displaying information about a model.
    /// </summary>
    /// <returns></returns>
    private Command CreateShowCommand()
    {
        var command = new Command("show", "Show information for a model");
        var modelArgument = new Argument<string>("model", "The model to show");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for show");
        var licenseFlag = new Option<bool>("--license", "Show license of a model");
        var modelfileFlag = new Option<bool>("--modelfile", "Show Modelfile of a model");
        var parametersFlag = new Option<bool>("--parameters", "Show parameters of a model");
        var systemFlag = new Option<bool>("--system", "Show system message of a model");
        var templateFlag = new Option<bool>("--template", "Show template of a model");

        command.AddArgument(modelArgument);
        command.AddOption(helpFlag);
        command.AddOption(licenseFlag);
        command.AddOption(modelfileFlag);
        command.AddOption(parametersFlag);
        command.AddOption(systemFlag);
        command.AddOption(templateFlag);

        command.SetHandler(async (model, help, license, modelfile, parameters, system, template) =>
        {
            if (help)
            {
                return;
            }

            int optionCount = Convert.ToInt32(license) + Convert.ToInt32(modelfile) + Convert.ToInt32(parameters) + Convert.ToInt32(system) + Convert.ToInt32(template);
            if (optionCount > 1)
            {
                _consoleHelper.ShowError("Error: Only one of --license, --modelfile, --parameters, --system, or --template options is allowed.");
                return;
            }

            var fieldToShow = ShowCommandHandler.FieldToShow.All;
            switch (true)
            {
                case bool _ when license:
                    fieldToShow = ShowCommandHandler.FieldToShow.License;
                    break;
                case bool _ when modelfile:
                    fieldToShow = ShowCommandHandler.FieldToShow.Modelfile;
                    break;
                case bool _ when parameters:
                    fieldToShow = ShowCommandHandler.FieldToShow.Parameters;
                    break;
                case bool _ when system:
                    fieldToShow = ShowCommandHandler.FieldToShow.System;
                    break;
                case bool _ when template:
                    fieldToShow = ShowCommandHandler.FieldToShow.Template;
                    break;
            }

            var handler = new ShowCommandHandler(model, fieldToShow, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, modelArgument, helpFlag, licenseFlag, modelfileFlag, parametersFlag, systemFlag, templateFlag);

        return command;
    }

    /// <summary>
    /// Creates the 'ps' command for listing running models.
    /// </summary>
    /// <returns></returns>
    private Command CreatePsCommand()
    {
        var command = new Command("ps", "List running models");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for ps");

        command.AddOption(helpFlag);

        command.SetHandler(async (help) =>
        {
            if (help)
            {
                return;
            }

            var handler = new PsCommandHandler(_logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, helpFlag);

        return command;
    }

    /// <summary>
    /// Creates the 'list' command for listing models.
    /// </summary>
    /// <returns></returns>
    private Command CreateListCommand()
    {
        var command = new Command("list", "List models");
        command.AddAlias("ls");

        var helpFlag = new Option<bool>(["-h", "--help"], "help for list");

        command.AddOption(helpFlag);

        command.SetHandler(async (help) =>
        {
            if (help)
            {
                return;
            }

            var handler = new ListCommandHandler(_logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, helpFlag);

        return command;
    }

    /// <summary>
    /// Creates the 'stop' command for stopping a running model.
    /// </summary>
    /// <returns></returns>
    private Command CreateStopCommand()
    {
        var command = new Command("stop", "Stop a running model");
        var modelArgument = new Argument<string>("model", "The model to stop");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for stop");

        command.AddArgument(modelArgument);
        command.AddOption(helpFlag);

        command.SetHandler(async (model, help) =>
        {
            if (help)
            {
                return;
            }

            var handler = new StopCommandHandler(model, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, modelArgument, helpFlag);

        return command;
    }

    /// <summary>
    /// Creates the 'run' command for running a model.
    /// </summary>
    /// <returns></returns>
    private Command CreateRunCommand()
    {
        var command = new Command("run", "Run a model");
        var modelArgument = new Argument<string>("model", "The model to run");
        var promptArgument = new Argument<string>("prompt", "The prompt to use");
        var formatFlag = new Option<string>("--format", "Response format (e.g. json)");
        var helpFlag = new Option<bool>(["-h", "--help"], "help for run");
        var insecureFlag = new Option<bool>("--insecure", "Use an insecure registry");
        var keepAliveFlag = new Option<string>("--keepalive", "Duration to keep a model loaded (e.g. 5m)");
        var noWordWrapFlag = new Option<bool>("--nowordwrap", "Don't wrap words to the next line automatically");
        var verboseFlag = new Option<bool>("--verbose", "Show timings for response");

        command.AddArgument(modelArgument);
        command.AddArgument(promptArgument);
        command.AddOption(formatFlag);
        command.AddOption(helpFlag);
        command.AddOption(insecureFlag);
        command.AddOption(keepAliveFlag);
        command.AddOption(noWordWrapFlag);
        command.AddOption(verboseFlag);

        command.SetHandler(async (model, prompt, format, help, insecure, keepAlive, noWordWrap, verbose) =>
        {
            if (help)
            {
                return;
            }

            var handler = new RunCommandHandler(model, prompt, format, insecure, keepAlive, noWordWrap, verbose, _logger, _consoleHelper);
            await handler.ExecuteAsync().ConfigureAwait(false);
        }, modelArgument, promptArgument, formatFlag, helpFlag, insecureFlag, keepAliveFlag, noWordWrapFlag, verboseFlag);

        return command;
    }
}
