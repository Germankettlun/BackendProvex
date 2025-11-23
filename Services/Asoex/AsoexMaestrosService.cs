using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProvexApi.Data;
using ProvexApi.Data.DTOs.ASOEX;
using ProvexApi.Entities.ASOEX;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public interface IAsoexMaestrosService
{
    Task<string> SincronizarTemporadasAsync(bool guardarEnBD = true);
    Task<string> SincronizarExportadoresAsync(bool guardarEnBD = true);
    Task<string> SincronizarSemanasAsync(bool guardarEnBD = true);
    Task<string> SincronizarConsignatariosAsync(bool guardarEnBD = true);
    Task<string> SincronizarRegionesOrigenAsync(bool guardarEnBD = true);
    Task<string> SincronizarRegionesDestinoAsync(bool guardarEnBD = true);
    Task<string> SincronizarPaisesDestinoAsync(bool guardarEnBD = true);
    Task<string> SincronizarPuertosEmbarqueAsync(bool guardarEnBD = true);
    Task<string> SincronizarPuertosDestinoAsync(bool guardarEnBD = true);
    Task<string> SincronizarTipoEspeciesAsync(bool guardarEnBD = true);
    Task<string> SincronizarEspeciesAsync(bool guardarEnBD = true);
    Task<string> SincronizarVariedadesAsync(bool guardarEnBD = true);
    Task<string> SincronizarNavesAsync(bool guardarEnBD = true);
}

