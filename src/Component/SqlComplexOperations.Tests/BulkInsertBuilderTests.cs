using SqlComplexOperations.Models.Enumerators;
using SqlComplexOperations.Services;
using SqlComplexOperations.Tests.Models;
using NSubstitute;
using System.Data;
using Xunit;
using SqlComplexOperations.Models.Output;
using SqlComplexOperations.Exceptions;

namespace SqlComplexOperations.Tests
{
    public class BulkInsertBuilderTests
    {
        private readonly string _pkQuery = "select column_name from information_schema.key_column_usage where objectproperty(object_id(constraint_schema + '.' + quotename(constraint_name)), 'IsPrimaryKey') = 1 and table_name = '{0}'";
        private readonly string _pkQueryWithSchema = "select column_name from information_schema.key_column_usage where objectproperty(object_id(constraint_schema + '.' + quotename(constraint_name)), 'IsPrimaryKey') = 1 and table_name = '{0}' and table_schema = '{1}'";
        private readonly string _buildAllColumnsDbOrderQuery = "select name from sys.columns where object_id = object_id('{0}') order by column_id";
        private readonly string _buildAllColumnsDbOrderQueryWithSchema = "select name from sys.columns where object_id = object_id('{1}.{0}') order by column_id";
        private readonly string _mergeQuery = "MERGE {0} as tgt \n using (select * from #{0}) as src on {1}\n when matched {2} then \n update set {3}\n when not matched by target then \n insert  ({4}) values ({5}) \n output $action;";
        private readonly string _mergeQuerySchema = "MERGE [{1}].{0} as tgt \n using (select * from [{1}].#{0}) as src on {2}\n when matched {3} then \n update set {4}\n when not matched by target then \n insert  ({5}) values ({6}) \n output $action;";
        private readonly string _mergeQueryCompleteResponse = "MERGE {0} as tgt \n using (select * from #{0}) as src on {1}\n when matched {2} then \n update set {3}\n when not matched by target then \n insert  ({4}) values ({5}) \n output $action{6};";
        private readonly string _mergeQueryNoneResponse = "MERGE {0} as tgt \n using (select * from #{0}) as src on {1}\n when matched {2} then \n update set {3}\n when not matched by target then \n insert  ({4}) values ({5});";
        private readonly string _mergeQueryDeleteClause = "MERGE {0} as tgt \n using (select * from #{0}) as src on {1}\n when matched {2} then \n update set {3}\n when not matched by source then \n delete \n when not matched by target then \n insert  ({4}) values ({5}) \n output $action;";
        private readonly string _mergeQueryDeleteClauseStatusOn = "MERGE {0} as tgt \n using (select * from #{0}) as src on {1}\n when matched {2} then \n update set {3}\n when not matched by source then \n update set {4} \n when not matched by target then \n insert  ({5}) values ({6}) \n output $action;";

        private readonly IDatabaseService _databaseService;
        private readonly IDbTransaction _dbTransaction;

        private readonly IBulkInsertBuilder _bulkInsertBuilder;

        public BulkInsertBuilderTests()
        {
            _databaseService = Substitute.For<IDatabaseService>();
            _dbTransaction = Substitute.For<IDbTransaction>();

            _bulkInsertBuilder = new BulkInsertBuilder(_databaseService);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS OFF | SCHEMA OFF )")]
        public async void Test_Ok()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var buildAllColumnsDbOrderQuery = string.Format(_buildAllColumnsDbOrderQuery, "PersonEntity");
            
            _databaseService.ExecuteReaderCommand(Arg.Any<IDbTransaction>(), Arg.Is(buildAllColumnsDbOrderQuery))
                .Returns(new List<string>
                {
                    "Id",
                    "Name",
                    "Document",
                    "BirthDate",
                    "UpdatedDate"
                });

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteReaderCommand(Arg.Any<IDbTransaction>(), buildAllColumnsDbOrderQuery);
        }

        [Fact]
        public async void Test_Ok_Schema()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var buildAllColumnsDbOrderQuery = string.Format(_buildAllColumnsDbOrderQueryWithSchema, "PersonEntity", "dbo");

            _databaseService.ExecuteReaderCommand(Arg.Any<IDbTransaction>(), Arg.Is(buildAllColumnsDbOrderQuery))
                .Returns(new List<string>
                {
                    "Id",
                    "Name",
                    "Document",
                    "BirthDate",
                    "UpdatedDate"
                });

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .UseDatabaseSchema("dbo")
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteReaderCommand(Arg.Any<IDbTransaction>(), buildAllColumnsDbOrderQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS OFF | SCHEMA OFF | RESULT TYPE SIMPLE) Usando annotation de property name")]
        public async void Test_Ok_WithPropertyNameAnnotation()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.GetWithPropName(10);
            var buildAllColumnsDbOrderQuery = string.Format(_buildAllColumnsDbOrderQuery, "PersonEntity");

            _databaseService.ExecuteReaderCommand(Arg.Any<IDbTransaction>(), Arg.Is(buildAllColumnsDbOrderQuery))
                .Returns(new List<string>
                {
                    "id",
                    "nome",
                    "cpf",
                    "dta_aniversario",
                    "dta_atualizacao"
                });

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntityPropName>("PersonEntity")
                .UsePropertyNameAttribute()
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteReaderCommand(Arg.Any<IDbTransaction>(), buildAllColumnsDbOrderQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE ON | STATUS OFF | SCHEMA OFF | RESULT TYPE SIMPLE)")]
        public async void Test_Ok_SnakeCase()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var buildAllColumnsDbOrderQuery = string.Format(_buildAllColumnsDbOrderQuery, "person_entity");

            _databaseService.ExecuteReaderCommand(Arg.Any<IDbTransaction>(), Arg.Is(buildAllColumnsDbOrderQuery))
                .Returns(new List<string>
                {
                    "id",
                    "name",
                    "document",
                    "birth_date",
                    "updated_date"
                });

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .UseSnakeCaseNamingConvention()
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteReaderCommand(Arg.Any<IDbTransaction>(), buildAllColumnsDbOrderQuery);
        }

        [Fact(DisplayName = "Teste cenario com erro (DbTransaction == null)")]
        public async void Test_Error_WithoutTransaction()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .SetDataSource(dataSource);

            //ACTION
            var result = await Assert.ThrowsAsync<InvalidDbTransactionException<PersonEntity>>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You need to inform the DbTransaction, call the method SetTransaction(IDbTransaction transaction) before execute merge.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (DbTransaction.Connection == null)")]
        public async void Test_Error_WithoutTransactionConnection()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);

            _dbTransaction.Connection
                .ReturnsForAnyArgs(x => null);

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<InvalidDbTransactionException<PersonEntity>>(() => builder.Execute());

            //ASSERT
            Assert.Equal("The DbTransaction informed is without one active Connection.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (!DataSource.Any())")]
        public async void Test_Error_WithoutDataSource()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<InvalidDataSourceException<PersonEntity>>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You need to inform the DataSource, call the method SetDataSource(...) before execute merge.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (Invalid attribute configuration)")]
        public async void Test_Error_InvalidPropNameAttribute()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _bulkInsertBuilder.Create<PersonEntity>()
                .SetDataSource(dataSource)
                .UsePropertyNameAttribute()
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<InvalidPropertyNameConfigurationException<PersonEntity>>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You are using the 'UsePropertyNameAttribute' configuration, all attributes of your entity needs be mapped.", result.Message);
        }
    }
}