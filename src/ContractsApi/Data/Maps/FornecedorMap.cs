﻿using ContractsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractsApi.Data.Maps
{
    public class FornecedorMap : IEntityTypeConfiguration<Fornecedor>
    {
        public void Configure(EntityTypeBuilder<Fornecedor> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(x => x.Nome)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Cep)
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(x => x.Documento)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.DataAlteracao)
                .IsRequired();

            builder.HasIndex(x => x.Documento)
                .IsUnique();
        }
    }
}
