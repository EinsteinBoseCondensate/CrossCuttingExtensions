using CrossCuttingExtensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace CrossCuttingExtensions.Implementations
{
    /// <summary>
    /// Generic repository for basic EF labours
    /// </summary>
    /// <typeparam name="T">DbContext associated to repository</typeparam>
    /// <typeparam name="TEntity">Entity within related DbContext</typeparam>
    public abstract class GenericRepo<T, TEntity> : IRepository<T, TEntity> where T : DbContext, new() where TEntity : class
    {
        #region Private fields
        private string Name => GetType().Name;
        private T Context { get; set; }
        private DbSet<TEntity> Set { get; set; }
        private bool disposed { get; set; } = false;
        private bool contextDisposed { get; set; } = false;
        private bool? UseLogging { get; set; }

        private ILogger Logger;
        #endregion

        #region Constructors
        public GenericRepo(ILoggerProvider loggerProvider, IConfiguration configuration)
        {
            ConfigLogging(configuration, loggerProvider);
            Context = new T();
            Set = Context.Set<TEntity>();
        }
        public GenericRepo(T Context, ILoggerProvider loggerProvider, IConfiguration configuration)
        {
            ConfigLogging(configuration, loggerProvider);
            this.Context = Context;
            Set = Context.Set<TEntity>();
        }

        #region Private methods
        private void ConfigLogging(IConfiguration configuration, ILoggerProvider loggerProvider)
        {
            UseLogging = configuration?.Get<LoggingConfig>()?.LoggingSection?.IsEnabled;
            if (UseLogging ?? false)
            {
                Logger = loggerProvider?.CreateLogger(Name) ?? throw new NullReferenceException("No ILoggerProvider implementation registered...");
            }
        }
        #endregion

        #endregion

        #region EF Methods
        /// <summary>
        /// Mark entries to be inserted in database
        /// </summary>
        /// <param name="entries">The entries to insert</param>
        public void InsertEntries(ICollection<TEntity> entries) => Set.AddRange(entries.AsEnumerable());
        /// <summary>
        /// Mark entry to be inserted in database
        /// </summary>
        /// <param name="entry">The entry to insert</param>
        public void InsertEntry(TEntity entry) => Set.Add(entry);
        /// <summary>
        /// Mark entries to be updated in database
        /// </summary>
        /// <param name="entries">The entries to update</param>
        public void UpdateEntries(ICollection<TEntity> entries) => Set.UpdateRange(entries.AsEnumerable());
        /// <summary>
        /// Mark entry to be updated in database
        /// </summary>
        /// <param name="entry">The entry to update</param>
        public void UpdateEntry(TEntity entry) => Set.Update(entry);
        /// <summary>
        /// Mark entries to be removed from database
        /// </summary>
        /// <param name="entries">The entries to be removed from database</param>
        public void RemoveEntries(ICollection<TEntity> entries) => Set.RemoveRange(entries.AsEnumerable());
        /// <summary>
        /// Mark entry to be removed from database
        /// </summary>
        /// <param name="entry">The entry to be removed from database</param>
        public void RemoveEntry(TEntity entry) => Set.Remove(entry);
        /// <summary>
        /// Mark entries as detached from EF tracking layer
        /// </summary>
        /// <param name="entries">The entries to be detached from EF tracking layer</param>
        public void DetachEntries(ICollection<TEntity> entries) => entries.ToList().ForEach(entry => DetachEntry(entry));
        /// <summary>
        /// Mark entry as detached from EF tracking layer
        /// </summary>
        /// <param name="entry">The entry to be detached from EF tracking layer</param>
        public void DetachEntry(TEntity entry) => Context.Entry(entry).State = EntityState.Detached;
        /// <summary>
        /// Get first result from linq query sentence async
        /// </summary>
        /// <param name="predicate">Linq query sentence</param>
        /// <returns>Task which gives a single instance of related entity type as result</returns>
        public async Task<TEntity> GetFirstOrDefaultBy(Expression<Func<TEntity, bool>> predicate) => await Set.FirstOrDefaultAsync(predicate);
        /// <summary>
        /// Gets boolean result from linq query sentence async
        /// </summary>
        /// <param name="predicate">Linq query sentence</param>
        /// <returns>boolean related to predicate parameter</returns>
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate) => await Set.AnyAsync<TEntity>(predicate);
        /// <summary>
        /// Get all results from linq query sentence
        /// </summary>
        /// <param name="predicate">Linq query sentence</param>
        /// <returns>Task which gives a collection of related entity type as result</returns>
        public async Task<ICollection<TEntity>> GetManyBy(Expression<Func<TEntity, bool>> predicate) => await SetQueryForMany(predicate).ToListAsync();
        public IQueryable<TEntity> SetSkippedAndCappedQueryable(Expression<Func<TEntity, bool>> predicate, int skippedIndex = 0, int topResults = 0) => SetQueryForMany(predicate).Skip(skippedIndex).Take(topResults);
        public async Task<ICollection<TEntity>> GetEntitiesByWith(Expression<Func<TEntity, bool>> predicate = null, int skippedIndex = 0, int topResults = 0, params Expression<Func<TEntity, object>>[] includes)            
        {
            IQueryable<TEntity> aux = predicate != null ? SetQueryForMany(predicate) : Set;
            if (skippedIndex != 0)
                aux = aux.Skip(skippedIndex);
            if (topResults != 0)
                aux = aux.Take(topResults);
            foreach (var inc in includes)
                aux = aux.Include(inc);            
            return await aux.ToListAsync(); ;
        }
        /// <summary>
        /// Unmaterialized query from associated set of related entity
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>IQueryable of related entity</returns>
        public IQueryable<TEntity> SetQueryForMany(Expression<Func<TEntity, bool>> predicate) => Set.Where(predicate);

        /// <summary>
        /// Load items into context, applies given transformation and return the collection async
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="transformation"></param>
        /// <returns>awaitable collection</returns>
        public async Task<ICollection<KEntity>> GetManyAndConvertBy<KEntity>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, KEntity>> transformation, int skippedIndex = 0, int topResults = 0)
        {
            IQueryable<TEntity> aux = SetQueryForMany(predicate);
            if (skippedIndex != 0)
                aux = aux.Skip(skippedIndex);
            if (topResults != 0)
                aux = aux.Take(topResults);

            try
            {
                await aux.LoadAsync();
            }
            catch (Exception e)
            {
                if (UseLogging ?? false)
                    Logger.LogError("Exception while attempting to load", e);
            }
            return aux.Select(transformation).ToList();
        }

        public void DisableLazyLoading() {
            Context.ChangeTracker.LazyLoadingEnabled = false;
        }
        public void EnableLazyLoading()
        {
            Context.ChangeTracker.LazyLoadingEnabled = true;
        }
        /// <summary>
        /// Save changes to database asynchronously
        /// </summary>
        /// <returns>Number of affected records</returns>
        public async Task<SaveChangesState> SaveChangesAsync()
        {
            try
            {
                await Context.SaveChangesAsync();
                return SaveChangesState.OK;
            }
            catch (Exception ex)
            {
                if (UseLogging ?? false)
                    Logger.LogError("Exception while saving changes", ex);
                return SaveChangesState.KO;
            }
        }
        #endregion

        #region Dispose
        /// <summary>
        /// To dispose only internal set, use Dispose(false). Call Dispose(true) to flush internal context also.
        /// </summary>
        /// <param name="disposeContext">True to dispose context along with asociated related entity's set, False to not.</param>
        public void Dispose(bool disposeContext = false)
        {
            if (!disposed || disposeContext)
            {
                if (!disposed)
                    Set = null;
                disposed = true;
                if (disposeContext)
                {
                    if (!contextDisposed)
                        Context = null;
                    contextDisposed = true;
                }
                GC.SuppressFinalize(this);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose() => Dispose(true);

        #endregion


    }
    public enum SaveChangesState
    {
        OK,
        KO
    }
}