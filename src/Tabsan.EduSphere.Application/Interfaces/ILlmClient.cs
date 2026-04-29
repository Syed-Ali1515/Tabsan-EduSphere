namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for a provider-agnostic LLM HTTP client.
/// The implementation uses the OpenAI-compatible chat completions API format,
/// which works with Azure OpenAI, OpenAI, Ollama, and other compatible providers.
/// Provider base URL and API key are resolved from configuration.
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Sends a list of messages to the language model and returns the assistant's reply text.
    /// </summary>
    /// <param name="systemPrompt">System-level instruction injected as the first message.</param>
    /// <param name="messages">Conversation history in (role, content) pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The assistant reply text, or an empty string on failure.</returns>
    Task<(string Reply, int TokensUsed)> SendAsync(
        string                                     systemPrompt,
        IEnumerable<(string Role, string Content)> messages,
        CancellationToken                          ct = default);
}
