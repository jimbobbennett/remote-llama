using Microsoft.Extensions.Logging;

using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

public class ServeCommandHandler(ILogger logger) : BaseCommandHandler(logger)
{
    protected override async Task ExecuteImplAsync()
    {
        var url = ConfigManager.Url;
        var proxy = new ProxyServer(url, _logger);

        await proxy.Start(11434);
    }
}