namespace SqlComplexOperations.Models.Output
{
    /// <summary>
    /// Classe de resultado especificando quantas linhas foram Inseridas|Atualizadas|Deletadas (ResponseType.SIMPLE).
    /// </summary>
    public class OutputModelSimple : OutputModel
    {
        /// <summary>
        /// Numero de linhas inseridas.
        /// </summary>
        public int Inserted { get; set; } = 0;

        /// <summary>
        /// Numero de linhas atualizadas.
        /// </summary>
        public int Updated { get; set; } = 0;

        /// <summary>
        /// Numero de linhas deletadas.
        /// </summary>
        public int Deleted { get; set; } = 0;

        /// <summary>
        /// Numero total de linhas afetadas.
        /// </summary>
        public int Total
        {
            get
            {
                return Inserted + Updated + Deleted;
            }
        }

    }
}
