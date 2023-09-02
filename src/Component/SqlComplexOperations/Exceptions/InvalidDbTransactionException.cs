using Azure;
using System.Data;
using System.Runtime.Serialization;

namespace SqlComplexOperations.Exceptions
{
    [Serializable]
    public class InvalidDbTransactionException<T> : Exception, ISerializable
        where T : class
    {
        public InvalidDbTransactionException(string message, string operation) : base(message)
        {
            Operation = operation;
        }

        public InvalidDbTransactionException(string message, IDbTransaction dbTransaction, string operation) : base(message)
        {
            DbTransaction = dbTransaction;
            Operation = operation;
        }

        public string Operation { get; }
        public IDbTransaction? DbTransaction { get; }
    }
}
