using System.Management;
using System.Runtime.InteropServices;

var builder = DistributedApplication.CreateBuilder(args);

// Detect GPU vendor from environment or auto-detect from hardware
var gpuVendor = Environment.GetEnvironmentVariable("GPU_VENDOR")?.ToLowerInvariant() 
    ?? DetectGpuVendor();

// Log detected GPU for debugging
Console.WriteLine($"[Aspire] Detected GPU vendor: {gpuVendor ?? "none (will try NVIDIA)"}");

var ollama = builder.AddOllama("ollama")
    .WithDataVolume()
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent);

// Enable GPU acceleration based on available hardware and OS
// Windows Host + Docker: Only NVIDIA works reliably (via NVIDIA Container Toolkit)
// Windows Host + AMD/Intel: Vulkan not supported in Linux containers - falls back to CPU
// Linux Host: NVIDIA (CUDA), AMD (ROCm), Intel (device passthrough)
bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

switch (gpuVendor)
{
    case "amd":
        if (isWindows)
        {
            // Windows AMD with Docker: Vulkan passthrough to Linux containers is NOT supported
            // The container runs in WSL2/Linux VM which cannot access Windows Vulkan drivers
            // Option 1: Run Ollama natively on Windows (outside Docker) with OLLAMA_VULKAN=1
            // Option 2: Use CPU-only in container (current fallback)
            Console.WriteLine("[Aspire] WARNING: AMD GPU detected on Windows.");
            Console.WriteLine("[Aspire] Vulkan passthrough to Docker containers is not supported.");
            Console.WriteLine("[Aspire] Ollama will run on CPU. For GPU acceleration:");
            Console.WriteLine("[Aspire]   - Run Ollama natively: set OLLAMA_VULKAN=1 and run 'ollama serve'");
            Console.WriteLine("[Aspire]   - Or use WSL2 with GPU passthrough (requires additional setup)");
            // Don't set OLLAMA_VULKAN in container - it won't work
        }
        else
        {
            // Linux AMD: ROCm support - requires ROCm drivers on host
            ollama = ollama
                .WithImageTag("rocm")
                .WithContainerRuntimeArgs("--device", "/dev/kfd", "--device", "/dev/dri");
            Console.WriteLine("[Aspire] AMD GPU: Using ROCm image with device passthrough");
        }
        break;
    
    case "intel":
        if (isWindows)
        {
            // Windows Intel with Docker: Same limitation as AMD
            Console.WriteLine("[Aspire] WARNING: Intel GPU detected on Windows.");
            Console.WriteLine("[Aspire] Vulkan passthrough to Docker containers is not supported.");
            Console.WriteLine("[Aspire] Ollama will run on CPU. For GPU acceleration:");
            Console.WriteLine("[Aspire]   - Run Ollama natively: set OLLAMA_VULKAN=1 and run 'ollama serve'");
        }
        else
        {
            // Linux Intel: Mount /dev/dri for Intel integrated/discrete GPU access
            ollama = ollama.WithContainerRuntimeArgs("--device", "/dev/dri");
            Console.WriteLine("[Aspire] Intel GPU: Using device passthrough for /dev/dri");
        }
        break;
    
    case "nvidia":
        // NVIDIA GPU support - requires NVIDIA Container Toolkit
        // This works on both Windows (Docker Desktop with WSL2) and Linux
        ollama = ollama.WithContainerRuntimeArgs("--gpus", "all");
        Console.WriteLine("[Aspire] NVIDIA GPU: Using --gpus all");
        break;
    
    case "none":
    case "cpu":
        // CPU-only mode - no GPU acceleration
        Console.WriteLine("[Aspire] CPU-only mode: No GPU acceleration");
        break;
    
    default:
        // Auto-detect: try NVIDIA first (most common), fallback to CPU
        // Users can override with GPU_VENDOR environment variable
        Console.WriteLine("[Aspire] No GPU vendor specified, attempting NVIDIA...");
        try
        {
            ollama = ollama.WithContainerRuntimeArgs("--gpus", "all");
        }
        catch
        {
            Console.WriteLine("[Aspire] NVIDIA GPU not available, falling back to CPU");
        }
        break;
}

/// <summary>
/// Detects the GPU vendor on Windows using WMI.
/// Returns: "nvidia", "amd", "intel", or null if not detected.
/// </summary>
static string? DetectGpuVendor()
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return null;

    try
    {
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
        foreach (ManagementObject obj in searcher.Get())
        {
            var name = obj["Name"]?.ToString()?.ToLowerInvariant() ?? "";
            var adapterCompatibility = obj["AdapterCompatibility"]?.ToString()?.ToLowerInvariant() ?? "";
            
            // Check for NVIDIA first (highest priority for CUDA support)
            if (name.Contains("nvidia") || adapterCompatibility.Contains("nvidia"))
                return "nvidia";
        }
        
        // Second pass: check for AMD or Intel (will use Vulkan)
        foreach (ManagementObject obj in searcher.Get())
        {
            var name = obj["Name"]?.ToString()?.ToLowerInvariant() ?? "";
            var adapterCompatibility = obj["AdapterCompatibility"]?.ToString()?.ToLowerInvariant() ?? "";
            
            if (name.Contains("amd") || name.Contains("radeon") || 
                adapterCompatibility.Contains("amd") || adapterCompatibility.Contains("advanced micro"))
                return "amd";
            
            if (name.Contains("intel") || adapterCompatibility.Contains("intel"))
                return "intel";
        }
    }
    catch
    {
        // WMI query failed, return null for fallback behavior
    }

    return null;
}

var chat = ollama.AddModel("chat", "llama3.2");
var embeddings = ollama.AddModel("embeddings", "all-minilm");

var qdrant = builder.AddQdrant("vector-db")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.AI_Workshop_ChatApp_Web>("aichatweb-app")
    .WithEnvironment("VECTOR_STORE", "Qdrant")
    .WithReference(chat)
    .WithReference(embeddings)
    .WaitFor(chat)
    .WaitFor(embeddings)
    .WithReference(qdrant)
    .WaitFor(qdrant);

builder.Build().Run();
