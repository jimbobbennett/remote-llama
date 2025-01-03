using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class ShowCommandHandler(string model, bool license, bool modelfile, bool parameters, bool system, bool template, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _model = model;
    private readonly bool _license = license;
    private readonly bool _modelfile = modelfile;
    private readonly bool _parameters = parameters;
    private readonly bool _system = system;
    private readonly bool _template = template;

    protected override Task ExecuteImplAsync()
    {
        throw new NotImplementedException();
    }
}