using ProvexBackendAPI.Data.Sql.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IComboRepository
    {
        Task<List<ComboItem>> LlenaComboGenericoAsync(ComboRequest req);


        Task<List<ComboItem>> LlenaComboEnvaseProductorEspecieVariedad(
            string idProductor,string idEspecie, string idVariedad);
    }
}
