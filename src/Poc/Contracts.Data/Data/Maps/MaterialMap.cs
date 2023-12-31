﻿using Contracts.Data.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Contracts.Data.Data.Maps
{
    public class MaterialMap : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(x => x.Nome)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Numero)
                .HasMaxLength(5)
                .IsRequired();

            builder.Property(x => x.DataAlteracao)
                .IsRequired();

            builder.HasIndex(x => x.Numero)
                .IsUnique();
        }
    }
}
