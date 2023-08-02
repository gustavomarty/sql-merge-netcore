using ContractsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractsApi.Data.Maps
{
    public class ClubeMap : IEntityTypeConfiguration<Clube>
    {
        public void Configure(EntityTypeBuilder<Clube> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(x => x.Nome)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Abreviacao)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.Apelido)
                .HasMaxLength(50);

            builder.HasIndex(x => x.Nome)
                .IsUnique();
        }
    }
}
