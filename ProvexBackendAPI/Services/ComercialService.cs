using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
using ProvexBackendAPI.Data.Models.Users;
using ProvexBackendAPI.Dto;
using ProvexBackendAPI.Repository.IRepository;
using ProvexBackendAPI.Services.IServices;
using System.Data;

namespace ProvexBackendAPI.Services
{
    public class ComercialService : IComercial
    {
        private readonly IGenericRepository repository;

        public ComercialService(IGenericRepository repository) 
        {
            this.repository = repository;
        }
        public async Task<List<ComboItemDto>> ObtenerAgrupacionEspecieCalibre(RequestContextDTO contextDTO)
        {
            List<AgrupacionEspecieCalibre> listaAgrupacionEspecieCalibre = await repository.GetAll<AgrupacionEspecieCalibre>();

            List<ComboItemDto> responses = [];

            for (int i = 0; i < listaAgrupacionEspecieCalibre.Count; i++)
            {
                responses[i].Label = listaAgrupacionEspecieCalibre[i].descripcion;
                responses[i].Value = i.ToString();
            }

            return responses;
        }

        public async Task<List<ComboItemDto>> ObtenerCalibres(string empresa, string especie)
        {
            var parameters = new SqlParameter[]
            {
                new("@CodEmpresa", empresa),
                new("@CodEspecie", especie)
            };
            var res = await repository.GetDataTable("ProgramaComercial.sp_ObtenerCalibres", parameters);

            var listaGrupoCalibre = new List<ComboItemDto>();

            for (int i = 0; i < res.Rows.Count; i++)
            {
                ComboItemDto item = new()
                {
                    Label = res.Rows[i].Field<string>("CodigoGrupoCalibre") ?? "",
                    Value = res.Rows[i].Field<Int32>("IdGrupoCalibre").ToString() ?? i.ToString()
                };
                ;
                listaGrupoCalibre.Add(item);
                
            }

            return listaGrupoCalibre;
        }

        public async void CrearAgrupacion(CrearAgrupacionRequest request)
        {
            try
            {
                DataTable dt = new();

                dt.Columns.Add("ID_CALIBRE", typeof(string));

                foreach (var item in request.IdsCalibres)
                {
                    dt.Rows.Add(item.Value);
                }

                var parameters = new SqlParameter[]
                    {
                    new SqlParameter("@IdTemporada", request.idTemporada),
                    new SqlParameter("@codEmpresa", request.idEmpresa),
                    new SqlParameter("@IdEspecie", request.idEspecie),
                    new SqlParameter("@IdsCalibres", dt)
                    {
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.listaCalibres"
                    },
                    new SqlParameter("@NombreAgrupacion", request.descripcion)
                    };

                await repository.SpVoid("[ProgramaComercial].[sp_CrearAgrupacionDeCalibresPorEspecie]", parameters);
            }
            catch (Exception)
            {
                throw new Exception("Error al crear la agrupación.");
            }
            
        }

    }
}
