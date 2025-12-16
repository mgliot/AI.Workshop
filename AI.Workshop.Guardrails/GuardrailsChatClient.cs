using Microsoft.Extensions.AI;

namespace AI.Workshop.Guardrails;

/// <summary>
/// A delegating chat client that applies guardrails to inputs and outputs
/// </summary>
public class GuardrailsChatClient : DelegatingChatClient
{
    private readonly GuardrailsService _guardrailsService;
    private readonly Action<GuardrailResult>? _onViolation;

    /// <summary>
    /// Creates a new guardrails chat client
    /// </summary>
    /// <param name="innerClient">The inner chat client to delegate to</param>
    /// <param name="options">Guardrails configuration options</param>
    /// <param name="onViolation">Optional callback when a violation is detected</param>
    public GuardrailsChatClient(
        IChatClient innerClient,
        GuardrailsOptions? options = null,
        Action<GuardrailResult>? onViolation = null)
        : base(innerClient)
    {
        _guardrailsService = new GuardrailsService(options);
        _onViolation = onViolation;
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Validate input messages
        var inputValidation = ValidateInputMessages(messages);
        if (!inputValidation.IsAllowed)
        {
            _onViolation?.Invoke(inputValidation);
            return CreateBlockedResponse(inputValidation);
        }

        // Get response from inner client
        var response = await base.GetResponseAsync(messages, options, cancellationToken);

        // Validate output
        var outputContent = GetResponseContent(response);
        var outputValidation = _guardrailsService.ValidateOutput(outputContent);

        if (!outputValidation.IsAllowed)
        {
            _onViolation?.Invoke(outputValidation);
            return CreateBlockedResponse(outputValidation);
        }

        // Apply redaction if needed
        if (outputValidation.RedactedContent != null)
        {
            return CreateRedactedResponse(response, outputValidation.RedactedContent);
        }

        return response;
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Validate input messages
        var inputValidation = ValidateInputMessages(messages);
        if (!inputValidation.IsAllowed)
        {
            _onViolation?.Invoke(inputValidation);
            yield return new ChatResponseUpdate(ChatRole.Assistant, _guardrailsService.Options.BlockedResponseMessage);
            yield break;
        }

        // Stream response and validate chunks
        var accumulatedContent = new System.Text.StringBuilder();

        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            var updateText = update.Text;
            if (!string.IsNullOrEmpty(updateText))
            {
                accumulatedContent.Append(updateText);

                // Periodic validation during streaming
                if (accumulatedContent.Length > 0 && accumulatedContent.Length % 500 == 0)
                {
                    var streamValidation = _guardrailsService.ValidateOutput(accumulatedContent.ToString());
                    if (!streamValidation.IsAllowed)
                    {
                        _onViolation?.Invoke(streamValidation);
                        yield return new ChatResponseUpdate(ChatRole.Assistant, $"\n\n[Content blocked: {streamValidation.ViolationType}]");
                        yield break;
                    }
                }
            }

            yield return update;
        }

        // Final validation
        var finalValidation = _guardrailsService.ValidateOutput(accumulatedContent.ToString());
        if (!finalValidation.IsAllowed)
        {
            _onViolation?.Invoke(finalValidation);
        }
    }

    private GuardrailResult ValidateInputMessages(IEnumerable<ChatMessage> messages)
    {
        foreach (var message in messages)
        {
            if (message.Role == ChatRole.User || message.Role == ChatRole.System)
            {
                var content = GetMessageContent(message);
                var result = _guardrailsService.ValidateInput(content);
                if (!result.IsAllowed)
                {
                    return result;
                }
            }
        }
        return GuardrailResult.Allowed();
    }

    private static string GetMessageContent(ChatMessage message)
    {
        if (message.Contents == null || message.Contents.Count == 0)
        {
            return message.Text ?? string.Empty;
        }

        return string.Join(" ", message.Contents
            .OfType<TextContent>()
            .Select(c => c.Text));
    }

    private static string GetResponseContent(ChatResponse response)
    {
        // Use the Text property which concatenates all message texts
        return response.Text;
    }

    private ChatResponse CreateBlockedResponse(GuardrailResult result)
    {
        return new ChatResponse(new ChatMessage(
            ChatRole.Assistant,
            _guardrailsService.Options.BlockedResponseMessage))
        {
            FinishReason = ChatFinishReason.ContentFilter
        };
    }

    private static ChatResponse CreateRedactedResponse(ChatResponse original, string redactedContent)
    {
        return new ChatResponse(new ChatMessage(
            ChatRole.Assistant,
            redactedContent))
        {
            FinishReason = original.FinishReason,
            ModelId = original.ModelId,
            CreatedAt = original.CreatedAt,
            Usage = original.Usage
        };
    }
}
