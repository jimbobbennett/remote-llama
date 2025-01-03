using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class CreateCommandHandler(string modelId, string file, string quantize, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _modelId = modelId;
    private readonly string _file = file;
    private readonly string _quantize = quantize;

    protected override Task ExecuteImplAsync()
    {
        throw new NotImplementedException();
    }
}