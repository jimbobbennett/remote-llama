using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class ListCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    protected override Task ExecuteImplAsync()
    {
        throw new NotImplementedException();
    }
}