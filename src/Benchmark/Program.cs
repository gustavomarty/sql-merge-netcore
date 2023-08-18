using SqlComplexOperations.Extensions;
using Contracts.Service;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Hosting;
using Contracts.Service.Interfaces;
using Contracts.Data.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Data.Data.Entities;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.ConfigureSqlComplexOperations();
                services.AddEntityFrameworkConfiguration(context.Configuration);
                services.AddScoped<IClubeService, ClubeService>();
                services.AddScoped<IFornecedorService, FornecedorService>();
                services.AddScoped<IMaterialService, MaterialService>();
                services.AddScoped<IContratoService, ContratoService>();
                services.AddScoped<ConfigureTestes>();
            })
            .Build();


        var _fornecedorService = host.Services.GetService<IFornecedorService>();


        await _fornecedorService.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000));

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        //Busca todos os que existem no banco de dados
        var fornecedores = await _fornecedorService.GetMany(fornecedoresMix.Select(f => f.Documento).ToList());
        var fornecedoresNovos = new List<Fornecedor>();

        foreach(var fornecedorDto in fornecedoresMix)
        {
            var fornecedor = fornecedores.FirstOrDefault(f => f.Documento.Equals(fornecedorDto.Documento));

            //Se não existir, insert
            if(fornecedor == null)
            {
                fornecedoresNovos.Add(new Fornecedor(fornecedorDto.Nome, fornecedorDto.Documento, fornecedorDto.Cep));
                continue;
            }

            //Se existir, valida se tem modificação e executa o updade
            if(fornecedorDto.Nome != fornecedor.Nome || fornecedorDto.Cep != fornecedor.Cep)
            {
                fornecedor.Nome = fornecedorDto.Nome;
                fornecedor.Cep = fornecedorDto.Cep;
                fornecedor.DataAlteracao = DateTime.Now;
                await _fornecedorService.Update(fornecedor);
                Console.WriteLine("Dado alterado");
                continue;
            }

            //Caso não tenha modificação, descarta o dado
            Console.WriteLine("Dado descartado");
        }

        if(fornecedoresNovos != null)
        {
            await _fornecedorService.InsertRange(fornecedoresNovos);
            Console.WriteLine("Novos fornecedores inseridos");
        }






        //await TesteUpsert(host);
        //await TesteUpdateCompleto(host);
        //await TesteUpdateMetadeDadosEditados(host);
        //await TesteUpdateMetadeDadosEditadosEmMemoria(host);
        //await TesteUpsertEmMemoria(host);

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
    /// Teste de um Upsert, com dados novos, alterados e imutáveis
    /// </summary>
    static async Task TesteUpsertEmMemoria(IHost host)
    {
        BenchmarkRunner.Run<TesteUpsertDadosNovosEUpdateEmMemoria>();
    }

    /// <summary>
    /// Teste de um update de 50% dos dados (unitário vs upsert), validando o que efetivamente tem alteração
    /// </summary>
    static async Task TesteUpdateMetadeDadosEditados(IHost host)
    {
        BenchmarkRunner.Run<TesteUpdateMetadeDadosEditados>();
    }

    /// <summary>
    /// Teste de um update de 50% dos dados (unitário vs upsert), validando o que efetivamente tem alteração (mantem lista em memória)
    /// </summary>
    static async Task TesteUpdateMetadeDadosEditadosEmMemoria(IHost host)
    {
        BenchmarkRunner.Run<TesteUpdateMetadeDadosEditadosEmMemoria>();
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
        services.ConfigureSqlComplexOperations();
        services.AddEntityFrameworkConfiguration(configuration);
        services.AddScoped<IClubeService, ClubeService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IContratoService, ContratoService>();
    }
}