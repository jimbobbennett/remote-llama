using Microsoft.Extensions.Logging;

using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class ServeCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    protected override async Task ExecuteImplAsync()
    {
        var url = ConfigManager.Url;
        var proxy = new ProxyServer(url, Logger);

        await proxy.Start(11434);
    }
}