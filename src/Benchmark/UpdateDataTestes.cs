using BenchmarkDotNet.Attributes;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[RPlotExporter]
public class UpdateDataTestes
{
    //[Params(100, 1000)]
    //public int N;

    private List<MaterialDto> _materials;
    private List<ClubeDto> _clubes;
    private List<FornecedorDto> _fornecedores;

    private ServiceProvider _serviceProvider;
    private IClubeService _clubeService;
    private IMaterialService _materialService;
    private IFornecedorService _fornecedorService;
    private IContratoService _contratoService;


    [GlobalSetup]
    public async Task Setup()
    {
        var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = configurationBuilder.Build();
        //EntityFrameworkManager.ContextFactory = context => new ApplicationContext(Program.GetDbContextOptions(configuration));

        _serviceProvider = Program.GetServiceProvider(configuration);

        _materialService = _serviceProvider.GetRequiredService<IMaterialService>();
        _fornecedorService = _serviceProvider.GetRequiredService<IFornecedorService>();
        _clubeService = _serviceProvider.GetRequiredService<IClubeService>();
        _contratoService = _serviceProvider.GetRequiredService<IContratoService>();
    }

    [Benchmark]
    public async Task UpdateOneByOne()
    {
        Console.WriteLine($"Inicio Unitario -> {DateTime.Now}");

        var contratosMix = await _contratoService!.GetMixAll();

        foreach (var contrato in contratosMix)
        {
            await _contratoService.Update(contrato);
        }

        Console.WriteLine($"Fim Unitário -> {DateTime.Now}");

    }

    [Benchmark]
    public async Task Upsert()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        var contratosMix = await _contratoService!.GetMixAll();
        await _contratoService.Upsert(contratosMix);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");

    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }
}