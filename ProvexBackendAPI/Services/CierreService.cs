using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using System.Data;

namespace ProvexBackendAPI.Services
{
    public class CierreService : ICierreService

    {
        private readonly IGenericRepository repository;

        public CierreService(IGenericRepository repository)
        {
            this.repository = repository;
        }
        public async Task<IReadOnlyList<CierreVersionDto>> GetListadoCierreVersion(string idEmpresa, string idTemporada, string? idEspecie, string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(idEmpresa))
                throw new ArgumentException("CodigoEmpresa es obligatorio.", nameof(idEmpresa));

            if (string.IsNullOrWhiteSpace(idTemporada))
                throw new ArgumentException("CodigoTemporada es obligatorio.", nameof(idTemporada));

            var parameters = new[]
            {
            new SqlParameter("@ID_EMPRESA",       SqlDbType.NVarChar, 10) { Value = idEmpresa.Trim().ToUpperInvariant() },
            new SqlParameter("@ID_TEMPORADA",     SqlDbType.VarChar,  10) { Value = idTemporada.Trim().ToUpperInvariant() },
            new SqlParameter("@ID_ESPECIE",     SqlDbType.NVarChar, 10) { Value = (object?)idEspecie?.Trim().ToUpperInvariant() ?? DBNull.Value },
            new SqlParameter("@DESCRIPCION", SqlDbType.VarChar, 200) { Value = (object?)descripcion?.Trim().ToUpperInvariant() ?? DBNull.Value }
            };

            var dataTable = await repository.GetDataTable("[Estimaciones].[usp_ESTIMACION_CIERRE_VERSION]", parameters);

            var result = new List<CierreVersionDto>();

            if (dataTable == null || dataTable.Rows.Count == 0)
                return result;

            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(new CierreVersionDto
                {
                    IdVersion = row.Field<int>("IDVERSION"),
                    IdEspecie = row.Field<string>("IDESPECIE")!,
                    Especie = row.Field<string>("ESPECIE")!,
                    Version = row.Field<string>("VERSION")!,
                    Descripcion = row.Field<string>("DESCRIPCION")!,
                    Fecha = row.Field<DateTime>("FECHA"),
                    IdUsuario = row.Field<Guid>("IDUSUARIO"), 
                    Usuario = row.Field<string>("USUARIO")!
                });
            }

            return result;
        }
    }
}
