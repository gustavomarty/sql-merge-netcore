using Bogus;
using SqlComplexOperations;
using SqlComplexOperations.Models.Enumerators;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Extensions;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Context = Contracts.Data.Data.ApplicationContext;

namespace Contracts.Service
{
    public class ClubeService : IClubeService
    {
        private readonly Context _context;
        private readonly IMergeBuilder _mergeBuilder;

        public ClubeService(Context context, IMergeBuilder mergeBuilder)
        {
            _context = context;
            _mergeBuilder = mergeBuilder;
        }

        public async Task CleanTable()
        {
            await _context.Database.ExecuteSqlRawAsync("delete from Clube");
        }

        public async Task Insert(Clube clube)
        {
            await _context.AddAsync(clube);
            await _context.SaveChangesAsync();
        }

        public async Task InsertRange(List<ClubeDto> clubesDto)
        {
            var clubes = GenerateClubeListFromClubeDtoList(clubesDto);

            await _context.AddRangeAsync(clubes);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ClubeDto clubeDto)
        {
            var clube = _context.Set<Clube>().FirstOrDefault(c => c.Nome.Equals(clubeDto.Nome));
            if (clube != null)
            {
                clube.DataAlteracao = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        public async Task Upsert(List<ClubeDto> clubesDto) 
        {
            var dataSource = GenerateClubeListFromClubeDtoList(clubesDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = await _mergeBuilder.Create<Clube>()
                .DeleteWhenDataIsNotInDataSource()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Nome)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => new { x.Abreviacao, x.Apelido })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }

        public async Task<List<Clube>> GetAll()
        {
            return await _context.Set<Clube>().ToListAsync();
        }
        public async Task<List<ClubeDto>> GetNewFakes(int qtd)
        {
            var faker = new Faker<ClubeDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Abreviacao, f => f.Company.CompanyName()[..3].ToUpper())
                .RuleFor(x => x.Apelido, string.Empty);

            var response = faker.Generate(qtd);

            var responseGroup = response.GroupBy(x => x.Nome)
                .Select(g => g.First());

            return responseGroup.ToList();
        }
        public async Task<List<ClubeDto>> GetMix(int qtd, bool withChanges)
        {
            var existingClubes = await _context.Set<Clube>().ToListAsync();

            var quantityForGenerateClubes = qtd / 2;
            var quantityForExistingClubes = qtd / 2;

            if (existingClubes.Count < quantityForGenerateClubes)
            {
                quantityForExistingClubes = existingClubes.Count;
                quantityForGenerateClubes += (quantityForGenerateClubes - existingClubes.Count);
            }

            var novosClubes = await GetNewFakes(quantityForGenerateClubes);
            existingClubes.Shuffle();

            existingClubes = existingClubes.Take(quantityForExistingClubes).ToList();


            List<ClubeDto> result = new();
            result.AddRange(novosClubes);
            result.AddRange(existingClubes.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new ClubeDto
                {
                    Nome = x.Nome,
                    Abreviacao = x.Abreviacao,
                    Apelido = changeItems ? $"ALTERADO: {x.Apelido}" : x.Apelido ?? string.Empty
                };
            }));

            return result;
        }
        private static List<Clube> GenerateClubeListFromClubeDtoList(List<ClubeDto> clubesDto)
        {
            return clubesDto.Select(x => new Clube
            {
                Nome = x.Nome,
                Abreviacao = x.Abreviacao,
                Apelido = x.Apelido,
                DataAlteracao = DateTime.Now
            }).ToList();
        }
    }
}