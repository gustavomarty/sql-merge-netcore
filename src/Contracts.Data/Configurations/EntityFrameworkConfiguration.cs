using Contracts.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Data.Configurations
{
    public static class EntityFrameworkConfiguration
    {
        public static void AddEntityFrameworkConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection");

            services.AddDbContext<ApplicationContext>(options => {
                options.UseSqlServer(connectionString).UseSnakeCaseNamingConvention();
            }, ServiceLifetime.Scoped);
        }
    }
}
