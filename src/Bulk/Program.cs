using System.Data;
using Bulk.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bulk
{
    public class Program
    {
        private static List<Time> _listaPayload = new();

        static void Main(string[] args)
        {
            PopulateNewTeams(2);

            using (var dbContext = new ApplicationContext())
            {
                using (var transaction = dbContext.Database.BeginTransaction().GetDbTransaction())
                {
                    ExecuteBuilderMerge(transaction);

                    var count = dbContext.Database.SqlQuery<int>($"select count(*) from #Time");
                    transaction.Commit();
                }
            }
        }

        private static void PopulateNewTeams(int quantityTeams)
        {
            string[] campeonatos = new string[] { "Brasileiro", "Copa do Brasil" }; 

            for(int i=0; i<quantityTeams; i++)
            {
                var time = new Time
                {
                    Campeonato = campeonatos[i%2],
                    DataAtualizacao = DateTime.Now,
                    Nome = $"Time {i}",
                    Titulos = i,
                    Derrotas = i,
                    Empates = i,
                    Jogos = i,
                    Participacoes = i,
                    Vitorias = i
                };

                _listaPayload.Add(time);
            }
        }

        private static void ExecuteBuilderMerge(IDbTransaction transaction)
        {
            var builder = new MergeBuilder<Time>()
                .SetDataSource(_listaPayload)
                .SetMergeColumns(x => new { x.Campeonato, x.Nome })
                .SetUpdatedColumns(x => new { x.DataAtualizacao, x.Campeonato, x.Nome, x.Titulos, x.Participacoes, x.Jogos, x.Vitorias, x.Derrotas, x.Empates })
                .SetTransaction(transaction)
                .Execute();
            

        }
    }
}