using Microsoft.OpenApi.Models;

namespace ContractsApi.Configurations
{
    public static class SwaggerConfiguration
    {
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(s => {

                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Contracts API",
                    Description = "API de contrato de jogadores"
                });
            });
        }

        public static void UseSwaggerSetup(this IApplicationBuilder application)
        {
            application.UseSwagger();
            application.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contracts API");
            });
        }
    }
}
