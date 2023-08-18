using SqlComplexOperations.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace SqlComplexOperations.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureSqlComplexOperations(this IServiceCollection services)
        {
            if(services is null)
                throw new ArgumentException($"The service collection cannot be null", nameof(services));

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IMergeBuilder, MergeBuilder>();
        }
    }
}
