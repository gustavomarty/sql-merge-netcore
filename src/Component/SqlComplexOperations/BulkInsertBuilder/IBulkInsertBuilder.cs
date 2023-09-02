namespace SqlComplexOperations
{
    public interface IBulkInsertBuilder
    {
        /// <summary>
        /// Cria uma nova instancia do BulkInsertBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        BulkInsertBuilder<TEntity> Create<TEntity>() where TEntity : class;

        /// <summary>
        /// Cria uma nova instancia do BulkInsertBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o bulk insert será executado.
        /// </remarks>
        BulkInsertBuilder<TEntity> Create<TEntity>(string tableName) where TEntity : class;
    }
}