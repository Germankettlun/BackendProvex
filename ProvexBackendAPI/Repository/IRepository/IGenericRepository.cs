using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;

namespace ProvexBackendAPI.Repository.IRepository
{
    public interface IGenericRepository
    {
        Task<List<TEntity>> GetAll<TEntity>() where TEntity : class;
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class;
        Task<TEntity> GetFirst<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        Task<List<TEntity>> GetList<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        Task<List<TEntity>> GetList<TEntity>(Expression<Func<TEntity, bool>> predicate, int? maxRecords = null) where TEntity : class;
        Task Add<TEntity>(TEntity entity) where TEntity : class;
        Task Delete<TEntity>(TEntity entity) where TEntity : class;
        Task Update<TEntity>(TEntity entity, object id) where TEntity : class;
        Task<TEntity> Find<TEntity>(object id) where TEntity : class;
        Task<TEntity> FindPredicate<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        Task<bool> Exists<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        Task<DataTable> GetDataTable(string query, SqlParameter[] parameters);
        Task SpVoid(string query, SqlParameter[] parameters);
    }
}
