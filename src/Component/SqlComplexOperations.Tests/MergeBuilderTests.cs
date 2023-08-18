using SqlComplexOperations.Models.Enumerators;
using SqlComplexOperations.Services;
using SqlComplexOperations.Tests.Models;
using NSubstitute;
using System.Data;
using Xunit;

namespace SqlComplexOperations.Tests
{
    public class MergeBuilderTests
    {
        private readonly string _pkQuery = "select column_name from information_schema.key_column_usage where objectproperty(object_id(constraint_schema + '.' + quotename(constraint_name)), 'IsPrimaryKey') = 1 and table_name = '{0}'";
        private readonly string _createTempTableQuery = "Select Top 0 * into #{0} from {0}";
        private readonly string _dropTemTableQuery = "drop table #{0}";
        private readonly string _mergeQuery = "MERGE {0} as tgt \n using (select * from #{0}) as src on {1}\n when matched {2} then \n update set {3}\n when not matched then \n insert values ({4}) \n output $action;";

        private readonly IDatabaseService _databaseService;
        private readonly IDbTransaction _dbTransaction;

        private readonly IMergeBuilder _mergeBuilder;

        public MergeBuilderTests()
        {
            _databaseService = Substitute.For<IDatabaseService>();
            _dbTransaction = Substitute.For<IDbTransaction>();

            _mergeBuilder = new MergeBuilder(_databaseService);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS OFF)")]
        public async void Test_Ok()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntity");
            var createTempTableQuery = string.Format(_createTempTableQuery, "PersonEntity");
            var dropTempTableQuery = string.Format(_dropTemTableQuery, "PersonEntity");
            
