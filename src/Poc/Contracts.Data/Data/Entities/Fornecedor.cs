﻿using Bulk.Models.Enumerators;

namespace Contracts.Data.Data.Entities
{
    public class Fornecedor
    {
        public Fornecedor()
        {
                
        }

        public Fornecedor(string nome, string documento, string cep) 
        {
            Nome = nome;
            Documento = documento;
            Cep = cep;
            Status = BulkStatus.INSERIDO;
        } 

        public int Id { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }
        public string Cep { get; set; }
        public DateTime DataAlteracao { get; set; }
        public BulkStatus Status { get; set; }
    }
}