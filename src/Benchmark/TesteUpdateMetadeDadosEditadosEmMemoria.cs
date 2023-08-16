using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


/// <summary>
/// 1- Cria 1000 registros no banco
/// 2- Modifica ~40% dos existentes (~400) e mantem ~60% (~600) inalterados
/// 3- Valida se:
///     - o dados é novo? Insere (range, mantendo em memória)
///     - o dado foi alterado? Atualiza
///     - o dado não foi alterado? Descarta
/// 4- Faz busca única de todos os dados e atualiza em memória
/// 
/// -->> Executa o comparativo de forma unitária e com Upsert
/// </summary>
[RPlotExporter]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 10)]
public class TesteUpdateMetadeDadosEditadosEmMemoria
{
    private ServiceProvider _serviceProvider;
    private IFornecedorService _fornecedorService;

    //|          Method |       Mean |      Error |   StdDev |     Median |
    //|---------------- |-----------:|-----------:|---------:|-----------:|
    //| ExecuteOneByOne | 3,492.1 ms | 3,190.9 ms | 828.7 ms | 3,690.2 ms |
    //|   ExecuteUpsert |   492.3 ms | 2,404.4 ms | 624.4 ms |   216.1 ms |

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
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000));

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true, false);

        //Busca todos os que existem no banco de dados
        var fornecedores = await _fornecedorService.GetMany(fornecedoresMix.Select(f => f.Documento).ToList());

        foreach (var fornecedorDto in fornecedoresMix)
        {
            var fornecedor = fornecedores.FirstOrDefault(f => f.Documento.Equals(fornecedorDto.Documento));

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