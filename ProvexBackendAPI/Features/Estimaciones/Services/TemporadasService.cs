using ProvexBackendAPI.Features.Estimaciones.Dto.Temporadas;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public Task<List<SemanaDto>> GetSemanasTemporadaAsync(string codTem, string codEmp, int? vigente, string? semana = null, int? ano = null)
            => _repo.GetSemanasTemporadaAsync(codTem, codEmp, vigente, semana, ano);

        public Task<List<TemporadaDto>> ListAsync(string? codTem, string codEmp, int? vigente)
            => _repo.ListAsync(codTem, codEmp, vigente);
       
    }
}
