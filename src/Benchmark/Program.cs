using Contracts.Service;
using Microsoft.Extensions.Hosting;
using Contracts.Service.Interfaces;
using Contracts.Data.Configurations;
using Microsoft.Extensions.DependencyInjection;
using BenchmarkDotNet.Running;
using Contracts.Data.Data.Entities;
using Contracts.Data.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddEntityFrameworkConfiguration(context.Configuration);
                services.AddScoped<ITimeService, TimeService>();
                services.AddScoped<IFornecedorService, FornecedorService>();
                services.AddScoped<IMaterialService, MaterialService>();
                services.AddScoped<IContratoService, ContratoService>();
                services.AddScoped<ConfigureTestes>();
            })
            .Build();

        //Configure Data
        var insertTestsDebug = host.Services.GetService<ConfigureTestes>();
        var contratoService = host.Services.GetService<IContratoService>();
        await insertTestsDebug!.CleanTables();
        await insertTestsDebug!.BuildPayloads(400);
        await insertTestsDebug!.RunInsertBulk(400);

        //var teste = await contratoService.GetMixAll();
        //await contratoService.Upsert(teste);

        //Run Benchmark
        BenchmarkRunner.Run<UpdateDataTestes>();


        await host.RunAsync();
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
        services.AddEntityFrameworkConfiguration(configuration);
        services.AddScoped<ITimeService, TimeService>();
        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IContratoService, ContratoService>();
    }

    //public static DbContextOptions<ApplicationContext> GetDbContextOptions(IConfiguration configuration)
    //{
    //    string? connectionString = configuration["ConnectionStrings:SqlServerConnection"];

    //    DbContextOptionsBuilder<ApplicationContext> optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>()
    //        .UseSqlServer(connectionString);

    //    return optionsBuilder.Options;
    //}

}