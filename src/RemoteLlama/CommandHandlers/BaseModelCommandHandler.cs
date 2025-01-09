using System.Text.Json;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal abstract class BaseModelCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    public static async Task<ModelResponse?> LoadModelResponse(string url, HttpClient client)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        // Optimization: Deserialize directly from the response stream
        return await JsonSerializer.DeserializeAsync<ModelResponse>(await response.Content.ReadAsStreamAsync()).ConfigureAwait(false);
    }

}
