using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.AiChat;

/// <summary>
/// OpenAI-compatible chat completions HTTP client.
/// Works with OpenAI, Azure OpenAI, and Ollama by changing
/// <c>AiChat:BaseUrl</c> and <c>AiChat:ApiKey</c> in configuration.
/// </summary>
public sealed class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly string     _model;
    private readonly ILogger<OpenAiLlmClient> _logger;

    /// <summary>Initialises the client from configuration.</summary>
    public OpenAiLlmClient(HttpClient http, IConfiguration config, ILogger<OpenAiLlmClient> logger)
    {
        _http   = http;
        _model  = config["AiChat:Model"] ?? "gpt-3.5-turbo";
        _logger = logger;
    }

    /// <summary>
    /// Posts the conversation to the /v1/chat/completions endpoint and returns the reply text.
    /// Returns an empty string and logs an error on any HTTP or parsing failure.
    /// </summary>
    public async Task<(string Reply, int TokensUsed)> SendAsync(
        string                                     systemPrompt,
        IEnumerable<(string Role, string Content)> messages,
        CancellationToken                          ct = default)
    {
        var payload = new
        {
            model    = _model,
            messages = BuildMessages(systemPrompt, messages)
        };

        try
        {
            using var response = await _http.PostAsJsonAsync("v1/chat/completions", payload, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
                             cancellationToken: ct);

            var reply  = result?.Choices?[0]?.Message?.Content ?? string.Empty;
            var tokens = result?.Usage?.TotalTokens ?? 0;
            return (reply, tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM request failed for model {Model}", _model);
            return (string.Empty, 0);
        }
    }

    private static List<object> BuildMessages(
        string systemPrompt,
        IEnumerable<(string Role, string Content)> messages)
    {
        var list = new List<object> { new { role = "system", content = systemPrompt } };
        foreach (var (role, content) in messages)
            list.Add(new { role, content });
        return list;
    }

    // ── Private response models ───────────────────────────────────────────────

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
        [JsonPropertyName("usage")]   public Usage?        Usage   { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")] public MessageContent? Message { get; set; }
    }

    private sealed class MessageContent
    {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }

    private sealed class Usage
    {
        [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
    }
}
