using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidMergedColumnsException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidMergedColumnsException(string message, List<string> mergedColumns, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergedColumns = mergedColumns;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public List<string>? MergedColumns { get; }
    }
}
