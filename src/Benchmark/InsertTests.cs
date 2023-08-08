using BenchmarkDotNet.Attributes;
using Contracts.Data.Data;
using Contracts.Data.Models.Dtos;
using Contracts.Service;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Z.EntityFramework.Extensions;

[RPlotExporter]
public class InsertTests
{
    //[Params(100, 1000)]
    //public int N;

    private List<MaterialDto> _materials;
    private List<TeamDto> _times;
    private List<FornecedorDto> _fornecedores;

    private ServiceProvider _serviceProvider;
    private ITimeService _timeService;
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
        _timeService = _serviceProvider.GetRequiredService<ITimeService>();
        _contratoService = _serviceProvider.GetRequiredService<IContratoService>();

        await BuildPayloads();
    }

    public async Task BuildPayloads()
    {
        _materials = await _materialService.GetNewFakes(10);
        _times = await _timeService.GetNewFakes(10);
        _fornecedores = await _fornecedorService.GetNewFakes(10);
    }

    [Benchmark]
    public async Task RunInsertOneByOne()
    {
        Console.WriteLine($"Inicio Unitario -> {DateTime.Now}");

        //Insert Clube
        await _timeService.Create(_times);

        //Insert Material
        await _materialService.Create(_materials);

        //Insert Fornecedor
        await _fornecedorService.Create(_fornecedores);

        //Insert Contrato
        await _contratoService.Create(await _contratoService.GetNewFakes(10));

        //Limpa tabelas
        await _contratoService.CleanTable();
        await _timeService.CleanTable();
        await _materialService.CleanTable();
        await _fornecedorService.CleanTable();

        Console.WriteLine($"Fim Unitário -> {DateTime.Now}");

    }

    //[Benchmark]
    public async Task RunInsertBulk()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        //Insert Clube
        await _timeService.CreateBulk(_times);

        //Insert Material
        await _materialService.CreateBulk(_materials);

        //Insert Fornecedor
        await _fornecedorService.CreateBulk(_fornecedores);

        //Insert Contrato
        await _contratoService.CreateBulk(await _contratoService.GetNewFakes(100));

        //Limpa tabelas
        await _timeService.CleanTable();
        await _materialService.CleanTable();
        await _fornecedorService.CleanTable();
        await _contratoService.CleanTable();

        await Task.Delay(3000);
        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");

    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }
}