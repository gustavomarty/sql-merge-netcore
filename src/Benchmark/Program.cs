using Bulk.Extensions;
using Contracts.Service;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Hosting;
using Contracts.Service.Interfaces;
using Contracts.Data.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.ConfigureMergeBuilder();
                services.AddEntityFrameworkConfiguration(context.Configuration);
                services.AddScoped<IClubeService, ClubeService>();
                services.AddScoped<IFornecedorService, FornecedorService>();
                services.AddScoped<IMaterialService, MaterialService>();
                services.AddScoped<IContratoService, ContratoService>();
                services.AddScoped<ConfigureTestes>();
            })
            .Build();

        //await TesteUpsert(host);
        await TesteUpdateCompleto(host);

    }

    /// <summary>
    /// Teste de um update completo em todas as entidades (unitário vs upsert)
    /// </summary>
    static async Task TesteUpdateCompleto(IHost host)
    {
        //Configure Data
        var configurationService = host.Services.GetService<ConfigureTestes>();
        await configurationService!.CleanTables();
        await configurationService!.BuildPayloads(1000);
        await configurationService!.RunInsertBulk(1000);

        BenchmarkRunner.Run<TesteUpdateCompleto>();
    }

    /// <summary>
    /// Teste de um Upsert, com dados novos, alterados e imutáveis
    /// </summary>
    static async Task TesteUpsert(IHost host)
    {
        BenchmarkRunner.Run<TesteUpsertDadosNovosEUpdate>();
    }

    public static ServiceProvider GetServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureMergeBuilder();
        services.AddEntityFrameworkConfiguration(configuration);
        services.AddScoped<IClubeService, ClubeService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IContratoService, ContratoService>();
    }
}