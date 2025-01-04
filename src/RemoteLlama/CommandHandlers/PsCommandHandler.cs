using Humanizer;

using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class PsCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseModelCommandHandler(logger, consoleHelper)
{
    private static readonly string[] Headers = ["NAME", "ID", "SIZE", "UNTIL"];

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "ps";
            Logger.LogInformation("Running PS");

            var modelResponse = await LoadModelResponse(url).ConfigureAwait(false);

            if (modelResponse == null)
            {
                Logger.LogError("Failed to deserialize response");
                ConsoleHelper.ShowError("Failed to deserialize response");
                return;
            }

            ConsoleHelper.WriteTable(Headers, modelResponse.Models?.Select(m => new List<string>
            {
                m.Name ?? "",
                m.Digest?[..12] ?? "",
                m.Size.Bytes().Humanize("#.##"),
                m.ExpiresAt.Humanize()
            }) ?? []);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run ps");
            ConsoleHelper.ShowError("Failed to run ps");
        }
    }
}
