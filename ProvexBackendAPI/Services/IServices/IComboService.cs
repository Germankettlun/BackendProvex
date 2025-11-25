using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IComboService
    {
        Task<List<ComboItemDto>> GetComboGenericoAsync(ComboRequest req);
    }
}
