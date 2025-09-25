using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;

namespace ProvexBackendAPI.Features.Estimaciones.Services
{
    public class TemporadasService : ITemporadasService
    {
        private readonly ITemporadasRepository _repo;
        public TemporadasService(ITemporadasRepository repo) => _repo = repo;
        public Task<TemporadaDto?> GetByIdAsync(string codTem)
        {
            throw new NotImplementedException();
        }

        public Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem, string codEmp, int? vigente)
            => _repo.GetSemanasTemporadaAsync(codTem, codEmp, vigente);

        public Task<List<TemporadaDto>> ListAsync(string codEmp, int? vigente)
        {
            throw new NotImplementedException();
        }
    }
}
