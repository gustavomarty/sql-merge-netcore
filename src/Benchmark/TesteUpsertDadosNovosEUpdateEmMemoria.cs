﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
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
/// 4- Faz busca única de todos os dados e atualiza em memória. Faz apenas 1 insert de todos
/// 
/// -->> Executa o comparativo de forma unitária e com Upsert
/// </summary>
[RPlotExporter]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 10)]
public class TesteUpsertDadosNovosEUpdateEmMemoria
{

    //|          Method |       Mean |      Error |     StdDev |     Median |
    //|---------------- |-----------:|-----------:|-----------:|-----------:|
    //| ExecuteOneByOne | 4,793.3 ms | 2,808.9 ms | 1,857.9 ms | 4,215.0 ms |
    //|   ExecuteUpsert |   508.5 ms | 1,032.9 ms |   683.2 ms |   278.3 ms |

    private ServiceProvider _serviceProvider;
    private IFornecedorService _fornecedorService;

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
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.ROW_COUNT);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        //Busca todos os que existem no banco de dados
        var fornecedores = await _fornecedorService.GetMany(fornecedoresMix.Select(f => f.Documento).ToList());
        var fornecedoresNovos = new List<Fornecedor>();

        foreach (var fornecedorDto in fornecedoresMix)
        {
            var fornecedor = fornecedores.FirstOrDefault(f => f.Documento.Equals(fornecedorDto.Documento));

            //Se não existir, insert
            if (fornecedor == null)
            {
                fornecedoresNovos.Add(new Fornecedor(fornecedorDto.Nome, fornecedorDto.Documento, fornecedorDto.Cep));
                continue;
            }

            //Se existir, valida se tem modificação e executa o updade
            if (fornecedorDto.Nome != fornecedor.Nome || fornecedorDto.Cep != fornecedor.Cep)
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

        if(fornecedoresNovos != null)
        {
            await _fornecedorService.InsertRange(fornecedoresNovos);
            Console.WriteLine("Novos fornecedores inseridos");
        }
        Console.WriteLine($"Fim Unitário -> {DateTime.Now}");
    }

    [Benchmark]
    public async Task ExecuteUpsert()
    {
        Console.WriteLine($"Inicio Bulk -> {DateTime.Now}");

        //Configura o banco com os 100 primeiros fornecedores
        await _fornecedorService.CleanTable();
        await _fornecedorService.Upsert(await _fornecedorService.GetNewFakes(1000), ResponseType.ROW_COUNT);

        var fornecedoresMix = await _fornecedorService!.GetMix(1000, true);

        //Roda upsert
        await _fornecedorService.Upsert(fornecedoresMix, ResponseType.ROW_COUNT);

        Console.WriteLine($"Fim Bulk -> {DateTime.Now}");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }
}