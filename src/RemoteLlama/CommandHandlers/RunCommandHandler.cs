using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class RunCommandHandler(string model, string prompt, string format, bool insecure, string keepAlive, bool noWordWrap, bool verbose, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _model = model;
    private readonly string _prompt = prompt;
    private readonly string _format = format;
    private readonly bool _insecure = insecure;
    private readonly string _keepAlive = keepAlive;
    private readonly bool _noWordWrap = noWordWrap;
    private readonly bool _verbose = verbose;

    protected override Task ExecuteImplAsync()
    {
        throw new NotImplementedException();
    }
}