public class AsoexMaestrosService : IAsoexMaestrosService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ProvexDbContext _db;
    private readonly ILogger<AsoexMaestrosService> _logger;

    public AsoexMaestrosService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ProvexDbContext db,
        ILogger<AsoexMaestrosService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _config = config;
        _db = db;
        _logger = logger;
    }

    private void ConfiguraAuthHeader()
    {
        var user = _config["Asoex:User"];
        var pass = _config["Asoex:Pass"];
        var credentials = $"{user}:{pass}";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {base64}");
    }

    private async Task<List<T>> ObtenerListaAsync<T>(string url)
    {
        ConfiguraAuthHeader();
        var resp = await _httpClient.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Error al obtener datos de {url}: {Status}", url, resp.StatusCode);
            throw new Exception($"API Error: {resp.StatusCode}");
        }
        var json = await resp.Content.ReadAsStringAsync();
        var lista = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return lista ?? new List<T>();
    }

    // ---- 1. TEMPORADAS ----
    public async Task<string> SincronizarTemporadasAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroTemporadas:url"];
        var lista = await ObtenerListaAsync<TemporadaDto>(url);

        if (!guardarEnBD)
            return $"Temporadas obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Temporada.ToList());

        var entidades = lista.Select(dto => new Temporada
        {
            TempSeqNro = dto.temp_seq_nro,
            TempFechaDesde = DateTime.Parse(dto.temp_fecha_desde),
            TempFechaHasta = DateTime.Parse(dto.temp_fecha_hasta),
            TempEstado = dto.temp_estado,
            NomTemporada = dto.nom_temporada
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Temporadas procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 2. EXPORTADORES ----
    public async Task<string> SincronizarExportadoresAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroExportadores:url"];
        var lista = await ObtenerListaAsync<ExportadorDto>(url);

        if (!guardarEnBD)
            return $"Exportadores obtenidos: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Exportador.ToList());

        var entidades = lista.Select(dto => new Exportador
        {
            RutEmpresa = dto.rut_empresa,
            DvEmpresa = dto.dv_empresa,
            NomEmpresa = dto.nom_empresa
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Exportadores procesados (insert masivo): {entidades.Count}";
    }

    // ---- 3. SEMANAS ----
    public async Task<string> SincronizarSemanasAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroSemanas:url"];
        var lista = await ObtenerListaAsync<SemanaDto>(url);

        if (!guardarEnBD)
            return $"Semanas obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Semana.ToList());

        var entidades = lista.Select(dto => new Semana
        {
            TempSeqNro = dto.temp_seq_nro,
            NroOtro = dto.nro_otro,
            Fecha1 = DateTime.Parse(dto.fecha_1),
            Fecha2 = DateTime.Parse(dto.fecha_2),
            NroSemana = dto.nro_semana
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Semanas procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 4. CONSIGNATARIOS ----
    public async Task<string> SincronizarConsignatariosAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroConsignatarios:url"];
        var lista = await ObtenerListaAsync<ConsignatarioDto>(url);

        if (!guardarEnBD)
            return $"Consignatarios obtenidos: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Consignatario.ToList());

        var entidades = lista.Select(dto => new Consignatario
        {
            CodConsignatario = dto.cod_consignatario,
            NomConsignatario = dto.nom_consignatario
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Consignatarios procesados (insert masivo): {entidades.Count}";
    }

    // ---- 5. REGIONES ORIGEN ----
    public async Task<string> SincronizarRegionesOrigenAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroRegionesorigen:url"];
        var lista = await ObtenerListaAsync<RegionOrigenDto>(url);

        if (!guardarEnBD)
            return $"Regiones Origen obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_RegionOrigen.ToList());

        var entidades = lista.Select(dto => new RegionOrigen
        {
            CodRegionOrigen = dto.cod_region_origen,
            NomRegion = dto.nom_region
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Regiones Origen procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 6. REGIONES DESTINO ----
    public async Task<string> SincronizarRegionesDestinoAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroRegionesdestino:url"];
        var lista = await ObtenerListaAsync<RegionDestinoDto>(url);

        if (!guardarEnBD)
            return $"Regiones Destino obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_RegionesDestino.ToList());

        var entidades = lista.Select(dto => new RegionDestino
        {
            CodRegionPais = dto.cod_region_pais,
            NomRegionPais = dto.nom_region_pais
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Regiones Destino procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 7. PAISES DESTINO ----
    public async Task<string> SincronizarPaisesDestinoAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroPaisesdestino:url"];
        var lista = await ObtenerListaAsync<PaisDestinoDto>(url);

        if (!guardarEnBD)
            return $"Países Destino obtenidos: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_PaisesDestino.ToList());

        var entidades = lista.Select(dto => new PaisDestino
        {
            CodPais = dto.cod_pais,
            NomPais = dto.nom_pais,
            CodRegionPais = dto.cod_region_pais
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Países Destino procesados (insert masivo): {entidades.Count}";
    }

    // ---- 8. PUERTOS EMBARQUE ----
    public async Task<string> SincronizarPuertosEmbarqueAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroPuertosembarque:url"];
        var lista = await ObtenerListaAsync<PuertoEmbarqueDto>(url);

        if (!guardarEnBD)
            return $"Puertos Embarque obtenidos: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_PuertoEmbarque.ToList());

        var entidades = lista.Select(dto => new PuertoEmbarque
        {
            CodPuerto = dto.cod_puerto,
            NomPuerto = dto.nom_puerto
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Puertos Embarque procesados (insert masivo): {entidades.Count}";
    }

    // ---- 9. PUERTOS DESTINO ----
    public async Task<string> SincronizarPuertosDestinoAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroPuertosdestino:url"];
        var lista = await ObtenerListaAsync<PuertoDestinoDto>(url);

        if (!guardarEnBD)
            return $"Puertos Destino obtenidos: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_PuertosDestino.ToList());

        var entidades = lista.Select(dto => new PuertoDestino
        {
            CodPuerto = dto.cod_puerto,
            NomPuerto = dto.nom_puerto,
            CodCosta = dto.cod_costa,
            CodPais = dto.cod_pais
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Puertos Destino procesados (insert masivo): {entidades.Count}";
    }

    // ---- 10. TIPO ESPECIES ----
    public async Task<string> SincronizarTipoEspeciesAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroTipoespecies:url"];
        var lista = await ObtenerListaAsync<TipoEspecieDto>(url);

        if (!guardarEnBD)
            return $"Tipo Especies obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_TipoEspecie.ToList());

        var entidades = lista.Select(dto => new TipoEspecie
        {
            CodTipoEpecie = dto.cod_tipo_epecie,
            NombreTipo = dto.nombre_tipo
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Tipo Especies procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 11. ESPECIES ----
    public async Task<string> SincronizarEspeciesAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroEspecies:url"];
        var lista = await ObtenerListaAsync<EspecieDto>(url);

        if (!guardarEnBD)
            return $"Especies obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Especie.ToList());

        var entidades = lista.Select(dto => new Especie
        {
            CodEspecie = dto.cod_especie,
            CodTipoEpecie = dto.cod_tipo_epecie,
            NomEspecie = dto.nom_especie
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Especies procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 12. VARIEDADES ----
    public async Task<string> SincronizarVariedadesAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroVariedades:url"];
        var lista = await ObtenerListaAsync<VariedadDto>(url);

        if (!guardarEnBD)
            return $"Variedades obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Variedad.ToList());

        var entidades = lista.Select(dto => new Variedad
        {
            CodVariedad = dto.cod_variedad,
            CodEspecie = dto.cod_especie,
            CodTipoEpecie = dto.cod_tipo_epecie,
            NomVariedad = dto.nom_variedad
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Variedades procesadas (insert masivo): {entidades.Count}";
    }

    // ---- 13. NAVES ----
    public async Task<string> SincronizarNavesAsync(bool guardarEnBD = true)
    {
        var url = _config["Asoex:Apis:MaestroNaves:url"];
        var lista = await ObtenerListaAsync<NaveDto>(url);

        if (!guardarEnBD)
            return $"Naves obtenidas: {lista.Count}";

        await _db.BulkDeleteAsync(_db.Asoex_Nave.ToList());

        var entidades = lista.Select(dto => new Nave
        {
            CodNave = dto.cod_nave,
            NomNave = dto.nom_nave,
            TipoNave = dto.tipo_nave
        }).ToList();

        await _db.BulkInsertAsync(entidades);

        return $"Naves procesadas (insert masivo): {entidades.Count}";
    }
}
