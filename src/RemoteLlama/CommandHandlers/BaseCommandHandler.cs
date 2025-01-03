namespace RemoteLlama.CommandHandlers;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

internal abstract class BaseCommandHandler(ILogger logger, IConsoleHelper consoleHelper)
{
    protected ILogger Logger { get; } = logger;
    protected IConsoleHelper ConsoleHelper { get; } = consoleHelper;

    public async Task ExecuteAsync()
    {
        if (string.IsNullOrEmpty(ConfigManager.Url) && this is not SetUrlCommandHandler)
        {
            Logger.LogError("API URL has not been set. Use 'set-url' command first.");
            throw new InvalidOperationException("API URL has not been set. Use 'set-url' command first.");
        }

        await ExecuteImplAsync().ConfigureAwait(false);
    }

    protected abstract Task ExecuteImplAsync();
} 