using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[RPlotExporter]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
public class TesteUpdateMetadeDadosEditados
{
    private ServiceProvider _serviceProvider;
    private IFornecedorService _fornecedorService;

    //|          Method |       Mean |      Error |     StdDev |     Median |
    //|---------------- |-----------:|-----------:|-----------:|-----------:|
    //| ExecuteOneByOne | 1,848.6 ms | 3,895.8 ms | 1,011.7 ms | 1,399.9 ms |
    //|   ExecuteUpsert |   519.3 ms | 2,638.1 ms |   685.1 ms |   219.3 ms |

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

        //Configura o banco com os 100 primeiros fornecedores
        await _fornecedorService.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000));

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true, false);
        
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
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000));

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true, false);

        //Roda upsert
        await _fornecedorService.Upsert(fornecedoresMix);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }
}