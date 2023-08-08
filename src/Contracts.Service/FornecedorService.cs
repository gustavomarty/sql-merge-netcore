using Bulk.Models.Enumerators;
using Bulk;
using Contracts.Data.Data.Entities;
using Contracts.Data.Models.Dtos;
using Contracts.Service.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Context = Contracts.Data.Data.ApplicationContext;
using Microsoft.EntityFrameworkCore;

namespace Contracts.Service
{
    public class FornecedorService : IFornecedorService
    {
        private readonly Context _context;

        public FornecedorService(Context context)
        {
            _context = context;
        }

        public Task Create(List<FornecedorDto> fornecedorDto)
        {
            throw new NotImplementedException();
        }

        public async Task CreateBulk(List<FornecedorDto> fornecedorDto)
        {
            var dataSource = GenerateFornecedorListFromFornecedorDtoList(fornecedorDto);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var builder = new MergeBuilder<Fornecedor>()
                .SetDataSource(dataSource)
                .SetTransaction(transaction.GetDbTransaction())
                .UseSnakeCaseNamingConvention()
                .SetMergeColumns(x => x.Documento)
                .SetUpdatedColumns(x => x)
                .WithCondition(ConditionTypes.NOT_EQUAL, ConditionOperator.OR, x => new { x.Cep, x.Nome })
                .SetIgnoreOnIsertOperation(x => x.Id)
                .Execute();

            transaction.Commit();
        }
        public async Task<List<Fornecedor>> GetAll()
        {
            return await _context.Set<Fornecedor>().ToListAsync();
        }

        private static List<Fornecedor> GenerateFornecedorListFromFornecedorDtoList(List<FornecedorDto> fornecedorsDto)
        {
            return fornecedorsDto.Select(x => new Fornecedor
            {
                Nome = x.Nome,
                Documento = x.Documento,
                Cep = x.Cep,
                DataAlteracao = DateTime.Now
            }).ToList();
        }
    }
}