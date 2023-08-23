using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlComplexOperations.Models.Enumerators;

/// <summary>
/// 1- Cria 1000 registros no banco
/// 2- Cria 500 novos registros, modifica ~40% dos existentes (~200) e mantem ~60% (~300) inalterados
/// 3- Valida se:
///     - o dados é novo? Insere
///     - o dado foi alterado? Atualiza
///     - o dado não foi alterado? Descarta
/// 
/// -->> Executa o comparativo de forma unitária e com Upsert
/// </summary>
[RPlotExporter]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 3)]
public class TesteUpsertDadosNovosEUpdate
{
    //|          Method |        Mean |      Error |     StdDev |      Median |
    //|---------------- |------------:|-----------:|-----------:|------------:|
    //| ExecuteOneByOne | 11,393.5 ms | 6,724.8 ms | 4,448.0 ms | 11,230.3 ms |
    //|   ExecuteUpsert |    498.7 ms |   931.6 ms |   616.2 ms |    299.2 ms |

    private ServiceProvider _serviceProvider;
    private IFornecedorService _fornecedorService;

    [GlobalSetup]
    public async Task Setup()
    {
        var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = configurationBuilder.Build();

        _serviceProvider = Program.GetServiceProvider(configuration);
        _fornecedorService = _serviceProvider.GetRequiredService<IFornecedorService>();
    }

    [Benchmark]
    public async Task ExecuteOneByOne()
    {
        Console.WriteLine($"Inicio Unitario -> {DateTime.Now}");

        await _fornecedorService.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.ROW_COUNT);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);
        
        foreach (var fornecedorDto in fornecedoresMix)
        {
            //Validar existencia
            var fornecedor = await _fornecedorService.Get(fornecedorDto.Documento);

            //Se não existir, insert
            if (fornecedor == null)
            {
                await _fornecedorService.Insert(new Contracts.Data.Data.Entities.Fornecedor(fornecedorDto.Nome, fornecedorDto.Documento, fornecedorDto.Cep));
                Console.WriteLine("Dado Inserido");
                continue;
            }

            //Se existir, valida se tem modificação e executa o updade
            if(fornecedorDto.Nome != fornecedor.Nome || fornecedorDto.Cep != fornecedor.Cep)
            {
                await _fornecedorService.Update(fornecedorDto);
                Console.WriteLine("Dado alterado");
                continue;
            }

            //Caso não tenha modificação, descarta o dado
            Console.WriteLine("Dado descartado");
        }

        Console.WriteLine($"Fim Unitário -> {DateTime.Now}");
    }

    [Benchmark]
    public async Task ExecuteUpsert()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        //Configura o banco com os 100 primeiros fornecedores
        await _fornecedorService.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.ROW_COUNT);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        //Roda upsert
        await _fornecedorService.Upsert(fornecedoresMix, ResponseType.ROW_COUNT);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }
}