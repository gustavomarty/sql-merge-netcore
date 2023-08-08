using Bulk;
using Bulk.Models.Enumerators;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Context = Contracts.Data.Data.ApplicationContext;

namespace Contracts.Service
{
    public class TimeService : ITimeService
    {
        private readonly Context _context;

        public TimeService(Context context)
        {
            _context = context;
        }

        public async Task Create(List<TeamDto> teamsDto)
        {
        }

        public async Task CreateBulk(List<TeamDto> teamsDto) 
        {
            var dataSource = GenerateClubeListFromTeamDtoList(teamsDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Clube>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Nome)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Abreviacao, x.Apelido })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }

        public async Task<List<Clube>> GetAll()
        {
            return await _context.Set<Clube>().ToListAsync();
        }

        private static List<Clube> GenerateClubeListFromTeamDtoList(List<TeamDto> teamsDto)
        {
            return teamsDto.Select(x => new Clube
            {
                Nome = x.Nome,
                Abreviacao = x.Abreviacao,
                Apelido = x.Apelido,
                DataAlteracao = DateTime.Now
            }).ToList();
        }
    }
}