using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class CopyCommandHandler(string source, string destination, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _source = source;
    private readonly string _destination = destination;

    protected override Task ExecuteImplAsync()
    {
        throw new NotImplementedException();
    }
}