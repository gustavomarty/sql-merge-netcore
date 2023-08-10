using Bulk.Models.Enumerators;
using Bulk.Services;
using Bulk.Tests.Models;
using NSubstitute;
using System.Data;
using Xunit;

namespace Bulk.Tests
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
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Name, x.BirthDate })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .SetDataSource(dataSource)
                .SetTransaction(_dbTransaction);

            //ACTION
            var result = await builder.Execute();

            //ASSERT
            Assert.NotNull(result);
            Assert.Equal("Deu boa!!", result);

            _databaseService.Received(1).ExecuteScalarCommand(Arg.Any<IDbTransaction>(), pkQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), createTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), dropTempTableQuery);
            _databaseService.Received(1).ExecuteNonQueryCommand(Arg.Any<IDbTransaction>(), mergeQuery);
        }
    }
}