var builder = DistributedApplication.CreateBuilder(args);

// Detect GPU vendor from environment or default to auto-detection
var gpuVendor = Environment.GetEnvironmentVariable("GPU_VENDOR")?.ToLowerInvariant();

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

// Enable GPU acceleration based on available hardware
// Supports: NVIDIA (default), AMD (ROCm), Intel (experimental)
switch (gpuVendor)
{
    case "amd":
        // AMD ROCm support - requires ROCm drivers on host
        // Mount both /dev/kfd and /dev/dri for full ROCm support
        ollama = ollama
            .WithImageTag("rocm")
            .WithContainerRuntimeArgs("--device", "/dev/kfd", "--device", "/dev/dri");
        break;
    
    case "intel":
        // Intel GPU support (experimental) - requires Intel compute runtime
        // Mount /dev/dri for Intel integrated/discrete GPU access
        ollama = ollama
            .WithContainerRuntimeArgs("--device", "/dev/dri");
        break;
    
    case "nvidia":
        // NVIDIA GPU support - requires NVIDIA Container Toolkit
        ollama = ollama.WithContainerRuntimeArgs("--gpus", "all");
        break;
    
    case "none":
    case "cpu":
        // CPU-only mode - no GPU acceleration
        break;
    
    default:
        // Auto-detect: try NVIDIA first (most common), fallback to CPU
        // Users can override with GPU_VENDOR environment variable
        try
        {
            ollama = ollama.WithContainerRuntimeArgs("--gpus", "all");
        }
        catch
        {
            // GPU not available, continue with CPU
        }
        break;
}

var chat = ollama.AddModel("chat", "llama3.2");
var embeddings = ollama.AddModel("embeddings", "all-minilm");

var qdrant = builder.AddQdrant("vector-db")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.AI_Workshop_ChatApp_Web>("aichatweb-app")
    .WithReference(chat)
    .WithReference(embeddings)
    .WaitFor(chat)
    .WaitFor(embeddings)
    .WithReference(qdrant)
    .WaitFor(qdrant);

builder.Build().Run();
