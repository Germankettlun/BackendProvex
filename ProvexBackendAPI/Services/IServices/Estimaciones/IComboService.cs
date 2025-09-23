using ProvexBackendAPI.Dto.Estimaciones.Combos;

namespace ProvexBackendAPI.Services.IServices.Estimaciones
{
    public interface IComboService
    {
        Task<List<ComboItemDto>> GetComboGenericoAsync(
           string nombreCombo,
            string codigoEmpresa);
    }
}
