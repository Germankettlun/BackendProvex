using ProvexBackendAPI.Dto.Estimaciones.Combos;
using ProvexBackendAPI.Repository.IRepository.Estimaciones;
using ProvexBackendAPI.Services.IServices.Estimaciones;

namespace ProvexBackendAPI.Services.Estimaciones
{
    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;

        public ComboService(IComboRepository comboRepository)
        {
            _comboRepository = comboRepository;
        }
        public async Task<List<ComboItemDto>> GetComboGenericoAsync(string nombreCombo, string codigoEmpresa)
        {
            if (string.IsNullOrWhiteSpace(nombreCombo))
                throw new ArgumentException("El nombre del combo es requerido.", nameof(nombreCombo));
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                throw new ArgumentException("El codigo de empresa es requerido.", nameof(codigoEmpresa));

            var rows = await _comboRepository.LlenaComboGenericoAsync(nombreCombo,codigoEmpresa);

            var list = new List<ComboItemDto>(rows.Count);
            foreach (var r in rows)
            {
                list.Add(new ComboItemDto
                {
                    Value = r.Value,
                    Label = r.Label
                });
            }
            return list;
        }
    }
}
