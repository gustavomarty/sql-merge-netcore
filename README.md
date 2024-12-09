# SqlComplexOperations
[Link da LIB](https://www.nuget.org/packages/SqlComplexOperations/)

## Objetivo

Quando precisamos processar uma quantidade significativa de dados em uma aplicação, por vezes executar de forma unitária cada registro pode não ser a melhor saída.
<br><br>
O projeto foi criado com o objetivo de contornar este problema, possibilitando uma forma de processamento mais eficaz utilizando recursos do SQL e .NET.
- SQL Merge (SqlServer). [[Documentação](https://learn.microsoft.com/pt-br/sql/t-sql/statements/merge-transact-sql?view=sql-server-ver16)]
- SQL Merge (Postgres). [[Documentação](https://www.postgresql.org/docs/current/sql-merge.html)]
- .Net SqlBulkCopy. [[Documentação](https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlbulkcopy?view=dotnet-plat-ext-7.0)]

## Estrutura do projeto

- **Component**:
Possui a implementação da biblioteca em si.

- **Poc**:
Possui projetos utilizando a implementação principal para testes e validações.

- **Bench**:
Possui o projeto que realiza benchmarks utilizando a abordagem proposta e implementações tradicionais.

## Bancos de dados suportados
 - [SqlServer](https://learn.microsoft.com/en-us/sql/sql-server/?view=sql-server-ver16)
 - [PostgreSql](https://www.postgresql.org/docs/)

## Sumario

 - [Guia de uso](https://github.com/gustavomarty/sql-merge-netcore/tree/main?tab=readme-ov-file#guia-de-uso)
 - [BulkMerge](https://github.com/gustavomarty/sql-merge-netcore/tree/main?tab=readme-ov-file#exemplo-de-uso-comando-merge-update--insert--delete)
 - [BulkInsert](https://github.com/gustavomarty/sql-merge-netcore/tree/main?tab=readme-ov-file#exemplo-de-uso-comando-copy-insert)
 - [Benchmarks](https://github.com/gustavomarty/sql-merge-netcore/tree/main?tab=readme-ov-file#benchmarks)
 - [Testes de Unidade](https://github.com/gustavomarty/sql-merge-netcore/tree/main?tab=readme-ov-file#exemplo-de-testes-de-unidade-usando-a-lib)

## Guia de uso

Com a biblioteca referenciada em seu projeto, realize a injeção da mesma utilizando o método *ConfigureSqlComplexOperations* para SqlServer:

    builder.Services.ConfigureSqlComplexOperations();

Ou *ConfigurePostgreSqlComplexOperations* para Postgres:

    builder.Services.ConfigurePostgreSqlComplexOperations();

## Exemplo de uso comando MERGE (update | insert | delete)

Configurando o builder para executarmos a instrução no banco de dados. Exemplo abaixo utilizando uma classe de exemplo "Fornecedor":

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

- Create [**obrigatório**]:
  - Cria o builder com a entidade de banco e o nomme da tabela que pode ser passado como string ou em caso de não passado como parametro, usa o nome da classe.

        .Create<Fornecedor>("fornecedores")

- SetDataSource [**obrigatório**]:
  - Lista de objetos que deseja adicionar, mesclar, atualizar ou deletar, no banco de dados. Deve possuir a mesma estrutura da tabela do banco.

        .SetDataSource(dataSource)

- SetMergeColumns [**obrigatório**]:
  - Lista de campos que deseja utilizar para comparar se o mesmo existe ou não na tabela de destino.
  - Caso as colunas comparadas existam, o comando entenderá uma atualização. Caso contrário, um novo registro será inserido.
  - Uma ou mais colunas podem ser configuradas, desde que elas existam na entidade configurada.

        .SetMergeColumns(x => x.Documento)

- SetUpdatedColumns [**obrigatório**]:
  - Colunas que serão afetadas no caso de uma atualização na tabela de destino.
  - Podemos configurar para executar em todas:

        .SetUpdatedColumns(x => x)
  
  - Podemos também configurar as colunas individualmente, sendo uma ou várias:

        .SetUpdatedColumns(x => new { x.Cep, x.Nome } )


- SetTransaction [**obrigatório**]:
  - Precisamos disponibilizar uma transação para a biblioteca realizar o comando no banco de dados.
  - Ao realizar a operação, você precisa realizar o commit no banco de dados para efetivar as alterações.
 
        .SetTransaction(transaction.GetDbTransaction())  

- UseDatabaseSchema [**opcional**]:
  - Caso a sua tabela não esteja no schema default do SQL você poderá informar o schema usando o *UseDatabaseSchema* 

        .UseDatabaseSchema("dif_schema")

- DeleteWhenDataIsNotInDataSource [**opcional**]:
  - Caso o dado esteja na sua tabela mas não esteja no data source, você pode deleta-lo. (Combinado com *UseStatusConfiguration*, a deleção pode ser logica, caso não seja usado o *UseStatusConfiguration* a deleção é real).

        .UseDatabaseSchema("dif_schema")

- WithCondition [**opcional**]:
  - Podemos configurar condições para o comando só realizar as alterações em registros existentes e que realmente sofreram alterações.
  - Antes de realizar o update, o comando checa se os campos que você deseja comparar estão diferentes entre o *data source* e a tabela de destino, evitando processamento desnecessário no banco de dados.
  - As condições devem ser configuradas citando um tipo de operação - quanto utilizada mais de uma coluna - (*AND* ou *OR*) e o tipo de comparação (*EQUAL* ou *NOT_EQUAL*).
  - Exemplo utilizando uma comparação *NOT_EQUAL* em duas colunas com instrução *OU*:

           .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Cep, x.Nome })

  - Exemplo utilizando uma comparação *EQUAL* em uma coluna:

           .WithCondition(ConditionTypes.EQUALS, x => x.Nome)
  
- SetIgnoreOnIsertOperation [**opcional**]:
  - Podemos configurar colunas que são ignoradas ao realizar a instrução de inserção, como por exemplo campos *auto identity* ou colunas que você simplesmente não queira utilizar na inserção do registro.

        .SetIgnoreOnIsertOperation(x => x.Id)
    
- UseEnumAsString [**opcional**]:
  - Em caso de querer usar enumeradores como string na base.

        .UseEnumAsString()
    
- UseStatusConfiguration [**opcional**]:
  - Utilize uma coluna de status em sua tabela de destino para receber a informação se, após a execução do comando merge, o registro foi alterado, inserido ou simplesmente não foi afetado (pode ser usado também para deleções lógicas). 
  - O status poode ser salvo como string ou como inteiro no seu banco de dados (em casos salvos como inteiro o valor terá os valores do enum *BulkMergeStatus*).

        .UseStatusConfiguration(false, x => x.Status) //Para salvar como inteiro.
        .UseStatusConfiguration(true, x => x.Status) //Para salvar como string.
  - Enum BulkMergeStatus

        PROCESSED = 0,
        UPDATED = 1,
        INSERTED = 2,
        PROCESSED_ERROR = 3,
        DELETED = 4

- UsePropertyNameAttribute [**opcional**]:
  - Em casos de nomes de coluna no .NET diferentes do nome de banco, você pode usar essa configuração.
  - Uso na propriedade da entidade (.NET)

            [PropertyName("Dif_Name")]
            public string Nome { get; set; }

            .UsePropertyNameAttribute()

- UseSnakeCaseNamingConvention [**opcional**]:
  - Caso utilize em seu banco de dados alguma [convensão](https://github.com/efcore/EFCore.NamingConventions) específica, a biblioteca da suporte a:
      - *UseSnakeCaseNamingConvention*

            .UseSnakeCaseNamingConvention()

- SetResponseType [**opcional**]:
  - Você pode setar o tipo de resposta que você espera receber, possiveis respostas:
    - NONE (Sem nenhum retorno)
    - ROW_COUNT (Retorna o numero de linhas afetadas) - [**default**]
    - SIMPLE (Retorna quantos registros foram atualizados, quantos foram inseridos e quantos foram deletados (Alem do total de registros alterados))
    - COMPLETE (Retorna um de -> para dos registros atualizados, o registro inserido (Caso exista) e o registro deletado (Caso exista))**

            .SetResponseType(ResponseType.SIMPLE)

    ** *O Uso do tipo COMPLETE quando usado com postgres, não consegue trazer em casos de atualização os dados antigos (pré update), só irá trazer os dados atualiados (pós update).*

## Exemplo de uso comando COPY (insert)

Configurando o builder para executarmos a instrução de inserção no banco de dados. Exemplo abaixo utilizando uma classe de exemplo "Fornecedor":

    var builder = _bulkInsertBuilder.Create<Fornecedor>()
        .SetDataSource(dataSource)
        .SetTransaction(_dbTransaction)
        .Execute();

Após configurar todos os parâmetros desejados, basta invocar o método *Execute*.

## Parâmetros

Pare configurar o builder, os possuímos os parâmetros:

- Create [**obrigatório**]:
  - Cria o builder com a entidade de banco e o nomme da tabela que pode ser passado como string ou em caso de não passado como parametro, usa o nome da classe.

        .Create<Fornecedor>("fornecedores")

- SetDataSource [**obrigatório**]:
  - Lista de objetos que deseja adicionar no banco de dados. Deve possuir a mesma estrutura da tabela do banco.

        .SetDataSource(dataSource)

- SetTransaction [**obrigatório**]:
  - Precisamos disponibilizar uma transação para a biblioteca realizar o comando no banco de dados.
  - Ao realizar a operação, você precisa realizar o commit no banco de dados para efetivar as alterações.
 
        .SetTransaction(transaction.GetDbTransaction())  
    
- UseDatabaseSchema [**opcional**]:
  - Caso a sua tabela não esteja no schema default do SQL você poderá informar o schema usando o *UseDatabaseSchema* 

        .UseDatabaseSchema("dif_schema")

- UsePropertyNameAttribute [**opcional**]:
  - Em casos de nomes de coluna no .NET diferentes do nome de banco, você pode usar essa configuração.
  - Uso na propriedade da entidade (.NET)

            [PropertyName("Dif_Name")]
            public string Nome { get; set; }

            .UsePropertyNameAttribute()

- UseEnumAsString [**opcional**]:
  - Em caso de querer usar enumeradores como string na base.

        .UseEnumAsString()
 

- UseSnakeCaseNamingConvention [**opcional**]:
  - Caso utilize em seu banco de dados alguma [convensão](https://github.com/efcore/EFCore.NamingConventions) específica, a biblioteca da suporte a:
      - *UseSnakeCaseNamingConvention*

            .UseSnakeCaseNamingConvention()

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

### Processamento de 1000 registros (500 registros novos, 200 updates e 300 descartados) (minimizando queries de busca no banco)

    //|          Method |       Mean |      Error |   StdDev |     Median |
    //|---------------- |-----------:|-----------:|---------:|-----------:|
    //| ExecuteOneByOne | 3,492.1 ms | 3,190.9 ms | 828.7 ms | 3,690.2 ms |
    //|   ExecuteUpsert |   492.3 ms | 2,404.4 ms | 624.4 ms |   216.1 ms |

### Processamento de 1000 registros (500 registros novos, 200 updates e 300 descartados) (minimizando queries de busca no banco e insert unico no processamento sem bulk)
    
    |          Method |       Mean |      Error |     StdDev |     Median |
    |---------------- |-----------:|-----------:|-----------:|-----------:|
    | ExecuteOneByOne | 4,793.3 ms | 2,808.9 ms | 1,857.9 ms | 4,215.0 ms |
    |   ExecuteUpsert |   508.5 ms | 1,032.9 ms |   683.2 ms |   278.3 ms |    

## Exemplo de testes de unidade usando a LIB

### Utilizando MOQ

```csharp
public class LibTestExampleMoq
{
    private readonly ApplicationContext _context;
    private readonly Mock<IMergeBuilder> _mergeBuilderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClubeService _clubeService;

    public LibTestExampleMoq()
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(contextOptions);

        _mergeBuilderMock = new Mock<IMergeBuilder>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var dbTransactionMock = new Mock<IDbTransaction>();
        _unitOfWorkMock.Setup(x => x.GetDbTransaction()).Returns(dbTransactionMock.Object);

        _clubeService = new(_context, _mergeBuilderMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async void Upsert_WithMoq()
    {
        //ARRANGE
        var list = new List<ClubeDto> {
            new() { Nome = "São Paulo", Abreviacao = "SPFC", Apelido = "Tricolor" },
            new() { Nome = "Coritiba", Abreviacao = "CFC", Apelido = "Coxa" }
        };

        var entityList = new List<Clube> {
            new() { Nome = "Coritiba", Abreviacao = "CFC", Apelido = "Verdão" }
        };

        await _context.AddRangeAsync(entityList);
        await _context.SaveChangesAsync();

        var databaseServiceMock = new Mock<IDatabaseService>();
        var builderMock = new Mock<MergeBuilder<Clube>>(databaseServiceMock.Object, DatabaseType.MICROSOFT_SQL_SERVER) { CallBase = true };
        _mergeBuilderMock.Setup(x => x.Create<Clube>()).Returns(builderMock.Object);
        builderMock.Setup(x => x.Execute()).Returns(Task.FromResult(new OutputModel()));

        //ACTION
        await _clubeService.Upsert(list);

        //ASSERT
        _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
    }
}
```

### Utilizando NSubstitute

```csharp
public class LibTestExampleNSubstitute
{
    private readonly ApplicationContext _context;
    private readonly IMergeBuilder _mergeBuilderMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ClubeService _clubeService;

    public LibTestExampleNSubstitute()
    {
        var contextOptions = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationContext(contextOptions);

        _mergeBuilderMock = Substitute.For<IMergeBuilder>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        var dbTransactionMock = Substitute.For<IDbTransaction>();
        _unitOfWorkMock.GetDbTransaction().Returns(dbTransactionMock);

        _clubeService = new(_context, _mergeBuilderMock, _unitOfWorkMock);
    }

    [Fact]
    public async void Upsert_WithMoq()
    {
        //ARRANGE
        var list = new List<ClubeDto> {
            new() { Nome = "São Paulo", Abreviacao = "SPFC", Apelido = "Tricolor" },
            new() { Nome = "Coritiba", Abreviacao = "CFC", Apelido = "Coxa" }
        };

        var entityList = new List<Clube> {
            new() { Nome = "Coritiba", Abreviacao = "CFC", Apelido = "Verdão" }
        };

        await _context.AddRangeAsync(entityList);
        await _context.SaveChangesAsync();

        var databaseServiceMock = Substitute.For<IDatabaseService>();
        var builderMock = Substitute.ForPartsOf<MergeBuilder<Clube>>(databaseServiceMock, DatabaseType.MICROSOFT_SQL_SERVER);
        _mergeBuilderMock.Create<Clube>().Returns(builderMock);
        builderMock.Execute().Returns(Task.FromResult(new OutputModel()));

        //ACTION
        await _clubeService.Upsert(list);

        //ASSERT
        _unitOfWorkMock.Received(1).CommitTransaction();
    }
}
```
