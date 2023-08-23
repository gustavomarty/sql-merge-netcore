using Contracts.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracts.Data.Data.Maps
{
    public class CampeonatoMap : IEntityTypeConfiguration<Campeonato>
    {
        public void Configure(EntityTypeBuilder<Campeonato> builder)
        {
            builder.ToTable($"test_{nameof(Campeonato)}", "test");

            builder.Property(x => x.Id)
                   .HasColumnName("idf_campeonato");

            builder.Property(x => x.UpdateAt)
                .HasColumnName("updatE_at");

            builder.Property(x => x.Nome)
                .HasColumnName("teStNoMe");

            builder.Property(x => x.Pais)
                .HasColumnName("test_pais");

            builder.Property(x => x.AnoFundacao)
                .HasColumnName("test_ano");

            builder.Property(x => x.Status)
                .HasColumnName("sts");

            builder.HasIndex(x => x.Id);
        }
    }
}
