using AI.Workshop.ChatApp.Web.Components;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Octokit;
using QdrantBased = AI.Workshop.VectorStore.Ingestion.Qdrant;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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
}
else if (vectorStoreSection == "Sqlite")
{
    var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
    var vectorStoreConnectionString = $"Data Source={vectorStorePath}";

    builder.Services.AddSqliteCollection<string, IngestedChunk>("data-ai_workshop_chatapp-chunks", vectorStoreConnectionString);
    builder.Services.AddSqliteCollection<string, IngestedDocument>("data-ai_workshop_chatapp-documents", vectorStoreConnectionString);

    builder.Services.AddScoped<DataIngestor>();
    builder.Services.AddSingleton<SemanticSearch>();
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

var gitHubKey = builder.Configuration["GITHUB_APIKEY"];
GitHubClient? gitHubClient = null;
if (!string.IsNullOrEmpty(gitHubKey))
{
    gitHubClient = new GitHubClient(new ProductHeaderValue("AI-Workshop.ChatApp.Web"))
    {
        Credentials = new Credentials(gitHubKey)
    };
}

//await DataIngestor.IngestDataAsync(
//    app.Services,
//    new GitHubMarkdownSource(gitHubClient, "dedalusmax", "dice-and-roll2", "/"));

app.Run();
