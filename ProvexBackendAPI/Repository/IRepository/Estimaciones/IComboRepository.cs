using ProvexBackendAPI.Data.Sql.Estimaciones;

namespace ProvexBackendAPI.Repository.IRepository.Estimaciones
{
    public interface IComboRepository
    {
        Task<List<ComboItem>> LlenaComboGenericoAsync(
            string nombreCombo,
            string codigoEmpresa);
    }
}
