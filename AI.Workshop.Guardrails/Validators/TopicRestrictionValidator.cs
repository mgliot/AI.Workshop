using Microsoft.Extensions.AI;

namespace AI.Workshop.Guardrails.Validators;

/// <summary>
/// Validates that content stays within allowed topics using semantic similarity.
/// Uses embeddings to compare user input against allowed topic descriptions.
/// </summary>
public class TopicRestrictionValidator : IContentValidator
{
    private readonly IEmbeddingGenerator<string, Embedding<float>>? _embeddingGenerator;
    private Dictionary<string, ReadOnlyMemory<float>>? _topicEmbeddings;

    /// <summary>
    /// Creates a topic restriction validator without embedding support (uses keyword matching)
    /// </summary>
    public TopicRestrictionValidator()
    {
    }

    /// <summary>
    /// Creates a topic restriction validator with semantic similarity support
    /// </summary>
    /// <param name="embeddingGenerator">Embedding generator for semantic matching</param>
    public TopicRestrictionValidator(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator;
    }

    /// <summary>
    /// Priority order - runs after basic checks but before expensive LLM calls
    /// </summary>
    public int Priority => 35;

    /// <summary>
    /// Validates that content is within allowed topics
    /// </summary>
    public GuardrailResult Validate(string content, GuardrailsOptions options)
    {
        if (!options.EnableTopicRestriction || options.AllowedTopics.Count == 0)
        {
            return GuardrailResult.Allowed();
        }

        // Use semantic similarity if embedding generator is available
        if (_embeddingGenerator != null)
        {
            return ValidateSemanticAsync(content, options).GetAwaiter().GetResult();
        }

        // Fall back to keyword-based matching
        return ValidateKeywordBased(content, options);
    }

    private async Task<GuardrailResult> ValidateSemanticAsync(string content, GuardrailsOptions options)
    {
        // Initialize topic embeddings if not done
        if (_topicEmbeddings == null || _topicEmbeddings.Count != options.AllowedTopics.Count)
        {
            await InitializeTopicEmbeddingsAsync(options.AllowedTopics);
        }

        // Generate embedding for the input content
        var contentVector = await _embeddingGenerator!.GenerateVectorAsync(content);

        // Check similarity against each topic
        var maxSimilarity = 0.0;
        string? bestMatchTopic = null;

        foreach (var (topic, topicVector) in _topicEmbeddings!)
        {
            var similarity = CosineSimilarity(contentVector.Span, topicVector.Span);
            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                bestMatchTopic = topic;
            }
        }

        // Check if similarity meets threshold
        if (maxSimilarity >= options.TopicSimilarityThreshold)
        {
            return GuardrailResult.Allowed();
        }

        return GuardrailResult.Blocked(
            GuardrailViolationType.OffTopic,
            $"Content does not match allowed topics. Best match: '{bestMatchTopic}' with {maxSimilarity:P0} similarity (threshold: {options.TopicSimilarityThreshold:P0})",
            matchedPattern: bestMatchTopic);
    }

    private async Task InitializeTopicEmbeddingsAsync(List<string> topics)
    {
        _topicEmbeddings = new Dictionary<string, ReadOnlyMemory<float>>();

        foreach (var topic in topics)
        {
            var embedding = await _embeddingGenerator!.GenerateVectorAsync(topic);
            _topicEmbeddings[topic] = embedding;
        }
    }

    private static GuardrailResult ValidateKeywordBased(string content, GuardrailsOptions options)
    {
        var contentLower = content.ToLowerInvariant();

        foreach (var topic in options.AllowedTopics)
        {
            // Check if any topic keyword appears in the content
            var topicKeywords = topic.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (topicKeywords.Any(keyword => contentLower.Contains(keyword)))
            {
                return GuardrailResult.Allowed();
            }
        }

        return GuardrailResult.Blocked(
            GuardrailViolationType.OffTopic,
            $"Content does not match any allowed topics: {string.Join(", ", options.AllowedTopics)}");
    }

    private static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
            return 0;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator == 0 ? 0 : dotProduct / denominator;
    }
}
