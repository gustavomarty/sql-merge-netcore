using Bulk.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bulk
{
    public class ApplicationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.;Initial Catalog=Fifa;Integrated Security=true;Encrypt=False;");
        }

        public DbSet<Time> Time { get; set; }
    }
}
