namespace SqlComplexOperations.Models.Enumerators
{
    /// <summary>
    /// Define o tipo de resposta esperado depois da execução do bulk.
    /// </summary>
    public enum ResponseType
    {
        /// <summary>
        /// Sem nenhum retorno.
        /// </summary>
        NONE,

        /// <summary>
        /// Retorna o numero de linhas afetadas.
        /// </summary>
        ROW_COUNT,

        /// <summary>
        /// Retorna quantos registros foram atualizados, quantos foram inseridos e quantos foram deletados (Alem do total de registros alterados).
        /// </summary>
        SIMPLE,

        /// <summary>
        /// Retorna um de -> para dos registros atualizados, o registro inserido (Caso exista) e o registro deletado (Caso exista).
        /// </summary>
        COMPLETE
    }
}
