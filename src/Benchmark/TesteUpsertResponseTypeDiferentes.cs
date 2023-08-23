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
public class TesteUpsertResponseTypeDiferentes
{
//|                      Method |     Mean |       Error |     StdDev |   Median |
//|---------------------------- |---------:|------------:|-----------:|---------:|
//|     ExecuteResponseTypeNone | 901.1 ms | 21,857.9 ms | 1,198.1 ms | 214.2 ms |
//| ExecuteResponseTypeRowCount | 643.7 ms | 14,264.0 ms |   781.9 ms | 195.3 ms |
//|   ExecuteResponseTypeSimple | 619.4 ms | 14,526.4 ms |   796.2 ms | 161.1 ms |
//| ExecuteResponseTypeComplete | 693.0 ms | 15,465.5 ms |   847.7 ms | 211.1 ms |

    private ServiceProvider? _serviceProvider;
    private IFornecedorService? _fornecedorService;

    [GlobalSetup]
    public void Setup()
    {
        var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = configurationBuilder.Build();

        _serviceProvider = Program.GetServiceProvider(configuration);
        _fornecedorService = _serviceProvider.GetRequiredService<IFornecedorService>();
    }

    [Benchmark]
    public async Task ExecuteResponseTypeNone()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        await _fornecedorService!.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.NONE);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        await _fornecedorService.Upsert(fornecedoresMix, ResponseType.NONE);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [Benchmark]
    public async Task ExecuteResponseTypeRowCount()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        await _fornecedorService!.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.ROW_COUNT);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        await _fornecedorService.Upsert(fornecedoresMix, ResponseType.ROW_COUNT);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [Benchmark]
    public async Task ExecuteResponseTypeSimple()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        await _fornecedorService!.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.SIMPLE);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        await _fornecedorService.Upsert(fornecedoresMix, ResponseType.SIMPLE);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [Benchmark]
    public async Task ExecuteResponseTypeComplete()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        await _fornecedorService!.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.COMPLETE);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        await _fornecedorService.Upsert(fornecedoresMix, ResponseType.COMPLETE);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider!.Dispose();
    }
}