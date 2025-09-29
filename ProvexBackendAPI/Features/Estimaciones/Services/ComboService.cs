using ProvexBackendAPI.Features.Estimaciones.Dto.Combos;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Helpers.Validation;

namespace ProvexBackendAPI.Features.Estimaciones.Services
{
    public class ComboService : IComboService
    {
        private readonly IComboRepository _comboRepository;

        public ComboService(IComboRepository comboRepository)
        {
            _comboRepository = comboRepository;
        }
        public async Task<List<ComboItemDto>> GetComboGenericoAsync(ComboRequest req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            req.CodigoEmpresa = Guard.RequireAndUpper(req.CodigoEmpresa, nameof(req.CodigoEmpresa));
            req.NombreCombo = Guard.RequireAndUpper(req.NombreCombo, nameof(req.NombreCombo));
           

            var rows = await _comboRepository.LlenaComboGenericoAsync(req);

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

        public async Task<List<ComboItemDto>> GetComboEnvaseProductorEspecieVariedadAsync(string idProductor, string idEspecie, string idVariedad)
        {
            if (string.IsNullOrWhiteSpace(idProductor))
                throw new ArgumentException("El id de productor es requerido.", nameof(idProductor));
            if (string.IsNullOrWhiteSpace(idEspecie))
                throw new ArgumentException("El id de especie es requerido.", nameof(idEspecie));
            if (string.IsNullOrWhiteSpace(idEspecie))
                throw new ArgumentException("El id de variedad es requerido.", nameof(idVariedad));

            var rows = await _comboRepository.LlenaComboEnvaseProductorEspecieVariedad(idProductor, idEspecie, idVariedad);

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
