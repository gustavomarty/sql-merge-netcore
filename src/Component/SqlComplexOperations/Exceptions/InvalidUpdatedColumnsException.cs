namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidUpdatedColumnsException : Exception
    {
        public InvalidUpdatedColumnsException() { }
        public InvalidUpdatedColumnsException(string message) : base(message) { }
        public InvalidUpdatedColumnsException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class InvalidUpdatedColumnsException<T> : InvalidMergedColumnsException
        where T : class
    {
        public InvalidUpdatedColumnsException(string message, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergeBuilder = mergeBuilder;
        }

        public InvalidUpdatedColumnsException(string message, List<string> updatedColumns) : base(message)
        {
            UpdatedColumns = updatedColumns;
        }

        public InvalidUpdatedColumnsException(string message, List<string> updatedColumns, MergeBuilder<T> mergeBuilder) : base(message)
        {
            UpdatedColumns = updatedColumns;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public List<string>? UpdatedColumns { get; }
    }
}
