using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaRepresentantesAPI
{

    public partial class ListaRepresentantes
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
        [JsonProperty("representante")]
        public long Representante { get; set; }

        [JsonProperty("cod_representante")]
        public object CodRepresentante { get; set; }

        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("fantasia")]
        public string Fantasia { get; set; }

        [JsonProperty("cgc")]
        public object Cgc { get; set; }

        [JsonProperty("cnpj")]
        public object Cnpj { get; set; }

        [JsonProperty("ie")]
        public object Ie { get; set; }

        [JsonProperty("ufie")]
        public object Ufie { get; set; }

        [JsonProperty("pf_pj")]
        public string PfPj { get; set; }

        [JsonProperty("e_mail")]
        public string EMail { get; set; }

        [JsonProperty("cod_endereco")]
        public string CodEndereco { get; set; }

        [JsonProperty("endereco")]
        public object Endereco { get; set; }

        [JsonProperty("bairro")]
        public object Bairro { get; set; }

        [JsonProperty("cidade")]
        public object Cidade { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("cep")]
        public object Cep { get; set; }

        [JsonProperty("pais")]
        public object Pais { get; set; }
    }

    public partial class ListaRepresentantes
    {
        public static ListaRepresentantes FromJson(string json) => JsonConvert.DeserializeObject<ListaRepresentantes>(json, ListaRepresentantesAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaRepresentantes self) => JsonConvert.SerializeObject(self, ListaRepresentantesAPI.Converter.Settings);
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
