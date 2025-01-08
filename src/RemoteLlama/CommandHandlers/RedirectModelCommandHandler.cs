using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class RedirectModelCommandHandler(string source, string destination, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _source = source;
    private readonly string _destination = destination;

    protected override Task ExecuteImplAsync()
    {
        Logger.LogInformation("Redirecting {source} to {destination}", _source, _destination);
        var modelRedirects = ConfigManager.ModelRedirects;

        // Remove any existing redirect for the source model
        modelRedirects.RemoveAll(redirect => redirect.Source == _source);

        // Add the new redirect
        modelRedirects.Add(new ConfigManager.ModelRedirect { Source = _source, Destination = _destination });

        // Update the configuration
        ConfigManager.ModelRedirects = modelRedirects;
        
        // Seeing as we redirected the model, let's pull it if needed
        var pullCommand = new PullCommandHandler(_destination, false, Logger, ConsoleHelper);
        return pullCommand.ExecuteAsync();
    }
}