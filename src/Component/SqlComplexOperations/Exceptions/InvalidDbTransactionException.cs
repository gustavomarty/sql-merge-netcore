using System.Data;
using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidDbTransactionException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidDbTransactionException(string message, MergeBuilder<T> mergeBuilder) : base(message)
        {
            MergeBuilder = mergeBuilder;
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
