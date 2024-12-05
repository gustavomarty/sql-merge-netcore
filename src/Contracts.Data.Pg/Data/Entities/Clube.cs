﻿namespace Contracts.Data.Data.Entities
{
    public class Clube
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Apelido { get; set; }
        public string Abreviacao { get; set; }
        public DateTime DataAlteracao { get; set; }
    }
}
