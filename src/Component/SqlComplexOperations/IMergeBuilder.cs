namespace SqlComplexOperations
{
    /// <summary>
    /// Interface para uso do merge builder
    /// </summary>
    public interface IMergeBuilder
    {
        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        MergeBuilder<TEntity> Create<TEntity>() where TEntity : class;

        /// <summary>
        /// Cria uma nova instancia do MergeBuilder.
        /// </summary>
        /// <param name="tableName">Caso necessario voce pode passar o tableName</param>
        /// <remarks>
        /// O Tipo <see cref="TEntity"/> é a entidade (Banco) onde o merge será executado.
        /// </remarks>
        MergeBuilder<TEntity> Create<TEntity>(string tableName) where TEntity : class;
    }
}
