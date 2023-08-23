using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidDataSourceException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidDataSourceException(string message, List<T> dataSource, MergeBuilder<T> mergeBuilder) : base(message)
        {
            DataSource = dataSource;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public List<T>? DataSource { get; }
    }
}
