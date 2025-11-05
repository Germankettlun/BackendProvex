using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProvexBackendAPI.Data;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Repository.IRepository;
using System.Data;
using System.Linq.Expressions;

namespace ProvexBackendAPI.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _ctx;
        protected readonly DbSet<T> _set;

        public GenericRepository(AppDbContext ctx)
        {
            _ctx = ctx;
            _set = _ctx.Set<T>();
        }
        // ---- Lectura
        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _set.FindAsync([id], ct).AsTask();

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
            => await _set.AsNoTracking().ToListAsync(ct);

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

        public IQueryable<T> Query(bool asNoTracking = true)
            => asNoTracking ? _set.AsNoTracking() : _set.AsQueryable();

        // ---- Escritura
        public async Task<T> AddAsync(T entity, CancellationToken ct = default)
        {
            await _set.AddAsync(entity, ct);
            return entity;
        }

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _set.AddRangeAsync(entities, ct);

        public void Update(T entity) => _set.Update(entity);

        public void Remove(T entity)
        {
            // Soft-delete si el modelo hereda de BaseEntity; si no, hard-delete.
            if (entity is BaseEntity be)
            {
                be.IsDeleted = true;
                _set.Update(entity);
                return;
            }
            _set.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            foreach (var e in entities) Remove(e);
        }

        /// <summary>
        /// realiza un procedimiento de escritura en la base de datos
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task SpVoid(string query, SqlParameter[] parameters)
        {
            var conn = _ctx.Database.GetDbConnection();
            var connectionState = conn.State;
            try
            {
                if (connectionState != ConnectionState.Open) await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (connectionState != ConnectionState.Closed) conn.Close();
            }
        }
    }
}
