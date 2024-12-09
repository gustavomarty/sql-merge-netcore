using System.Data;

namespace Contracts.Data.Data
{
    public interface IUnitOfWork
    {
        IDbTransaction GetDbTransaction();
        void CommitTransaction();
    }
}
