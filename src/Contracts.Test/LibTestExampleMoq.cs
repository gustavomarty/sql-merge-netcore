using Contracts.Data.Data;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service;
using Microsoft.EntityFrameworkCore;
using Moq;
using SqlComplexOperations;
using SqlComplexOperations.Models.Enumerators;
using SqlComplexOperations.Models.Output;
using SqlComplexOperations.Services;
using System.Data;

namespace Contracts.Test
{
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
}