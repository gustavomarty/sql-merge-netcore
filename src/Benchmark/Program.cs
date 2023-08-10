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


        //var _fornecedorService = host.Services.GetService<IFornecedorService>();

        ////Configura o banco com os 100 primeiros fornecedores
        ////await _fornecedorService.CleanTable();
        ////await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000));

        //var fornecedoresMix = await _fornecedorService!.GetMix(1000, true, false);

        ////Roda upsert
        //await _fornecedorService.Upsert(fornecedoresMix);


        await TesteUpsert(host);
        //await TesteUpdateCompleto(host);
        //await TesteUpdateMetadeDadosEditados(host);

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

    /// <summary>
    /// Teste de um update 50% dos dados (unitário vs upsert), validando oq efetivamente tem alteração
    /// </summary>
    static async Task TesteUpdateMetadeDadosEditados(IHost host)
    {
        BenchmarkRunner.Run<TesteUpdateMetadeDadosEditados>();
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