using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;

using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class RunCommandHandler : BaseCommandHandler
{
    private string _model;
    private readonly string _prompt;
    private readonly string _format;
    private readonly bool _insecure;
    private readonly string _keepAlive;
    private readonly bool _noWordWrap;
    private readonly bool _verbose;

    private readonly HttpClient _client;

    public RunCommandHandler(string model, string prompt, string format, bool insecure, string keepAlive, bool noWordWrap, bool verbose, ILogger logger, IConsoleHelper consoleHelper) : base(logger, consoleHelper)
    {
        _model = model;
        _prompt = prompt;
        _format = format;
        _insecure = insecure;
        _keepAlive = keepAlive;
        _noWordWrap = noWordWrap;
        _verbose = verbose;

        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    protected override async Task ExecuteImplAsync()
    {
        // First check for model redirects
        var redirected = ConfigManager.GetRedirectedModel(_model);
        if (redirected is not null && redirected != _model)
        {
            Logger.LogInformation("Model {Model} redirected to {Redirected}", _model, redirected);
            _model = redirected;
        }

        try
        {
            // There are 2 scenarios here:
            // 1. The prompt is given in the command line, so process that and end the command
            // 2. The prompt is not given in the command line, so start the prompt loop
            if (string.IsNullOrEmpty(_prompt))
            {
                await StartPromptLoopAsync().ConfigureAwait(false);
            }
            else
            {
                await ProcessPromptAsync(_prompt, _verbose).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to prompt model: {Model}", _model);
            ConsoleHelper.ShowError($"Failed to prompt model {_model}.");
        }
    }

    private async Task StartPromptLoopAsync()
    {
        // Start by prompting with an empty prompt to load the model
        await ProcessPromptAsync(string.Empty, false).ConfigureAwait(false);

        Logger.LogInformation("Starting chat with model: {Model}", _model);
        var messages = new List<ChatMessage>();

        while (true)
        {
            var prompt = ConsoleHelper.Prompt(">>> ");
            if (string.IsNullOrEmpty(prompt)) continue;

            // get the command
            var command = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();

            if (command.StartsWith('/'))
            {
                switch (command)
                {
                    case "/set":
                        // set session variables
                        break;
                    case "/show":
                        var showHandler = new ShowCommandHandler(_model, ShowCommandHandler.FieldToShow.All, Logger, ConsoleHelper);
                        await showHandler.ExecuteAsync().ConfigureAwait(false);
                        break;
                    case "/load":
                        // load a session or model
                        var modelToLoad = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);

                        if (string.IsNullOrEmpty(modelToLoad))
                        {
                            ConsoleHelper.ShowError("Please provide a model to load.");
                            continue;
                        }

                        // Get all the loaded models
                        var loadedModels = await BaseModelCommandHandler.LoadModelResponse(ConfigManager.Url + "tags", _client).ConfigureAwait(false);

                        // Check if the model exists
                        if (loadedModels is null || loadedModels.Models is null || !loadedModels.Models.Any(m => m.Name is not null && m.Name.StartsWith(modelToLoad, StringComparison.OrdinalIgnoreCase)))
                        {
                            ConsoleHelper.ShowError($"Error: Model '{modelToLoad}' not found.");
                            continue;
                        }

                        // Load the model
                        _model = modelToLoad;
                        await ProcessPromptAsync(string.Empty, false).ConfigureAwait(false);
                        break;
                    case "/save":
                        // save your current session as a new model
                        var modelFile = new StringBuilder();
                        modelFile.AppendLine($"FROM {_model}");

                        foreach (var message in messages)
                        {
                            modelFile.AppendLine($"MESSAGE {message.Role} \"{message.Content}\"");
                        }
                        break;
                    case "/clear":
                        // clear session context
                        messages.Clear();
                        break;
                    case "/bye":
                        return;
                    case "/?":
                    case "/help":
                        // help for a command
                        ShowHelp();
                        break;
                }
            }
            else
            {
                messages.Add(new ChatMessage("user", prompt));
                await ProcessChatAsync(messages).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessChatAsync(List<ChatMessage> messages)
    {
        // make a call to generate
        var url = ConfigManager.Url + "chat";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    model = _model,
                    messages,
                    keep_alive = _keepAlive,
                    format = _format,
                }),
                Encoding.UTF8,
                "application/json")
        };

        // Read the first token with a spinner
        using var reader = await ConsoleHelper.RunWithSpinner(async ctx =>
        {
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new StreamReader(stream);
        }).ConfigureAwait(false);

        ChatResponse? generateResponse = default;
        var messageResponseBuilder = new StringBuilder();

        // Read the rest of the stream
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(line)) continue;

            generateResponse = JsonSerializer.Deserialize<ChatResponse>(line);
            if (generateResponse is not null && !string.IsNullOrWhiteSpace(generateResponse.Message?.Content))
            {
                ConsoleHelper.WriteWord(generateResponse.Message.Content);
                messageResponseBuilder.Append(generateResponse.Message.Content);
            }
        }

        ConsoleHelper.WriteLine(string.Empty);
        messages.Add(new ChatMessage("model", messageResponseBuilder.ToString()));
    }

    private void ShowHelp()
    {
        ConsoleHelper.WriteLine("Available Commands:\n" +
                                "  /set            Set session variables\n" +
                                "  /show           Show model information\n" +
                                "  /load <model>   Load a session or model\n" +
                                "  /save <model>   Save your current session\n" +
                                "  /clear          Clear session context\n" +
                                "  /bye            Exit\n" +
                                "  /?, /help       Help for a command\n" +
                                "  /? shortcuts    Help for keyboard shortcuts\n" +
                                "\n" +
                                "Use \"\"\" to begin a multi-line message.\n");
    }

    private async Task ProcessPromptAsync(string prompt, bool verbose)
    {
        // make a call to generate
        var url = ConfigManager.Url + "generate";
        Logger.LogInformation("Prompting model: {Model} with \"{prompt}\"", _model, prompt);

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
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
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

        if (verbose && generateResponse is not null)
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
}
