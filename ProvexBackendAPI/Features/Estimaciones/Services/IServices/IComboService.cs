using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IComboService
    {
        Task<List<ComboItemDto>> GetComboGenericoAsync(
           string nombreCombo,
            string codigoEmpresa);
    }
}
