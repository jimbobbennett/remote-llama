using Microsoft.Extensions.Logging;

using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

/// <summary>
/// Handles the command to set the remote URL for the Llama configuration.
/// This command updates the URL in the ConfigManager.
/// </summary>
/// <remarks>
/// Initializes a new instance of the SetUrlCommandHandler class
/// </remarks>
/// <param name="url">The URL to be set in the configuration</param>
/// <param name="logger">The logger instance for logging operations</param>
internal class SetUrlCommandHandler(string url, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    /// <summary>
    /// The URL to be set in the configuration
    /// </summary>
    private string _url = url;

    /// <summary>
    /// Executes the command to set the URL in the configuration
    /// </summary>
    /// <returns>A Task representing the asynchronous operation</returns>
    protected override async Task ExecuteImplAsync()
    {
        // Format the URL
        // If it is missing https:// or http://, add it
        // Make sure it ends with api/
        if (!_url.StartsWith("https://") && !_url.StartsWith("http://"))
        {
            _url = "https://" + _url;
        }

        if (_url.EndsWith("/api"))
        {
            _url += "/";
        }
        
        if (!_url.EndsWith("/api/"))
        {
            _url = _url.TrimEnd('/');
            _url += "/api/";
        }

        // Make sure it is a valid URL
        if (!Uri.IsWellFormedUriString(_url, UriKind.Absolute))
        {
            throw new ArgumentException("Invalid URL");
        }

        Logger.LogInformation("Setting URL to: {Url}", _url);
        ConfigManager.Url = _url;
        await Task.CompletedTask.ConfigureAwait(false);
    }
} 