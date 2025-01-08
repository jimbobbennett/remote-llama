using System.Text;
using System.Text.Json;

using Humanizer;

using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class RunCommandHandler(string model, string prompt, string format, bool insecure, string keepAlive, bool noWordWrap, bool verbose, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private string _model = model;
    private readonly string _prompt = prompt;
    private readonly string _format = format;
    private readonly bool _insecure = insecure;
    private readonly string _keepAlive = keepAlive;
    private readonly bool _noWordWrap = noWordWrap;
    private readonly bool _verbose = verbose;

    protected override Task ExecuteImplAsync()
    {
        // First check for model redirects
        var redirected = ConfigManager.GetRedirectedModel(_model);
        if (redirected is not null && redirected != _model)
        {
            Logger.LogInformation("Model {Model} redirected to {Redirected}", _model, redirected);
            _model = redirected;
        }
        
        // There are 2 scenarios here:
        // 1. The prompt is given in the command line, so process that and end the command
        // 2. The prompt is not given in the command line, so start the prompt loop
        return string.IsNullOrEmpty(_prompt) ? StartPromptLoopAsync() : ProcessPromptAsync(_prompt);
    }

    private async Task StartPromptLoopAsync()
    {


    }

    private async Task ProcessPromptAsync(string prompt)
    {
        // make a call to generate
        try
        {
            var url = ConfigManager.Url + "generate";
            Logger.LogInformation("Prompting model: {Model} with \"{prompt}\"", _model, prompt);

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        model = _model,
                        prompt = _prompt,
                        keep_alive = _keepAlive,
                        format = _format,
                    }),
                    Encoding.UTF8,
                    "application/json")
            };

            // Read the first token with a spinner
            using var reader = await ConsoleHelper.RunWithSpinner(async ctx =>
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return new StreamReader(stream);
            }).ConfigureAwait(false);

            GenerateResponse? generateResponse = default;

            // Read the rest of the stream
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line)) continue;

                generateResponse = JsonSerializer.Deserialize<GenerateResponse>(line);
                if (generateResponse is not null && !string.IsNullOrWhiteSpace(generateResponse.Response))
                {
                    ConsoleHelper.WriteWord(generateResponse.Response);
                }
            }

            ConsoleHelper.WriteLine(string.Empty);

            if (_verbose && generateResponse is not null)
            {
                var rows = new List<List<string>>
                {
                    new() { "total duration:", TimeSpan.FromMilliseconds(generateResponse.TotalDuration / 1_000_000).Humanize() },
                    new() { "load duration:", TimeSpan.FromMilliseconds(generateResponse.LoadDuration / 1_000_000).Humanize() },
                    new() { "prompt eval count:", generateResponse.PromptEvalCount.ToString() },
                    new() { "prompt eval duration:", TimeSpan.FromMilliseconds(generateResponse.PromptEvalDuration / 1_000_000).Humanize() },
                    new() { "prompt eval rate:", (generateResponse.PromptEvalCount / (generateResponse.PromptEvalDuration / 1_000_000_000.0)).ToString("F2") + " tokens/s" },
                    new() { "eval count:", generateResponse.EvalCount.ToString() },
                    new() { "eval duration:", TimeSpan.FromMilliseconds(generateResponse.EvalDuration / 1_000_000).Humanize() },
                    new() { "eval rate:", (generateResponse.EvalCount / (generateResponse.EvalDuration / 1_000_000_000.0)).ToString("F2") + " tokens/s" },
                };

                ConsoleHelper.WriteColumns(rows);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to prompt model: {Model}", _model);
            ConsoleHelper.ShowError($"Failed to prompt model {_model}.");
        }
    }
}