            var mergeQuery = string.Format(_mergeQuery,
                "PersonEntity",
                "tgt.Document = src.Document",
                "AND (tgt.Name != src.Name or tgt.BirthDate != src.BirthDate)",
                "tgt.Name = src.Name, tgt.Document = src.Document, tgt.BirthDate = src.BirthDate, tgt.UpdatedDate = src.UpdatedDate",
                "src.Name, src.Document, src.BirthDate, src.UpdatedDate"
            );

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntity>()
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE ON | STATUS OFF)")]
        public async void Test_Ok_SnakeCase()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "person_entity");
            var createTempTableQuery = string.Format(_createTempTableQuery, "person_entity");
            var dropTempTableQuery = string.Format(_dropTemTableQuery, "person_entity");

            var mergeQuery = string.Format(_mergeQuery,
                "person_entity",
                "tgt.document = src.document",
                "AND (tgt.name = src.name and tgt.birth_date = src.birth_date)",
                "tgt.name = src.name, tgt.document = src.document, tgt.birth_date = src.birth_date, tgt.updated_date = src.updated_date",
                "src.name, src.document, src.birth_date, src.updated_date"
            );

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntity>()
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.EQUALS, ConditionOperator.AND, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS ON, TYPE = INT)")]
        public async void Test_Ok_StatusInt()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.GetWithStatus(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntityStatus");
            var createTempTableQuery = string.Format(_createTempTableQuery, "PersonEntityStatus");
            var dropTempTableQuery = string.Format(_dropTemTableQuery, "PersonEntityStatus");

            var mergeQuery = string.Format(_mergeQuery,
                "PersonEntityStatus",
                "tgt.Document = src.Document",
                "AND (tgt.Name != src.Name or tgt.BirthDate != src.BirthDate)",
                "tgt.Name = src.Name, tgt.Document = src.Document, tgt.BirthDate = src.BirthDate, tgt.UpdatedDate = src.UpdatedDate, tgt.Status = 1",
                "src.Name, src.Document, src.BirthDate, src.UpdatedDate, 2"
            );

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntityStatus>()
                .UseStatusConfiguration(false, x => x.Status)
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => new { x.Name, x.Document, x.BirthDate, x.UpdatedDate })
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => new { x.Id, x.StatusStr })
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS ON, TYPE = STRING)")]
        public async void Test_Ok_StatusString()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.GetWithStatus(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntityStatus");
            var createTempTableQuery = string.Format(_createTempTableQuery, "PersonEntityStatus");
            var dropTempTableQuery = string.Format(_dropTemTableQuery, "PersonEntityStatus");

            var mergeQuery = string.Format(_mergeQuery,
                "PersonEntityStatus",
                "tgt.Document = src.Document",
                "AND (tgt.Name != src.Name or tgt.BirthDate != src.BirthDate)",
                "tgt.Name = src.Name, tgt.Document = src.Document, tgt.BirthDate = src.BirthDate, tgt.UpdatedDate = src.UpdatedDate, tgt.Status = 'UPDATED'",
                "src.Name, src.Document, src.BirthDate, src.UpdatedDate, 'INSERTED'"
            );

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntityStatus>()
                .UseStatusConfiguration(true, x => x.Status)
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => new { x.Name, x.Document, x.BirthDate, x.UpdatedDate })
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => new { x.Id, x.StatusStr })
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS ON) usando diferentes chamadas de metodos")]
        public async void Test_Ok_StatusString_DifMethods()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.GetWithStatus(10);
            var pkQuery = string.Format(_pkQuery, "PersonStatus");
            var createTempTableQuery = string.Format(_createTempTableQuery, "PersonStatus");
            var dropTempTableQuery = string.Format(_dropTemTableQuery, "PersonStatus");

            var mergeQuery = string.Format(_mergeQuery,
                "PersonStatus",
                "tgt.Document = src.Document",
                "AND (tgt.Name != src.Name)",
                "tgt.Name = src.Name, tgt.Document = src.Document, tgt.BirthDate = src.BirthDate, tgt.UpdatedDate = src.UpdatedDate, tgt.StatusStr = 'UPDATED'",
                "src.Name, src.Document, src.BirthDate, src.UpdatedDate, 'INSERTED'"
            );

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntityStatus>("PersonStatus")
                .UseStatusConfiguration(true, x => x.StatusStr)
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => new { x.Name, x.Document, x.BirthDate, x.UpdatedDate })
                .WithCondition(ConditionType.NOT_EQUAL, x => x.Name)
                .SetIgnoreOnIsertOperation(x => new { x.Id, x.Status })
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }

        [Fact(DisplayName = "Teste cenario correto (SNAKE CASE OFF | STATUS ON) usando diferentes chamadas de metodos (Condition Struct)")]
        public async void Test_Ok_StatusString_DifMethods_ConditionStruct()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.GetWithStatus(10);
            var pkQuery = string.Format(_pkQuery, "PersonStatus");
            var createTempTableQuery = string.Format(_createTempTableQuery, "PersonStatus");
            var dropTempTableQuery = string.Format(_dropTemTableQuery, "PersonStatus");

            var mergeQuery = string.Format(_mergeQuery,
                "PersonStatus",
                "tgt.Document = src.Document",
                "AND (tgt.BirthDate != src.BirthDate)",
                "tgt.Name = src.Name, tgt.Document = src.Document, tgt.BirthDate = src.BirthDate, tgt.UpdatedDate = src.UpdatedDate, tgt.StatusStr = 'UPDATED'",
                "src.Name, src.Document, src.BirthDate, src.UpdatedDate, 'INSERTED'"
            );

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntityStatus>("PersonStatus")
                .UseStatusConfiguration(true, x => x.StatusStr)
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => new { x.Name, x.Document, x.BirthDate, x.UpdatedDate })
                .WithCondition(ConditionType.NOT_EQUAL, x => x.BirthDate)
                .SetIgnoreOnIsertOperation(x => new { x.Id, x.Status })
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.True(result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }

        [Fact(DisplayName = "Teste cenario com erro (DbTransaction == null)")]
        public async void Test_Error_WithoutTransaction()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntity");

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntity>()
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource);

            //ACTION
            var result = await Assert.ThrowsAsync<ArgumentException>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You need to inform the DbTransaction, call the method SetTransaction(IDbTransaction transaction) before execute merge.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (DbTransaction.Connection == null)")]
        public async void Test_Error_WithoutTransactionConnection()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntity");

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .ReturnsForAnyArgs(x => null);

            var builder = _mergeBuilder.Create<PersonEntity>()
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<ArgumentException>(() => builder.Execute());

            //ASSERT
            Assert.Equal("The DbTransaction informed is without one active Connection.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (!MergedColumns.Any())")]
        public async void Test_Error_WithoutMergedColumns()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntity");

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntity>()
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<ArgumentException>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You need to inform the MergedColumns, call the method SetMergeColumns(...) before execute merge.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (!UpdatedColumns.Any())")]
        public async void Test_Error_WithoutUpdatedColumns()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntity");

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntity>()
                .SetMergeColumns(x => x.Document)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<ArgumentException>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You need to inform the UpdatedColumns, call the method SetUpdatedColumns(...) before execute merge.", result.Message);
        }

        [Fact(DisplayName = "Teste cenario com erro (!DataSource.Any())")]
        public async void Test_Error_WithoutDataSource()
        {
            //ARRANGE
            var dataSource = PersonEntityMock.Get(10);
            var pkQuery = string.Format(_pkQuery, "PersonEntity");

            _databaseService.ExecuteScalarCommand(Arg.Any<IDbTransaction>(), Arg.Is(pkQuery))
                .Returns("Id");

            _dbTransaction.Connection
                .Returns(Substitute.For<IDbConnection>());

            var builder = _mergeBuilder.Create<PersonEntity>()
                .SetMergeColumns(x => x.Document)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await Assert.ThrowsAsync<ArgumentException>(() => builder.Execute());

            //ASSERT
            Assert.Equal("You need to inform the DataSource, call the method SetDataSource(...) before execute merge.", result.Message);
        }

    }
}