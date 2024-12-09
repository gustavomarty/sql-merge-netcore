namespace SqlComplexOperations.Models.Enumerators
{
    /// <summary>
    /// Define o tipo da base a ser utilizada.
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Caso você estiver usando uma base SqlServer.
        /// </summary>
        MICROSOFT_SQL_SERVER,

        /// <summary>
        /// Caso você estiver usando uma base Postgres.
        /// </summary>
        POSTGRES_SQL
    }
}
