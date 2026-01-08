using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Data.Models;
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

    }
}
