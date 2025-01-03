using System.Net;
using Microsoft.Extensions.Logging;

namespace RemoteLlama.Helpers;

public class ProxyServer(string targetServerUrl, ILogger logger)
{
    private readonly string _targetServerUrl = targetServerUrl;
    private readonly ILogger logger = logger;

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

        logger.LogInformation("Proxy server started on port {port}. Forwarding to {_targetServerUrl}", port, _targetServerUrl);

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = ProcessRequest(context, httpClient);
        }
    }

    private async Task ProcessRequest(HttpListenerContext context, HttpClient httpClient)
    {
        var request = context.Request;
        var response = context.Response;

        logger.LogInformation("Processing request: {HttpMethod} {RawUrl}", request.HttpMethod, request.RawUrl);

        var targetRoute = request.RawUrl?.ToLowerInvariant().TrimStart('/') ?? "";
        if (targetRoute.StartsWith("api/"))
        {
            targetRoute = targetRoute.Substring(4);
        }

        var targetUrl = $"{_targetServerUrl}{targetRoute}";

        logger.LogInformation("Calling target URL {targetUrl}", targetUrl);

        try
        {
            var proxyRequest = new HttpRequestMessage(new HttpMethod(request.HttpMethod), targetUrl);

            // Copy request headers
            foreach (var header in request.Headers.AllKeys)
            {
                proxyRequest.Headers.TryAddWithoutValidation(header!, request.Headers.GetValues(header)!);
            }

            // Copy request content
            if (request.ContentLength64 > 0)
            {
                using var stream = request.InputStream;
                var contentBytes = new byte[request.ContentLength64];
                var read = await stream.ReadAsync(contentBytes.AsMemory(0, contentBytes.Length));
                proxyRequest.Content = new ByteArrayContent(contentBytes);
                proxyRequest.Content.Headers.ContentType = request.ContentType != null ?
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse(request.ContentType) : null;
            }

            using var proxyResponse = await httpClient.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead);

            // Copy response status and headers
            response.StatusCode = (int)proxyResponse.StatusCode;
            foreach (var header in proxyResponse.Headers)
            {
                response.Headers.Add(header.Key, string.Join(",", header.Value));
            }

            // Send response content as a stream in chunks
            using var responseStream = await proxyResponse.Content.ReadAsStreamAsync();
            var buffer = new byte[4096]; 
            int bytesRead;
            while ((bytesRead = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
            {
                await response.OutputStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                await response.OutputStream.FlushAsync(); // Flush to send data immediately
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(ex.Message));
        }
        finally
        {
            response.Close();
        }
    }
}