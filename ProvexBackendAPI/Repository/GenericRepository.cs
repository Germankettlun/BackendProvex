using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProvexBackendAPI.Data;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Repository.IRepository;
using System.Data;
using System.Linq.Expressions;
using Serilog;

namespace ProvexBackendAPI.Repository
{
    public class GenericRepository : IGenericRepository
    {
        protected readonly AppDbContext _context;

        public GenericRepository(AppDbContext ctx)
        {
            _context = ctx;
        }

        /// <summary>
        /// metodo para agregar un registro en la base por Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task Add<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();
        }
        /// <summary>
        /// eliminar un registro de la base de datos por Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task Delete<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {

            return await _context.Set<TEntity>().AnyAsync(predicate);
        }

        /// <summary>
        /// traigo el primer registro de una consulta de una forma asincrona por el id por Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<TEntity> Find<TEntity>(object id) where TEntity : class
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }
        /// <summary>
        /// traigo el primer registro de una consulta de una forma asincrona por el predicado enviado
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<TEntity> FindPredicate<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }
        /// <summary>
        /// listo todos los registros de una tabla de forma asincrona por Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public async Task<List<TEntity>> GetAll<TEntity>() where TEntity : class
        {
            return await _context.Set<TEntity>().ToListAsync();
        }
        /// <summary>
        /// traigo el primer registro de una consulta de una forma asincrona por Entity de acuerdo a las condiciones
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<TEntity> GetFirst<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }
        /// <summary>
        /// listo todos los registros de una tabla de forma asincrona de acuerdo al predicado y por Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> GetList<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await _context.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public async Task<List<TEntity>> GetList<TEntity>(Expression<Func<TEntity, bool>> predicate, int? maxRecords = null) where TEntity : class
        {
            var query = _context.Set<TEntity>().Where(predicate);

            if (maxRecords.HasValue)
            {
                query = query.Take(maxRecords.Value);
            }

            return await query.ToListAsync();
        }
        public int GetCount<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return _context.Set<TEntity>().Where(predicate).Count();
        }

        /// <summary>
        /// consulta IQueryable generica para cualquier objeto por Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>().AsQueryable();
        }

        /// <summary>
        /// actualizar un registro
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task Update<TEntity>(TEntity entity, object id) where TEntity : class
        {
            var entidad = await _context.Set<TEntity>().FindAsync(id);
            if (entidad != null)
            {
                _context.Entry(entidad).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// consulta generica que nos sirve para obtener un objeto datatable
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<DataTable> GetDataTable(string query, SqlParameter[] parameters)
        {
            var dt = new DataTable();
            var conn = _context.Database.GetDbConnection();
            var connectionState = conn.State;
            try
            {
                // Log entrada SP lectura
                Log.Information("[DB_IN] SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                if (connectionState != ConnectionState.Open) await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                    
                }
                Log.Information("[DB_OUT] SP: {StoredProc} | Rows: {Rows}", query, dt.Rows.Count);
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "[DB_ERR] SQL Error in SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                throw;
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex, "[DB_ERR] Timeout in SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[DB_ERR] Exception in SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                throw;
            }
            finally
            {
                if (connectionState != ConnectionState.Closed) conn.Close();
            }
            return dt;
        }

        /// <summary>
        /// realiza un procedimiento de escritura en la base de datos
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task SpVoid(string query, SqlParameter[] parameters)
        {
            var conn = _context.Database.GetDbConnection();
            var connectionState = conn.State;
            try
            {
                Log.Information("[DB_IN] SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                if (connectionState != ConnectionState.Open) await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters);
                    await cmd.ExecuteNonQueryAsync();
                }
                Log.Information("[DB_OUT] SP: {StoredProc} | Status: OK", query);
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "[DB_ERR] SQL Error in SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                throw;
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex, "[DB_ERR] Timeout in SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[DB_ERR] Exception in SP: {StoredProc} | Params: {Params}", query, FormatParams(parameters));
                throw;
            }
            finally
            {
                if (connectionState != ConnectionState.Closed) conn.Close();
            }
        }

        private static string FormatParams(SqlParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return "<no-params>";
            try
            {
                return string.Join(", ", parameters.Select(p => $"{p.ParameterName}={(p.Value == null || p.Value == DBNull.Value ? "NULL" : p.Value)}"));
            }
            catch
            {
                return "<params-format-error>";
            }
        }
    }
}
