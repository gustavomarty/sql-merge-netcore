using Bulk.Models.Enumerators;
using Bulk;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Context = Contracts.Data.Data.ApplicationContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace Contracts.Service
{
    public class MaterialService : IMaterialService
    {
        private readonly Context _context;

        public MaterialService(Context context)
        {
            _context = context;
        }

        public Task Create(List<MaterialDto> materialDto)
        {
            throw new NotImplementedException();
        }

        public async Task CreateBulk(List<MaterialDto> materialDto)
        {
            var dataSource = GenerateMaterialListFromMaterialDtoList(materialDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Material>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Numero)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => x.Nome)
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }
        public async Task<List<Material>> GetAll()
        {
            return await _context.Set<Material>().ToListAsync();
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
    }
}