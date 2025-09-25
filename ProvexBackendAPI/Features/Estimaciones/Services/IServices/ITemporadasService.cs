using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;

namespace ProvexBackendAPI.Features.Estimaciones.Services.IServices
{
    public interface ITemporadasService
    {
        Task<List<TemporadaDto>> ListAsync(string codEmp, int? vigente);
        Task<TemporadaDto?> GetByIdAsync(string codTem);
        Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem, string codEmp, int? vigente, string? semana = null, int? ano = null);
    }
}
