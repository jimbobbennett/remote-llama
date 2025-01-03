using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class StopCommandHandler(string model, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _model = model;

    protected override async Task ExecuteImplAsync()
    {
        throw new NotImplementedException();
    }
}