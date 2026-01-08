using ProvexBackendAPI.Dto;

namespace ProvexBackendAPI.Services.IServices
{
    public interface IComercial
    {
        Task<List<ComboItemDto>> ObtenerAgrupacionEspecieCalibre(RequestContextDTO contextDTO);
        Task<List<ComboItemDto>> ObtenerCalibres(string empresa, string especie);
    }
}
