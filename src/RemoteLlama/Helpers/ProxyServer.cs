using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RemoteLlama.Helpers;

internal class ProxyServer(string targetServerUrl, ILogger logger)
{
    private readonly string _targetServerUrl = targetServerUrl;
    private readonly ILogger _logger = logger;

    public async Task Start(int port = 11434)
    {
        using var httpClientHandler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };

        using var httpClient = new HttpClient(httpClientHandler);

        var listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{port}/");
        listener.Start();

        _logger.LogInformation("Proxy server started on port {port}. Forwarding to {_targetServerUrl}", port, _targetServerUrl);

        while (true)
        {
            var context = await listener.GetContextAsync().ConfigureAwait(false);
            _ = ProcessRequest(context, httpClient);
        }
    }

    private async Task ProcessRequest(HttpListenerContext context, HttpClient httpClient)
    {
        var request = context.Request;
        var response = context.Response;

        _logger.LogInformation("Processing request: {HttpMethod} {RawUrl}", request.HttpMethod, request.RawUrl);

        var targetRoute = request.RawUrl?.TrimStart('/') ?? "";
        if (targetRoute.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            targetRoute = targetRoute[4..];
        }

        var targetUrl = $"{_targetServerUrl}{targetRoute}";

        _logger.LogInformation("Calling target URL {targetUrl}", targetUrl);

        try
        {
            var proxyRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), targetUrl);

            // Copy request headers
            foreach (var header in request.Headers.AllKeys)
            {
                proxyRequest.Headers.TryAddWithoutValidation(header!, request.Headers.GetValues(header)!);
            }

            await CopyBody(request, proxyRequest).ConfigureAwait(false);

            using var proxyResponse = await httpClient.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            // Copy response status and headers
            response.StatusCode = (int)proxyResponse.StatusCode;
            foreach (var header in proxyResponse.Headers)
            {
                response.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            // Send response content as a stream in chunks
            using var responseStream = await proxyResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
            {
                await response.OutputStream.WriteAsync(buffer.AsMemory(0, bytesRead)).ConfigureAwait(false);
                await response.OutputStream.FlushAsync().ConfigureAwait(false); // Flush to send data immediately
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(ex.Message)).ConfigureAwait(false);
        }
        finally
        {
            response.Close();
        }
    }

    /// <summary>
    /// Copies the body of the request to the proxy request.
    /// 
    /// If ths request is to the /generate endpoint, then we may need to intercept the body and change the model
    /// name depending on the redirect models configuration.
    /// </summary>
    /// <param name="request">The original request</param>
    /// <param name="proxyRequest">The request that will be sent to the remote Ollama endpoint</param>
    private async Task CopyBody(HttpListenerRequest request, HttpRequestMessage proxyRequest)
    {
        // Copy request content
        if (request.ContentLength64 > 0)
        {
            using var stream = request.InputStream;
            var contentBytes = new byte[request.ContentLength64];
            await stream.ReadAsync(contentBytes.AsMemory(0, contentBytes.Length)).ConfigureAwait(false);
            proxyRequest.Content = new ByteArrayContent(contentBytes);

            if (request.RawUrl is not null)
            {
                // if the request is to the /generate or /chat endpoint, we may need to change the model name
                if (request.RawUrl.TrimEnd('/').EndsWith("chat", StringComparison.OrdinalIgnoreCase) ||
                    request.RawUrl.TrimEnd('/').EndsWith("generate", StringComparison.OrdinalIgnoreCase))
                {
                    // read the input stream and deserialize the JSON into a dynamic object
                    var content = System.Text.Encoding.UTF8.GetString(contentBytes);
                    var jsonDocument = JsonSerializer.Deserialize<JsonDocument>(content);

                    _logger.LogDebug("Processing request on {url} with body:\n{body}", request.RawUrl, content);

                    if (jsonDocument != null)
                    {
                        var root = jsonDocument.RootElement;

                        // Convert JsonElement to a mutable dictionary
                        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, object>>(root.GetRawText());

                        if (jsonObject != null)
                        {
                            // Update a value in the JSON object
                            if (jsonObject.TryGetValue("model", out var modelValue))
                            {
                                var model = modelValue?.ToString();
                                if (!string.IsNullOrEmpty(model))
                                {
                                    var redirectedModel = ConfigManager.GetRedirectedModel(model);
                                    jsonObject["model"] = redirectedModel;

                                    var updatedJsonBytes = JsonSerializer.SerializeToUtf8Bytes(jsonObject);
                                    proxyRequest.Content = new ByteArrayContent(updatedJsonBytes);
                                }
                            }
                        }
                    }
                }
            }

            proxyRequest.Content.Headers.ContentType = request.ContentType != null ?
                System.Net.Http.Headers.MediaTypeHeaderValue.Parse(request.ContentType) : null;
        }
    }
}