using SqlComplexOperations.Models.Enumerators;

namespace SqlComplexOperations.Models.Output
{
    /// <summary>
    /// Classe de resultado completo (ResponseType.COMPLETE).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OutputModelComplete<T> : OutputModelSimple
        where T : class
    {
        /// <summary>
        /// Lista de todas as alterações ocorridas no comando merge.
        /// </summary>
        public List<OutputDataComplete<T>> Data { get; set; } = new();
    }

    /// <summary>
    /// Item de resultado de alterações ocorridas no comando merge.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OutputDataComplete<T>
        where T : class
    {
        /// <summary>
        /// Representa qual foi a action executada.
        /// </summary>
        public OutputAction Action { get; set; }

        /// <summary>
        /// Caso a action seja (INSERT ou UPDATE) esse dado é o dado novo, que foi mesclado (src).
        /// </summary>
        public T? InsertedData { get; set; }

        /// <summary>
        /// Caso a action seja (DELETE ou UPDATE) esse dado é o dado que já existia no database (tgt).
        /// </summary>
        public T? DeletedData { get; set; }
    }
}
