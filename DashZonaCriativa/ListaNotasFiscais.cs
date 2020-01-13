using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaNotasFiscaisAPI
{
    public partial class ListaNotasFicais
    {
        [JsonProperty("odata.metadata")]
        public Uri OdataMetadata { get; set; }

        [JsonProperty("odata.count")]
        public long OdataCount { get; set; }

        [JsonProperty("value")]
        public Value[] Value { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("cod_operacao")]
        public long CodOperacao { get; set; }

        [JsonProperty("tipo_operacao")]
        public string TipoOperacao { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("data_hora")]
        public string DataHora { get; set; }

        [JsonProperty("nota")]
        public string Nota { get; set; }

        [JsonProperty("serie")]
        public string Serie { get; set; }

        [JsonProperty("status")]
        public object Status { get; set; }

        [JsonProperty("valor")]
        public string Valor { get; set; }

        [JsonProperty("icms")]
        public string Icms { get; set; }

        [JsonProperty("v_icms")]
        public string VIcms { get; set; }

        [JsonProperty("icmss")]
        public string Icmss { get; set; }

        [JsonProperty("v_icmss")]
        public string VIcmss { get; set; }

        [JsonProperty("ipi")]
        public string Ipi { get; set; }

        [JsonProperty("v_ipi")]
        public string VIpi { get; set; }

        [JsonProperty("filial")]
        public string Filial { get; set; }

        [JsonProperty("idnfe")]
        public object Idnfe { get; set; }

        [JsonProperty("cidade")]
        public string Cidade { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }
    }

    public partial class ListaNotasFicais
    {
        public static ListaNotasFicais FromJson(string json) => JsonConvert.DeserializeObject<ListaNotasFicais>(json, ListaNotasFiscaisAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaNotasFicais self) => JsonConvert.SerializeObject(self, ListaNotasFiscaisAPI.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
