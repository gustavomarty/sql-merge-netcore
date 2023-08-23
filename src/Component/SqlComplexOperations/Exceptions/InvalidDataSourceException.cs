namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidDataSourceException : Exception
    {
        public InvalidDataSourceException() { }
        public InvalidDataSourceException(string message) : base(message) { }
        public InvalidDataSourceException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class InvalidDataSourceException<T> : InvalidMergedColumnsException
        where T : class
    {
        public InvalidDataSourceException(string message, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergeBuilder = mergeBuilder;
        }

        public InvalidDataSourceException(string message, List<T> dataSource) : base(message)
        {
            DataSource = dataSource;
        }

        public InvalidDataSourceException(string message, List<T> dataSource, MergeBuilder<T> mergeBuilder) : base(message)
        {
            DataSource = dataSource;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public List<T>? DataSource { get; }
    }
}
