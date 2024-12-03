using Contracts.Data.Data;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SqlComplexOperations;
using SqlComplexOperations.Models.Output;
using SqlComplexOperations.Services;
using System.Data;

namespace Contracts.Test
{
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
            var builderMock = Substitute.ForPartsOf<MergeBuilder<Clube>>(databaseServiceMock);
            _mergeBuilderMock.Create<Clube>().Returns(builderMock);
            builderMock.Execute().Returns(Task.FromResult(new OutputModel()));

            //ACTION
            await _clubeService.Upsert(list);

            //ASSERT
            _unitOfWorkMock.Received(1).CommitTransaction();
        }
    }
}