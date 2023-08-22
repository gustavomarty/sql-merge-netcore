namespace SqlComplexOperations.Models.Output
{
    /// <summary>
    /// Classe de resultado com linhas afetadas (ResponseType.ROW_COUNT).
    /// </summary>
    public class OutputModelRowCount : OutputModel
    {
        /// <summary>
        /// Numero total de linhas afetadas pelo comando merge.
        /// </summary>
        public int RowsAffected { get; set; }
    }
}
