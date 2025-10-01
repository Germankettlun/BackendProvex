using Microsoft.Data.SqlClient;
using ProvexBackendAPI.Features.Estimaciones.Dto.Estimaciones;
using ProvexBackendAPI.Features.Estimaciones.Repository.IRepository;
using ProvexBackendAPI.Features.Estimaciones.Services.IServices;
using ProvexBackendAPI.Helpers.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ProvexBackendAPI.Features.Estimaciones.Services
{
    public class EstimacionesService : IEstimacionesService
    {
        private readonly IEstimacionesRepository _EstimacionesRepository;
       
        public EstimacionesService(IEstimacionesRepository EstimacionesRepository)
        {
            _EstimacionesRepository = EstimacionesRepository;
        }
        public async Task<EstimacionDistribucionPorProductorDto> GetEstimacionBisemanalAsync(EstimacionesDto.EstimacionBisemanalQueryDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            req.CodEmpresa = Guard.RequireAndUpper(req.CodEmpresa, nameof(req.CodEmpresa));
            req.IdTemporada = Guard.RequireAndUpper(req.IdTemporada, nameof(req.IdTemporada));
            req.CodGrupoProductor = Guard.RequireAndUpper(req.CodGrupoProductor, nameof(req.CodGrupoProductor));
            req.IdEspecie = Guard.RequireAndUpper(req.IdEspecie, nameof(req.IdEspecie));

            if (req.AnioBase < 2000 || req.AnioBase > 2100)
                throw new ValidationException("Año fuera de rango");

            req.SemanaBase = (req.SemanaBase ?? string.Empty).Trim();
            var weekAttr = new WeekIsoStringAttribute();
            var valSemana = weekAttr.GetValidationResult(
                req.SemanaBase,
                new ValidationContext(req) { MemberName = nameof(req.SemanaBase) }
            );
            if (valSemana != ValidationResult.Success)
                throw new ValidationException(valSemana!.ErrorMessage!);

            req.Page = Guard.Require(nameof(req.Page), req.Page);
            req.WeeksPerPage = Guard.Require(nameof(req.WeeksPerPage), req.WeeksPerPage);


            var result = await _EstimacionesRepository.GetEstimacionBisemanalAsync(req);

            
            return result;
        }

        public async Task<List<EstimacionSemanalDto>> GetResumenSemanalAsync(string codigoEmpresa, string idTemporada, int idEstimacion)
        {
            codigoEmpresa = Guard.RequireAndUpper(codigoEmpresa, nameof(codigoEmpresa));
            idTemporada = Guard.RequireAndUpper(idTemporada, nameof(idTemporada));
            idEstimacion = Guard.Require(nameof(idEstimacion), idEstimacion);

            if (idEstimacion <= 0)
                throw new ArgumentException("El id de estimación debe ser mayor a 0", nameof(idEstimacion));

            return await _EstimacionesRepository.GetResumenSemanalAsync(codigoEmpresa, idTemporada, idEstimacion);


        }
    }
}
