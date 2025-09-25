using ProvexBackendAPI.Data.Sql.Estimaciones;

namespace ProvexBackendAPI.Features.Estimaciones.Repository.IRepository
{
    public interface IComboRepository
    {
        Task<List<ComboItem>> LlenaComboGenericoAsync(
            string nombreCombo,
            string codigoEmpresa);


        Task<List<ComboItem>> LlenaComboEnvaseProductorEspecieVariedad(
            string idProductor,string idEspecie, string idVariedad);
    }
}
