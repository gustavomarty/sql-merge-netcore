using BenchmarkDotNet.Attributes;
using Contracts.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[RPlotExporter]
public class TesteUpdateCompleto
{
    private ServiceProvider _serviceProvider;
    private IContratoService _contratoService;


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