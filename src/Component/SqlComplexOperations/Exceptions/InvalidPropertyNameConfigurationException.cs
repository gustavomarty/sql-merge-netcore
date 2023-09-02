using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidPropertyNameConfigurationException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidPropertyNameConfigurationException(string message, string operation) : base(message)
        {
            Operation = operation;
        }

        public string Operation { get; }
    }
}
