using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidDataSourceException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidDataSourceException(string message, List<T> dataSource, string operation) : base(message)
        {
            DataSource = dataSource;
            Operation = operation;
        }

        public string Operation { get; }
        public List<T>? DataSource { get; }
    }
}
