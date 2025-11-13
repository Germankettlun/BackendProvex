using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;

namespace ProvexBackendAPI.Services
{
    public class EstimacionService : IEstimacionService
    {
        private readonly IGenericRepository<BaseEntity> repository;

        public EstimacionService(IGenericRepository<BaseEntity> repository)
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
    }
}
