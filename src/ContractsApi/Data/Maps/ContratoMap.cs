using ContractsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractsApi.Data.Maps
{
    public class ContratoMap : IEntityTypeConfiguration<Contrato>
    {
        public void Configure(EntityTypeBuilder<Contrato> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(x => x.IdClube)
                .IsRequired();

            builder.Property(x => x.IdFornecedor)
                .IsRequired();

            builder.Property(x => x.IdMaterial)
                .IsRequired();

            builder.Property(x => x.Descricao);

            builder.Property(x => x.Preco)
                .IsRequired();

            builder.Property(x => x.Inicio)
                .IsRequired();

            builder.Property(x => x.Fim)
                .IsRequired();

            builder.Property(x => x.Numero)
                .HasMaxLength(5)
                .IsRequired();

            builder.Property(x => x.DataAlteracao)
                .IsRequired();

            builder.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.IdMaterial);

            builder.HasOne(x => x.Fornecedor)
                .WithMany()
                .HasForeignKey(x => x.IdFornecedor);

            builder.HasOne(x => x.Clube)
                .WithMany()
                .HasForeignKey(x => x.IdClube);

            builder.HasIndex(x => x.Numero)
                .IsUnique();
        }
    }
}
