using SqlComplexOperations.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Extensions
{
    /// <summary>
    /// Classe para facilitar a injeção de dependencia.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Injetar em casos de uso de SQL Server.
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ConfigureSqlComplexOperations(this IServiceCollection services)
        {
            if(services is null)
                throw new ArgumentException($"The service collection cannot be null", nameof(services));

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IMergeBuilder, MergeBuilder>(provider =>
            {
                var databaseService = provider.GetService<IDatabaseService>();
                var instance = new MergeBuilder(databaseService!, DatabaseType.MICROSOFT_SQL_SERVER);
                return instance;
            });

            services.AddScoped<IBulkInsertBuilder, BulkInsertBuilder>(provider =>
            {
                var databaseService = provider.GetService<IDatabaseService>();
                var instance = new BulkInsertBuilder(databaseService!, DatabaseType.MICROSOFT_SQL_SERVER);
                return instance;
            });
        }

        /// <summary>
        /// Injetar em casos de uso de PostgreSql.
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void ConfigurePostgreSqlComplexOperations(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentException($"The service collection cannot be null", nameof(services));

            services.AddScoped<IDatabaseService, DatabaseService>();
            services.AddScoped<IMergeBuilder, MergeBuilder>(provider =>
            {
                var databaseService = provider.GetService<IDatabaseService>();
                var instance = new MergeBuilder(databaseService!, DatabaseType.POSTGRES_SQL);
                return instance;
            });

            services.AddScoped<IBulkInsertBuilder, BulkInsertBuilder>(provider =>
            {
                var databaseService = provider.GetService<IDatabaseService>();
                var instance = new BulkInsertBuilder(databaseService!, DatabaseType.POSTGRES_SQL);
                return instance;
            });
        }
    }
}
