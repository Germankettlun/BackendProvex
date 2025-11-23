using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ProvexApi.Models.Rossi.RossiRoot;
using System.Text.Json.Serialization;
using ProvexApi.Models.SDT;
using ProvexApi.Converters;

namespace ProvexApi.Models.Rossi
{
    public class RossiRoot
    {

        public List<Documento> Documental { get; set; }

#nullable enable
        public class Documento
        {
            public int Id { get; set; }            // PK (no nullable)
            [JsonConverter(typeof(NumericToStringConverter))] public string? CodigoEmpresa { get; set; }
             [JsonConverter(typeof(NumericToStringConverter))] public string? CodigoTemporada { get; set; }
             [JsonConverter(typeof(NumericToStringConverter))] public string? despacho { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? referencia { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? booking { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? dusfecha { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? dusnro { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? dusdv { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? consignatario { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? puertoemb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? puertodes { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pais { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? programa { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? motonave { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? viaje { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tiponave { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechainicio { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horainicio { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechaestzarpe { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horaestzarpe { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechazarpe { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horazarpe { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechaenvioinforme { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horaenvioinforme { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? terminal { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? sitio { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_tipoflete { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_despacho_extranjero { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_despacho_nacional { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_etadestino { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_errorplanilla { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_certificado { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_factura { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_origen { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_correccionbl { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_bl { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_bltipo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_blnumero { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_fullset { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_vistobueno { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_observaciones { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? c_duslegalizada { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? aduananombre { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? aduanacodigo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? valorfob { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? valorliquidoretorno { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? valorflete { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? valorseguro { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? ws_bls { get; set; }

            // Sub-listas
            public List<Embarque>? Embarque { get; set; }
            public List<Facturacion>? Facturacion { get; set; }
            public List<Pallet>? Pallets { get; set; }

            // Archivos (del JSON) -> fullset / dus_legalizada
            public Archivos? Archivos { get; set; }

            // Relación con ArchivoUrl (guardado final en BD)
            public ICollection<ArchivoUrl>? ArchivoUrls { get; set; }

        }

#nullable enable

        public class Embarque
        {
            public int Id { get; set; }                   // PK
            public int DocumentoId { get; set; }          // FK
            public Documento? Documento { get; set; }      // Navegación

              [JsonConverter(typeof(NumericToStringConverter))] public string? envio { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? patente { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? transportista { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? condicion { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? nrocontenedor { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechaipto { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horaipto { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechatemb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horatemb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechacump { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horacump { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechaicon { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horaicon { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? fechatcon { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? horatcon { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? estado { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallemb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tcajas { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tkilosn { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tkilosb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tpallet { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? observaciones { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_latearrival { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_ingresomultipuerto { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_aforoaduana { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_consolidado { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_fzextraportuarias { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_inspeccionsag { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_instalacioncortina { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? costo_guiaingresadazeal { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tcontenedor { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? sello1 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? sello2 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? sello3 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? sello4 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tara { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? ventvalor { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? venttipo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tempsigno { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? tempvalor { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? temptipo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? atmosfera { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? atmco2 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? atmo2 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? etiqueta { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallet1 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? termografo1 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallet2 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? termografo2 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallet3 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? termografo3 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallet4 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? termografo4 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallet5 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? termografo5 { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? guia { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? packing { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? especie { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? variedad { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? atributo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? embalaje { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? embalajetipo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? embalajekn { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? embalajekb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? cajas { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pallet { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? pltxcjs { get; set; }
        }

        #nullable enable
        public class Facturacion
        {
            public int Id { get; set; }               // PK
            public int DocumentoId { get; set; }      // FK
            public Documento? Documento { get; set; } // Navegación

              [JsonConverter(typeof(NumericToStringConverter))] public string? f_factura { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_empresa { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_empresarut { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_empresarutdv { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_servicionombre { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_fechaemision { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_fechavencimiento { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_moneda { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_valorobservado { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_monto { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? f_cantidad { get; set; }
        }

#nullable enable
        public class Pallet
        {
            public int Id { get; set; }               // PK
            public int DocumentoId { get; set; }      // FK
            public Documento? Documento { get; set; }

              [JsonConverter(typeof(NumericToStringConverter))] public string? p_pallet { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_camara { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_nivel { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_contenedor { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_termografo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_temperatura { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_observaciones { get; set; }
            public List<PDetalle>? p_detalle { get; set; }
        }

        public class PDetalle
        {
            public int Id { get; set; }            // PK
            public int PalletId { get; set; }      // FK
            public Pallet? Pallet { get; set; }

              [JsonConverter(typeof(NumericToStringConverter))] public string? p_especie { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_variedad { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_atributo { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_cajas { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_etiqueta { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_calidad { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_calibre { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_fecha { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_productor { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_codembalaje { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_embalaje { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_kn { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_kb { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_embalajet { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_guia { get; set; }
              [JsonConverter(typeof(NumericToStringConverter))] public string? p_packing { get; set; }
        }

#nullable enable

        [NotMapped]
        public class Archivos
        {
            public ArchivoItem? fullset { get; set; }
            public ArchivoItem? dus_legalizada { get; set; }
        }

        [NotMapped]
        public class ArchivoItem
        {
            public List<string>? url { get; set; }
        }
        public class ArchivoUrl
        {
            public int Id { get; set; }           // PK
            public int DocumentoId { get; set; }  // FK
            public Documento? Documento { get; set; }

            // Indica si es 'fullset' o 'dus_legalizada'
            [JsonConverter(typeof(NumericToStringConverter))] public string? TipoArchivo { get; set; }

            // La URL concreta
            [JsonConverter(typeof(NumericToStringConverter))] public string? Url { get; set; }
        }

    }


  
}
