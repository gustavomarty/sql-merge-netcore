using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Contracts.Data.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _applicationContext;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
                return;

            _transaction.Commit();
        }

        public IDbTransaction GetDbTransaction()
        {
            if (_transaction == null)
                CreateTransaction();

            return _transaction!.GetDbTransaction();
        }

        private void CreateTransaction()
        {
            _transaction = _applicationContext.Database.BeginTransaction();
        }
    }
}
