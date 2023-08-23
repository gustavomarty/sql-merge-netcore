namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidMergedColumnsException : Exception
    {
        public InvalidMergedColumnsException() { }
        public InvalidMergedColumnsException(string message) : base(message) { }
        public InvalidMergedColumnsException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class InvalidMergedColumnsException<T> : InvalidMergedColumnsException
        where T : class
    {
        public InvalidMergedColumnsException(string message, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergeBuilder = mergeBuilder;
        }

        public InvalidMergedColumnsException(string message, List<string> mergedColumns) : base(message)
        {
            MergedColumns = mergedColumns;
        }

        public InvalidMergedColumnsException(string message, List<string> mergedColumns, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergedColumns = mergedColumns;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public List<string>? MergedColumns { get; }
    }
}
