using Humanizer;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class ListCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseModelCommandHandler(logger, consoleHelper)
{
    private static readonly string[] Headers = ["NAME", "ID", "SIZE", "MODIFIED"];

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "tags";
            Logger.LogInformation("Running List");

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var modelResponse = await LoadModelResponse(url, client).ConfigureAwait(false);

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
                m.ModifiedAt.Humanize()
            }) ?? []);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run list");
            ConsoleHelper.ShowError("Failed to run list");
        }
    }
}
