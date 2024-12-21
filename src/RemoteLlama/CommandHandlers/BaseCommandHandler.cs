namespace RemoteLlama.CommandHandlers;
using Microsoft.Extensions.Logging;

public abstract class BaseCommandHandler
{
    protected readonly ILogger _logger;

    protected BaseCommandHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        if (string.IsNullOrEmpty(ConfigManager.Url) && this is not SetUrlCommandHandler)
        {
            _logger.LogError("API URL has not been set. Use 'set-url' command first.");
            throw new InvalidOperationException("API URL has not been set. Use 'set-url' command first.");
        }

        await ExecuteImplAsync().ConfigureAwait(false);
    }

    protected abstract Task ExecuteImplAsync();
} 