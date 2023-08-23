using System.Data;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidDbTransactionException : Exception
    {
        public InvalidDbTransactionException() { }
        public InvalidDbTransactionException(string message) : base(message) { }
        public InvalidDbTransactionException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class InvalidDbTransactionException<T> : InvalidDbTransactionException
        where T : class
    {
        public InvalidDbTransactionException(string message, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergeBuilder = mergeBuilder;
        }

        public InvalidDbTransactionException(string message, IDbTransaction dbTransaction) : base(message)
        {
            DbTransaction = dbTransaction;
        }

        public InvalidDbTransactionException(string message, IDbTransaction dbTransaction, MergeBuilder<T> mergeBuilder) : base(message)
        {
            DbTransaction = dbTransaction;
            MergeBuilder = mergeBuilder;
        }

        public MergeBuilder<T>? MergeBuilder { get; }
        public IDbTransaction? DbTransaction { get; }
    }
}
