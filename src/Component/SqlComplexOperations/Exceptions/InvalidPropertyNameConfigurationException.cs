using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidPropertyNameConfigurationException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidPropertyNameConfigurationException(string message, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
    }
}
