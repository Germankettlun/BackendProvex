using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ProvexApi.Models.Reports;                    // EmbarquesRow, CtaCteRow
using CtaCteModel = ProvexApi.Models.Reports.CtaCteRow;

namespace ProvexApi.Services.Reports
{
    internal static class EmbarquesColumnMaps
    {
        internal const string DateFormat = "dd-MM-yyyy";
        internal const string NumFormat = "[$-en-US]#,##0.00";

        internal static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var formD = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc == UnicodeCategory.NonSpacingMark) continue;      // quita tildes
                if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))   // descarta . y -
                    sb.Append(ch);
            }
            return string.Join(" ", sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static readonly Dictionary<string, Func<EmbarquesRow, object?>> BaseMap =
            new(StringComparer.Ordinal)
            {
                ["codigotemporada"] = r => r.CodigoTemporada,
                ["grupo productor"] = r => r.GrupoProductor,
                ["grupoproductor"] = r => r.GrupoProductor,
                ["productor"] = r => r.NombreProductorReal,
                ["nombre productor real"] = r => r.NombreProductorReal,
                ["especie"] = r => r.Especie,
                ["nombre variedad"] = r => r.Variedad,
                ["variedad"] = r => r.Variedad,
                ["nombre variedad real"] = r => r.Variedad,
                ["mercado"] = r => r.Mercado,
                ["nombre recibidor"] = r => r.Recibidor,
                ["recibidor"] = r => r.Recibidor,
                ["codigo calibre"] = r => r.CodigoGrupoCalibre,
                ["grupo calibre"] = r => r.CodigoGrupoCalibre,
                ["calibre"] = r => r.CodigoCalibre,
                ["variedadeti"] = r => r.VariedadEti,
                ["peso neto"] = r => r.PesoEnvop,
                ["pesoenvop"] = r => r.PesoEnvop,
                ["peso neto real"] = r => r.PesoNetoReal,
                ["codigonave"] = r => r.CodigoNave,
                ["nave"] = r => r.CodigoNave + ' ' +  r.Nave,
                ["semanazarpe"] = r => r.SemanaZarpe,
                ["fecha zarpe"] = r => r.Fecha_Zarpe,
                ["fecha arribo"] = r => r.Fecha_Arribo,
                ["destino"] = r => r.Destino,
                ["tiponave"] = r => r.TipoNave,
                ["codigoenvop"] = r => r.CodigoEnvop,
                ["cod envop"] = r => r.CodigoEnvop,
                ["desc envop"] = r => r.DescEnvop,
                ["descenvop"] = r => r.DescEnvop,
                ["cajas bases"] = r => r.CajasEquivalentes,
                ["cajasbases"] = r => r.CajasEquivalentes,
                ["cajas"] = r => r.Cajas,
                ["condicion de llegada"] = r => r.CondicionLlegada,
                ["determinante de condicion"] = r => r.DeterminanteCondicion,
                ["estado liquidacion"] = r => r.EstadoLiquidacion,
                ["total retorno fob"] = r => r.TotalRetornoFOB,
                ["fob unitario bulto"] = r => r.FOBUnitarioBulto,
                ["comision"] = r => r.Comision,
                ["otros gastos"] = r => r.OtrosGastos,
                ["otrosgastos"] = r => r.OtrosGastos,
                ["total retorno"] = r => r.TotalRetorno,
                ["totalretorno"] = r => r.TotalRetorno,
                ["retorno unit cajas bulto"] = r => r.RetornoUnitCajasBulto,
                ["retorno unit cajas base"] = r => r.RetornoUnitCajasBase,
                ["total fob percibido"] = r => r.TotalFOBPercibido,
                ["fob unit percibido"] = r => r.FOBUnitPercibido,
                ["comision retorno"] = r => r.ComisionRetorno,
                ["otros costos"] = r => r.OtrosCostos,
                ["total retorno productor la fecha"] = r => r.TotalRetornoProductorLaFecha,
                ["total retorno productor a la fecha"] = r => r.TotalRetornoProductorLaFecha,
                ["retorno productor percibido caja bulto"] = r => r.RetornoProductorPercibidoCajaBulto,
                ["retorno productor percibido caja base"] = r => r.RetornoProductorPercibidoCajaBase,
                ["dif retorno estimado retorno percibido"] = r => r.Dif_Retorno_Estimado_Retorno_Percibido,
                ["fecha registro"] = r => r.Fecha_registro,
                ["fecha movimiento"] = r => r.Fecha_registro
            };

        internal static bool TryGetter(string header, out Func<EmbarquesRow, object?> getter)
        {
            var key = Normalize(header);
            if (BaseMap.TryGetValue(key, out getter)) return true;

            var pascal = string.Concat(key.Split(' ').Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
            var prop = typeof(EmbarquesRow).GetProperty(pascal);
            if (prop != null) { getter = r => prop.GetValue(r); return true; }

            getter = null!;
            return false;
        }
    }

    internal static class CtaCteColumnMaps
    {
        private static readonly Dictionary<string, Func<CtaCteModel, object?>> _getters =
            new(StringComparer.Ordinal)
            {
                [EmbarquesColumnMaps.Normalize("Productor")] = r => r.Productor,
                [EmbarquesColumnMaps.Normalize("Especie")] = r => r.Especie,
                [EmbarquesColumnMaps.Normalize("Fecha")] = r => r.Fecha,
                [EmbarquesColumnMaps.Normalize("Glosa")] = r => r.Glosa,
                [EmbarquesColumnMaps.Normalize("Item")] = r => r.Item,
                [EmbarquesColumnMaps.Normalize("SubItem")] = r => r.SubItem,
                [EmbarquesColumnMaps.Normalize("Tipo Cambio")] = r => r.TipoCambio,
                [EmbarquesColumnMaps.Normalize("T/Cambio")] = r => r.TipoCambio,
                [EmbarquesColumnMaps.Normalize("Dolar")] = r => r.Dolar,
                [EmbarquesColumnMaps.Normalize("Dólar (US$)")] = r => r.Dolar,
                [EmbarquesColumnMaps.Normalize("Pesos")] = r => r.Pesos,
                [EmbarquesColumnMaps.Normalize("Pesos ($)")] = r => r.Pesos,
                //[EmbarquesColumnMaps.Normalize("Saldo Dolar")] = r => r.SaldoDolar,
                //[EmbarquesColumnMaps.Normalize("Saldo Dólar (US$)")] = r => r.SaldoDolar,
                //[EmbarquesColumnMaps.Normalize("Saldo Pesos")] = r => r.SaldoPesos,
                //[EmbarquesColumnMaps.Normalize("Saldo Pesos ($)")] = r => r.SaldoPesos
            };

        public static bool TryGetter(string headerRaw, out Func<CtaCteModel, object?> getter)
            => _getters.TryGetValue(EmbarquesColumnMaps.Normalize(headerRaw), out getter!);
    }
}
