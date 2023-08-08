using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BenchmarkDotNet.Engines;
using Contracts.Data.Configurations;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Contracts.Service;
using Contracts.Service.Interfaces;

internal class Program
{
    private static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddEntityFrameworkConfiguration(builder.Configuration);
        builder.Services.AddScoped<ITimeService, TimeService>();
        builder.Services.AddScoped<IFornecedorService, FornecedorService>();
        builder.Services.AddScoped<IMaterialService, MaterialService>();
        builder.Services.AddScoped<IContratoService, ContratoService>();

        using Microsoft.Extensions.Hosting.IHost host = builder.Build();

        //ExemplifyServiceLifetime(host.Services, "Lifetime 1");
        //ExemplifyServiceLifetime(host.Services, "Lifetime 2");
        
        //BenchmarkRunner.Run<Md5VsSha256>();

        await host.RunAsync();
    }
}


