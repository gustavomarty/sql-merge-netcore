﻿// <auto-generated />
using System;
using Contracts.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Data.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20230802213733_AddFieldDataAlteracao")]
    partial class AddFieldDataAlteracao
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Contracts.Api.Data.Entities.Clube", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Abreviacao")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnName("abreviacao");

                    b.Property<string>("Apelido")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("apelido");

                    b.Property<DateTime>("DataAlteracao")
                        .HasColumnType("datetime2")
                        .HasColumnName("data_alteracao");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("nome");

                    b.HasKey("Id")
                        .HasName("pk_clube");

                    b.HasIndex("Nome")
                        .IsUnique()
                        .HasDatabaseName("ix_clube_nome");

                    b.ToTable("clube", (string)null);
                });

            modelBuilder.Entity("Contracts.Api.Data.Entities.Contrato", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DataAlteracao")
                        .HasColumnType("datetime2")
                        .HasColumnName("data_alteracao");

                    b.Property<string>("Descricao")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("descricao");

                    b.Property<DateTime>("Fim")
                        .HasColumnType("datetime2")
                        .HasColumnName("fim");

                    b.Property<int>("IdClube")
                        .HasColumnType("int")
                        .HasColumnName("id_clube");

                    b.Property<int>("IdFornecedor")
                        .HasColumnType("int")
                        .HasColumnName("id_fornecedor");

                    b.Property<int>("IdMaterial")
                        .HasColumnType("int")
                        .HasColumnName("id_material");

                    b.Property<DateTime>("Inicio")
                        .HasColumnType("datetime2")
                        .HasColumnName("inicio");

                    b.Property<string>("Numero")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)")
                        .HasColumnName("numero");

                    b.Property<decimal>("Preco")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("preco");

                    b.HasKey("Id")
                        .HasName("pk_contrato");

                    b.HasIndex("IdClube")
                        .HasDatabaseName("ix_contrato_id_clube");

                    b.HasIndex("IdFornecedor")
                        .HasDatabaseName("ix_contrato_id_fornecedor");

                    b.HasIndex("IdMaterial")
                        .HasDatabaseName("ix_contrato_id_material");

                    b.HasIndex("Numero")
                        .IsUnique()
                        .HasDatabaseName("ix_contrato_numero");

                    b.ToTable("contrato", (string)null);
                });

            modelBuilder.Entity("Contracts.Api.Data.Entities.Fornecedor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Cep")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("nvarchar(8)")
                        .HasColumnName("cep");

                    b.Property<DateTime>("DataAlteracao")
                        .HasColumnType("datetime2")
                        .HasColumnName("data_alteracao");

                    b.Property<string>("Documento")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("documento");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("nome");

                    b.HasKey("Id")
                        .HasName("pk_fornecedor");

                    b.HasIndex("Documento")
                        .IsUnique()
                        .HasDatabaseName("ix_fornecedor_documento");

                    b.ToTable("fornecedor", (string)null);
                });

            modelBuilder.Entity("Contracts.Api.Data.Entities.Material", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DataAlteracao")
                        .HasColumnType("datetime2")
                        .HasColumnName("data_alteracao");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("nome");

                    b.Property<string>("Numero")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)")
                        .HasColumnName("numero");

                    b.HasKey("Id")
                        .HasName("pk_material");

                    b.HasIndex("Numero")
                        .IsUnique()
                        .HasDatabaseName("ix_material_numero");

                    b.ToTable("material", (string)null);
                });

            modelBuilder.Entity("Contracts.Api.Data.Entities.Contrato", b =>
                {
                    b.HasOne("Contracts.Api.Data.Entities.Clube", "Clube")
                        .WithMany()
                        .HasForeignKey("IdClube")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_contrato_clube_clube_id");

                    b.HasOne("Contracts.Api.Data.Entities.Fornecedor", "Fornecedor")
                        .WithMany()
                        .HasForeignKey("IdFornecedor")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_contrato_fornecedor_fornecedor_id");

                    b.HasOne("Contracts.Api.Data.Entities.Material", "Material")
                        .WithMany()
                        .HasForeignKey("IdMaterial")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_contrato_material_material_id");

                    b.Navigation("Clube");

                    b.Navigation("Fornecedor");

                    b.Navigation("Material");
                });
#pragma warning restore 612, 618
        }
    }
}
