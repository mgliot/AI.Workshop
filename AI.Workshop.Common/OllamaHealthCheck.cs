using System.Net.Http;

namespace AI.Workshop.Common;

/// <summary>
/// Provides health check functionality for Ollama service
/// </summary>
public static class OllamaHealthCheck
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    /// <summary>
    /// Result of an Ollama health check
    /// </summary>
    public record HealthCheckResult(bool IsHealthy, string Message, string? Version = null);

    /// <summary>
    /// Checks if Ollama is running and accessible at the specified URI
    /// </summary>
    /// <param name="ollamaUri">The Ollama server URI</param>
    /// <returns>Health check result</returns>
    public static async Task<HealthCheckResult> CheckAsync(string ollamaUri)
    {
        return await CheckAsync(new Uri(ollamaUri));
    }

    /// <summary>
    /// Checks if Ollama is running and accessible at the specified URI
    /// </summary>
    /// <param name="ollamaUri">The Ollama server URI</param>
    /// <returns>Health check result</returns>
    public static async Task<HealthCheckResult> CheckAsync(Uri ollamaUri)
    {
        try
        {
            var response = await _httpClient.GetAsync(ollamaUri);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Ollama returns "Ollama is running" on the root endpoint
                if (content.Contains("Ollama", StringComparison.OrdinalIgnoreCase))
                {
                    return new HealthCheckResult(true, "Ollama is running", content.Trim());
                }
                return new HealthCheckResult(true, "Service is responding");
            }

            return new HealthCheckResult(false, $"Ollama returned status {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return new HealthCheckResult(false, $"Cannot connect to Ollama at {ollamaUri}: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return new HealthCheckResult(false, $"Connection to Ollama at {ollamaUri} timed out");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(false, $"Error checking Ollama: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if Ollama is running using default URI
    /// </summary>
    /// <returns>Health check result</returns>
    public static Task<HealthCheckResult> CheckAsync()
    {
        return CheckAsync(AIConstants.DefaultOllamaUri);
    }

    /// <summary>
    /// Waits for Ollama to become available, with retry logic
    /// </summary>
    /// <param name="ollamaUri">The Ollama server URI</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="delayBetweenRetries">Delay between retries</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result after all retries</returns>
    public static async Task<HealthCheckResult> WaitForOllamaAsync(
        string ollamaUri,
        int maxRetries = 10,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
    {
        var delay = delayBetweenRetries ?? TimeSpan.FromSeconds(2);
        HealthCheckResult? lastResult = null;

        for (int i = 0; i < maxRetries; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new HealthCheckResult(false, "Health check cancelled");
            }

            lastResult = await CheckAsync(ollamaUri);
            
            if (lastResult.IsHealthy)
            {
                return lastResult;
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        return lastResult ?? new HealthCheckResult(false, "No health check performed");
    }

    /// <summary>
    /// Waits for Ollama to become available using default URI
    /// </summary>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <param name="delayBetweenRetries">Delay between retries</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health check result after all retries</returns>
    public static Task<HealthCheckResult> WaitForOllamaAsync(
        int maxRetries = 10,
        TimeSpan? delayBetweenRetries = null,
        CancellationToken cancellationToken = default)
    {
        return WaitForOllamaAsync(AIConstants.DefaultOllamaUri, maxRetries, delayBetweenRetries, cancellationToken);
    }

    /// <summary>
    /// Displays a console-friendly startup check with spinner
    /// </summary>
    /// <param name="ollamaUri">The Ollama server URI</param>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <returns>True if Ollama is available, false otherwise</returns>
    public static async Task<bool> EnsureOllamaAvailableAsync(
        string? ollamaUri = null,
        int maxRetries = 5)
    {
        var uri = ollamaUri ?? AIConstants.DefaultOllamaUri;
        var spinnerChars = new[] { '|', '/', '-', '\\' };
        var spinnerIndex = 0;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Checking Ollama connection at {uri} ");

        for (int i = 0; i < maxRetries; i++)
        {
            var result = await CheckAsync(uri);
            
            if (result.IsHealthy)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\r✓ Ollama is running                              ");
                Console.ResetColor();
                return true;
            }

            // Show spinner
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\rConnecting to Ollama {spinnerChars[spinnerIndex++ % spinnerChars.Length]} (attempt {i + 1}/{maxRetries})");
            
            if (i < maxRetries - 1)
            {
                await Task.Delay(2000);
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\r✗ Cannot connect to Ollama at {uri}                    ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nPlease ensure Ollama is running:");
        Console.WriteLine("  1. Install Ollama from https://ollama.ai");
        Console.WriteLine("  2. Run 'ollama serve' in a terminal");
        Console.WriteLine("  3. Or start Ollama from your applications menu");
        Console.ResetColor();
        
        return false;
    }
}
