using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;

namespace ProvexBackendAPI.Services
{
    public class EstimacionService : IEstimacionService
    {
        private readonly IGenericRepository repository;

        public EstimacionService(IGenericRepository repository)
        {
            this.repository = repository;
        }

        public async Task IngresarEstimacion(IngresarEstimacionRequest request)
        {
            try
            {
                int idUsuario = 3;

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@id_estimacion", request.idEstimacion ?? null),
                    new SqlParameter("@id_empresa", request.idEmpresa),
                    new SqlParameter("@id_temporada", request.idTemporada),
                    new SqlParameter("@id_especie", request.idEspecie),
                    new SqlParameter("@id_variedad", request.idVariedad),
                    new SqlParameter("@id_productor", request.idProductor),
                    new SqlParameter("@semana_inicio", request.semanaInicio),
                    new SqlParameter("@anio_inicio", request.anioInicio),
                    new SqlParameter("@porc_exportacion", request.porcExportacion),
                    new SqlParameter("@frigorifico", request.frigorifico),
                    new SqlParameter("@packing", request.packing),
                    new SqlParameter("@envase", request.envase),
                    new SqlParameter("@contratado", request.contratado),
                    new SqlParameter("@id_usuario", idUsuario)
                };
                    
                await repository.SpVoid("Estimaciones.sp_IngresarEstimacion", parameters);
                
            }
            catch (Exception)
            {
                throw new Exception("Error al crear estimación");
            }
        }

        public async Task IngresarPorcentajeExportacionSemanal(PorcentajeExportacionSemanalDTO input)
        {
            int idUsuario = 3;

            try
            {
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@idEstimacion", input.idEstimacion),
                    new SqlParameter("@anio", input.anio),
                    new SqlParameter("@semana", input.semana),
                    new SqlParameter("@porcentaje", input.porcentaje),
                    new SqlParameter("@idUsuario", idUsuario)
                };

                await repository.SpVoid("Estimaciones.usp_INSERT_UPDATE_Procentaje_Exportacion_Semanal", parameters);
            }
            catch (Exception)
            {
                throw new Exception("Error al actualizar el porcentaje semanal");
            }
        }

        public async Task<List<ZonaDTO>> ObtenerZonas(string codEmpresa)
        {
            try
            {
                var res = await repository.GetList<Zona>(z => z.idEmpresa == codEmpresa);

                List<ZonaDTO> zonas = new List<ZonaDTO>();
                
                zonas = [.. res.Select(item => new ZonaDTO
                {
                    idEmpresa = item.idEmpresa,
                    nombre = item.nombre
                })];

                return zonas;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
    }
}
