using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CrossCuttingExtensions
{
    public interface IRepository<T, TEntity> : IDisposable where T : DbContext
    {
        void DetachEntries(ICollection<TEntity> entries);
        void DetachEntry(TEntity entry);
        Task<TEntity> GetFirstOrDefaultBy(Expression<Func<TEntity, bool>> predicate);
        Task<ICollection<TEntity>> GetManyBy(Expression<Func<TEntity, bool>> predicate);
        Task<ICollection<KEntity>> GetManyAndConvertBy<KEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, KEntity>> transformation, int skippedIndex = 0, int topResults = 0);
        void InsertEntry(TEntity entry);
        void InsertEntries(ICollection<TEntity> entries);
        void UpdateEntry(TEntity entry);
        void UpdateEntries(ICollection<TEntity> entries);
        void RemoveEntry(TEntity entry);
        void RemoveEntries(ICollection<TEntity> entries);
        IQueryable<TEntity> SetSkippedAndCappedQueryable(Expression<Func<TEntity, bool>> predicate, int skippedIndex = 0, int topResults = 0);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<ICollection<TEntity>> GetEntitiesByWith(Expression<Func<TEntity, bool>> predicate = null, int skippedIndex = 0, int topResults = 0, params Expression<Func<TEntity, object>>[] includes);
        void Dispose(bool disposeContext = false);
    }
}