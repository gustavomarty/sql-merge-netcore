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
    public class MaterialService : IMaterialService
    {
        private readonly Context _context;
        private readonly IMergeBuilder _mergeBuilder;

        public MaterialService(Context context, IMergeBuilder mergeBuilder)
        {
            _context = context;
            _mergeBuilder = mergeBuilder;
        }

        public async Task CleanTable()
        {
            await _context.Database.ExecuteSqlRawAsync("delete from Material");
        }
        public async Task InsertRange(List<MaterialDto> materialDto)
        {
            var materiais = GenerateMaterialListFromMaterialDtoList(materialDto);

            await _context.AddRangeAsync(materiais);
            await _context.SaveChangesAsync();
        }
        public async Task Update(MaterialDto materialDto)
        {
            var material = _context.Set<Material>().FirstOrDefault(m => m.Nome.Equals(materialDto.Nome));
            if (material != null)
            {
                material.DataAlteracao = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }

        public async Task Upsert(List<MaterialDto> materialDto)
        {
            var dataSource = GenerateMaterialListFromMaterialDtoList(materialDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = await _mergeBuilder.Create<Material>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Numero)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionType.NOT_EQUAL, ConditionOperator.OR, x => x.Nome)
                .SetIgnoreOnInsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }
        public async Task<List<Material>> GetAll()
        {
            return await _context.Set<Material>().ToListAsync();
        }
        public async Task<List<MaterialDto>> GetNewFakes(int qtd)
        {
            return GetNewFakeMaterials(qtd);
        }
        public async Task<List<MaterialDto>> GetMix(int qtd, bool withChanges)
        {
            var existingMaterials = await _context.Set<Material>().ToListAsync();

            var quantityForGenerateMaterials = qtd / 2;
            var quantityForExistingMaterials = qtd / 2;

            if (existingMaterials.Count < quantityForGenerateMaterials)
            {
                quantityForExistingMaterials = existingMaterials.Count;
                quantityForGenerateMaterials += (quantityForGenerateMaterials - existingMaterials.Count);
            }

            var newMaterials = GetNewFakeMaterials(quantityForGenerateMaterials);
            existingMaterials.Shuffle();

            existingMaterials = existingMaterials.Take(quantityForExistingMaterials).ToList();


            List<MaterialDto> result = new();
            result.AddRange(newMaterials);
            result.AddRange(existingMaterials.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new MaterialDto
                {
                    Nome = changeItems ? $"ALTERADO: {x.Nome}" : x.Nome,
                    Numero = x.Numero
                };
            }));

            return result;
        }

        private static List<Material> GenerateMaterialListFromMaterialDtoList(List<MaterialDto> materialsDto)
        {
            return materialsDto.Select(x => new Material
            {
                Nome = x.Nome,
                Numero = x.Numero,
                DataAlteracao = DateTime.Now
            }).ToList();
        }

        private List<MaterialDto> GetNewFakeMaterials(int quantity)
        {
            var faker = new Faker<MaterialDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Commerce.ProductName())
                .RuleFor(x => x.Numero, f => f.Random.Number(0, 99999).ToString().PadLeft(5, '0'));

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => x.Numero)
                .Select(g => g.First());

            return responseGroup.ToList();
        }
    }
}