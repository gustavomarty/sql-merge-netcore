using Bogus;
using Bulk;
using Bulk.Models.Enumerators;
using Contracts.Data.Data;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Extensions;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        public async Task CleanTable()
        {
            await _context.Database.ExecuteSqlRawAsync("delete from Clube");
        }
        public async Task InsertRange(List<TeamDto> teamsDto)
        {
            var teams = GenerateClubeListFromTeamDtoList(teamsDto);

            await _context.AddRangeAsync(teams);
            await _context.SaveChangesAsync();
        }
        public async Task Update(TeamDto teamDto)
        {
            var clube = _context.Set<Clube>().FirstOrDefault(c => c.Nome.Equals(teamDto.Nome));
            if (clube != null)
            {
                clube.DataAlteracao = DateTime.Now;

                await _context.SaveChangesAsync();
            }
        }
        public async Task Upsert(List<TeamDto> teamsDto) 
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
        public async Task<List<TeamDto>> GetNewFakes(int qtd)
        {
            return GetNewFakeTeams(qtd);
        }
        public async Task<List<TeamDto>> GetMix(int qtd, bool withChanges)
        {
            var existingTeams = await _context.Set<Clube>().ToListAsync();

            var quantityForGenerateTeams = qtd / 2;
            var quantityForExistingTeams = qtd / 2;

            if (existingTeams.Count < quantityForGenerateTeams)
            {
                quantityForExistingTeams = existingTeams.Count;
                quantityForGenerateTeams += (quantityForGenerateTeams - existingTeams.Count);
            }

            var newTeams = GetNewFakeTeams(quantityForGenerateTeams);
            existingTeams.Shuffle();

            existingTeams = existingTeams.Take(quantityForExistingTeams).ToList();


            List<TeamDto> result = new();
            result.AddRange(newTeams);
            result.AddRange(existingTeams.Select(x => {

                Random random = new();
                bool changeItems = withChanges && random.Next(100) < 40;

                return new TeamDto
                {
                    Nome = x.Nome,
                    Abreviacao = x.Abreviacao,
                    Apelido = changeItems ? $"ALTERADO: {x.Apelido}" : x.Apelido ?? string.Empty
                };
            }));

            return result;
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
        private List<TeamDto> GetNewFakeTeams(int quantity)
        {
            var faker = new Faker<TeamDto>("pt_BR")
                .RuleFor(x => x.Nome, f => f.Company.CompanyName())
                .RuleFor(x => x.Abreviacao, f => f.Company.CompanyName()[..3].ToUpper())
                .RuleFor(x => x.Apelido, string.Empty);

            var response = faker.Generate(quantity);

            var responseGroup = response.GroupBy(x => x.Nome)
                .Select(g => g.First());

            return responseGroup.ToList();
        }
    }
}