using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

public static class MediaStorageServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredMediaStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MediaStorageOptions>(configuration.GetSection(MediaStorageOptions.SectionName));

        var options = configuration.GetSection(MediaStorageOptions.SectionName).Get<MediaStorageOptions>() ?? new MediaStorageOptions();
        var provider = options.Provider?.Trim();

        // Final-Touches Phase 28 Stage 28.3 — configurable storage-provider selection.
        if (string.Equals(provider, "Blob", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IMediaStorageService, BlobMediaStorageService>();
        }
        else
        {
            services.AddScoped<IMediaStorageService, LocalMediaStorageService>();
        }

        return services;
    }
}
