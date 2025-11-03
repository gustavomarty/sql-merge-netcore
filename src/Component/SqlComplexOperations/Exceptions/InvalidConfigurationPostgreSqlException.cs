using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidConfigurationPostgreSqlException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidConfigurationPostgreSqlException(string message, string operation) : base(message)
        {
            Operation = operation;
        }

        public string Operation { get; }
    }
}
