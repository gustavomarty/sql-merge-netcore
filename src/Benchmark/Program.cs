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
                services.AddScoped<InsertTestsDebug>();
            })
            .Build();

        //Run Benchmark
        BenchmarkRunner.Run<InsertTests>();

        //Run Debug
        //var myTest = host.Services.GetService<InsertTestsDebug>();
        //await myTest!.BuildPayloads();
        //await myTest!.RunInsertOneByOne();

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