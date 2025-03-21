using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qdrant.Client;
using SICA.Tools.TextExtraction;
using SICA.Tools.TextExtraction.Implementations;
using SICA.Tools.VectorStore;

namespace SICA.Tools;

public static class Tools
{
    public static void AddTools(this IHostApplicationBuilder builder)
    {
        // TextExtraction
        builder.Services.AddOptions<TextExtractionSettings>().BindConfiguration(TextExtractionSettings.SectionName);
        builder.Services.AddTransient<ITextExtractionStrategy, TxtTextExtractionStrategy>();
        builder.Services.AddTransient<ITextExtractionStrategy, DocxTextExtractionStrategy>();
        builder.Services.AddTransient<ITextExtractionStrategy, PdfTextExtractionStrategy>();

        // VectorStore
        builder.Services.AddOptions<VectorStoreSettings>().BindConfiguration(VectorStoreSettings.SectionName);
        var vectorStoreSettings = builder.Configuration.GetSection(VectorStoreSettings.SectionName).Get<VectorStoreSettings>();
        ArgumentNullException.ThrowIfNull(vectorStoreSettings);
        builder.Services.AddTransient<IVectorStore, VectorStore.VectorStore>();
        builder.Services.AddEmbeddingGenerator(new OllamaEmbeddingGenerator(
            new Uri(vectorStoreSettings.EmbeddingModelUrl),
            modelId: vectorStoreSettings.EmbeddingModelId));
        builder.Services.AddTransient<QdrantClient>(
            _ => new QdrantClient(
                new Uri(vectorStoreSettings.QdrantUrl), 
                apiKey: vectorStoreSettings.QdrantApiKey));
    }
}