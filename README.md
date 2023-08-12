# BulkMerge

## Objetivo

Quando precisamos processar uma quantidade significativa de dados em uma aplicação, por vezes executar de forma unitária cada registro pode não ser a melhor saída.
<br><br>
O projeto foi criado com o objetivo de contornar este problema, possibilitando uma forma de processamento mais eficaz utilizando comandos existentes no SqlServer e no .Net.
- Sql MERGE. [[Documentação](https://learn.microsoft.com/pt-br/sql/t-sql/statements/merge-transact-sql?view=sql-server-ver16)]
- .Net SqlBulkCopy. [[Documentação](https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?view=dotnet-plat-ext-7.0)]

## Estrutura do projeto

- **Component**:
Possui a implementação da biblioteca em si.

- **Poc**:
Possui projetos utilizando a implementação principal para testes e validações.

- **Bench**:
Possui o projeto que realiza benchmarks utilizando a abordagem proposta e implementações tradicionais.

## Guia de uso

Com a biblioteca referenciada em seu projeto, realize a injeção da mesma utilizando o método *ConfigureMergeBuilder*:

    builder.Services.ConfigureMergeBuilder();

Com o serviço injetado, já podemos configurar o builder para posteriormente executarmos a intrução no banco de dados. Exemplo abaixo utilizando uma classe de exemplo "Fornecedor":

    var builder = await _mergeBuilder.Create<Fornecedor>()
        .SetDataSource(dataSource)
        .SetMergeColumns(x => x.Documento)
        .SetUpdatedColumns(x => x)
        .SetTransaction(transaction.GetDbTransaction())
        .UseSnakeCaseNamingConvention()
        .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Cep, x.Nome })
        .SetIgnoreOnIsertOperation(x => x.Id)
        .UseEnumStatusConfiguration(x => x.Status)
        .Execute();

Após configurar todos os parâmetros desejados, basta invocar o método *Execute*.

## Parâmetros

Pare configurar o builder, os possuímos os parâmetros:

- SetDataSource [**obrigatório**]:
  - teste

- SetMergeColumns [**obrigatório**]:
  - teste

- SetUpdatedColumns [**obrigatório**]:
  - teste

- SetTransaction [**obrigatório**]:
  - teste

- WithCondition [**opcional**]:
  - teste

- SetIgnoreOnIsertOperation [**opcional**]:
  - teste

- UseEnumStatusConfiguration [**opcional**]:
  - teste

- UseSnakeCaseNamingConvention [**opcional**]:
  - teste

## Benchmarks

### Processamento de 1000 registros (realizando o update em 100% dos registros)

    //|         Method |       Mean |      Error |     StdDev |     Median |
    //|--------------- |-----------:|-----------:|-----------:|-----------:|
    //| UpdateOneByOne | 9,374.0 ms | 4,796.4 ms | 1,245.6 ms | 8,974.7 ms |
    //|         Upsert |   640.1 ms | 3,296.8 ms |   856.2 ms |   283.1 ms |

### Processamento de 1000 registros (realizando o update em 40% dos registros, descartando 60% de dados inalterados)

    //|          Method |       Mean |      Error |   StdDev |     Median |
    //|---------------- |-----------:|-----------:|---------:|-----------:|
    //| ExecuteOneByOne | 4,371.5 ms | 2,710.0 ms | 703.8 ms | 4,487.3 ms |
    //|   ExecuteUpsert |   512.2 ms | 2,610.2 ms | 677.9 ms |   214.0 ms |

### Processamento de 1000 registros (500 registros novos, 200 updates e 300 descartados)

    //|          Method |        Mean |      Error |     StdDev |      Median |
    //|---------------- |------------:|-----------:|-----------:|------------:|
    //| ExecuteOneByOne | 11,393.5 ms | 6,724.8 ms | 4,448.0 ms | 11,230.3 ms |
    //|   ExecuteUpsert |    498.7 ms |   931.6 ms |   616.2 ms |    299.2 ms |
