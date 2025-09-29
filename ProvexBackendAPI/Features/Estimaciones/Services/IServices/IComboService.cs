using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface IComboService
    {
        Task<List<ComboItemDto>> GetComboGenericoAsync(ComboRequest req);

        Task<List<ComboItemDto>> GetComboEnvaseProductorEspecieVariedadAsync(
           string idProductor, string idEspecie, string idVariedad);
    }
}
