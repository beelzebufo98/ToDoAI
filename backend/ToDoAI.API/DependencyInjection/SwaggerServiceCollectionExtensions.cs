using Microsoft.OpenApi;

namespace ToDoAI.API.DependencyInjection;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDoAI Service API", Version = "v1.0" });
            c.SwaggerDoc("internal", new OpenApiInfo { Title = "ToDoAI Internal API", Version = "v1.0" });
            c.DocInclusionPredicate((docName, apiDesc) =>
            {
                var relativePath = apiDesc.RelativePath ?? string.Empty;
                var controllerName = apiDesc.ActionDescriptor.RouteValues.TryGetValue("controller", out var value)
                    ? value
                    : string.Empty;
                var isInternal = relativePath.Contains("/dev/", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(controllerName, "DevEmail", StringComparison.OrdinalIgnoreCase);

                return docName switch
                {
                    "internal" => isInternal,
                    "v1" => !isInternal,
                    _ => false
                };
            });
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });

        return services;
    }
}
