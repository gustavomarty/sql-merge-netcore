using Bulk.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bulk.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureMergeBuilder(this IServiceCollection services)
        {
            if(services is null)
                throw new ArgumentException($"The service collection cannot be null", nameof(services));

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IMergeBuilder, MergeBuilder>();
        }
    }
}
