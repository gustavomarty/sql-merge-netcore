using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidUpdatedColumnsException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidUpdatedColumnsException(string message, List<string> updatedColumns, MergeBuilder<T> mergeBuilder) : base(message)
        {
            UpdatedColumns = updatedColumns;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public List<string>? UpdatedColumns { get; }
    }
}
