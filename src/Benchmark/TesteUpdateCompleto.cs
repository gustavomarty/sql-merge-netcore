using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[RPlotExporter]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
public class TesteUpdateCompleto
{
    private ServiceProvider _serviceProvider;
    private IContratoService _contratoService;

    //|         Method |       Mean |      Error |     StdDev |     Median |
    //|--------------- |-----------:|-----------:|-----------:|-----------:|
    //| UpdateOneByOne | 9,374.0 ms | 4,796.4 ms | 1,245.6 ms | 8,974.7 ms |
    //|         Upsert |   640.1 ms | 3,296.8 ms |   856.2 ms |   283.1 ms |

    [GlobalSetup]
    public async Task Setup()
    {
        var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = configurationBuilder.Build();

        _serviceProvider = Program.GetServiceProvider(configuration);
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