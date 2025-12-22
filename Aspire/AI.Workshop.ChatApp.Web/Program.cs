using AI.Workshop.ChatApp.Web.Components;
using AI.Workshop.ChatApp.Web.Services;
using AI.Workshop.Common;
using AI.Workshop.Guardrails;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using QdrantBased = AI.Workshop.VectorStore.Ingestion.Qdrant;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Register AI settings from configuration
builder.Services.Configure<AISettings>(builder.Configuration.GetSection(AISettings.SectionName));

// Register chat settings and guardrails services
builder.Services.AddScoped<ChatSettingsService>();
builder.Services.AddScoped<GuardrailsService>(sp => new GuardrailsService(new GuardrailsOptions
{
    BlockedPatterns = [@"\b(password|secret|api[_-]?key)\b"],
    MaxInputLength = 5000,
    MaxOutputLength = 10000,
    EnablePiiDetection = true
}));

builder.AddOllamaApiClient("chat")
    .AddChatClient()
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());

builder.AddOllamaApiClient("embeddings")
    .AddEmbeddingGenerator();

var vectorStoreSection = builder.Configuration.GetValue<string>("VECTOR_STORE");

if (vectorStoreSection == "Qdrant")
{
    builder.AddQdrantClient("vector-db");

    builder.Services.AddQdrantCollection<Guid, QdrantBased.IngestedChunk>("data-ai_workshop_chatapp-chunks");
    builder.Services.AddQdrantCollection<Guid, QdrantBased.IngestedDocument>("data-ai_workshop_chatapp-documents");

    builder.Services.AddScoped<QdrantBased.DataIngestor>();
    builder.Services.AddSingleton<QdrantBased.SemanticSearch>();
    builder.Services.AddScoped<RagService>(sp => new RagService(
        sp.GetRequiredService<QdrantBased.SemanticSearch>(),
        sp.GetRequiredService<IWebHostEnvironment>(),
        sp.GetRequiredService<ChatSettingsService>(),
        sp.GetRequiredService<ILogger<RagService>>()));
}
else if (vectorStoreSection == "Sqlite")
{
    var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
    var vectorStoreConnectionString = $"Data Source={vectorStorePath}";

    builder.Services.AddSqliteCollection<string, IngestedChunk>("data-ai_workshop_chatapp-chunks", vectorStoreConnectionString);
    builder.Services.AddSqliteCollection<string, IngestedDocument>("data-ai_workshop_chatapp-documents", vectorStoreConnectionString);

    builder.Services.AddScoped<DataIngestor>();
    builder.Services.AddSingleton<SemanticSearch>();
    builder.Services.AddScoped<RagService>(sp => new RagService(
        sp.GetRequiredService<SemanticSearch>(),
        sp.GetRequiredService<IWebHostEnvironment>(),
        sp.GetRequiredService<ChatSettingsService>(),
        sp.GetRequiredService<ILogger<RagService>>()));
}
else
{
    throw new InvalidOperationException("Please set the VECTOR_STORE configuration to either 'Qdrant' or 'Sqlite'.");
}

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using var scope = app.Services.CreateScope();

if (vectorStoreSection == "Qdrant")
{
    var ingestor = scope.ServiceProvider.GetRequiredService<QdrantBased.DataIngestor>();

    await ingestor.IngestDataAsync(
        new QdrantBased.PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));
}
else if (vectorStoreSection == "Sqlite")
{
    var ingestor = scope.ServiceProvider.GetRequiredService<DataIngestor>();

    await ingestor.IngestDataAsync(
        new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));
}
else
{
    throw new InvalidOperationException("Please set the VECTOR_STORE configuration to either 'Qdrant' or 'Sqlite'.");
}

app.Run();
