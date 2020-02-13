using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaFiliaisAPI
{
    public partial class ListaFiliais
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
        [JsonProperty("filial")]
        public long Filial { get; set; }

        [JsonProperty("cod_filial")]
        public string CodFilial { get; set; }

        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("fantasia")]
        public string Fantasia { get; set; }

        [JsonProperty("cgc")]
        public string Cgc { get; set; }

        [JsonProperty("cnpj")]
        public string Cnpj { get; set; }

        [JsonProperty("ie")]
        public string Ie { get; set; }

        [JsonProperty("ufie")]
        public string Ufie { get; set; }

        [JsonProperty("conta")]
        public object Conta { get; set; }

        [JsonProperty("tipo_empresa")]
        public string TipoEmpresa { get; set; }

        [JsonProperty("e_mail")]
        public string EMail { get; set; }

        [JsonProperty("endereco")]
        public string Endereco { get; set; }

        [JsonProperty("bairro")]
        public string Bairro { get; set; }

        [JsonProperty("cidade")]
        public string Cidade { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("cep")]
        public string Cep { get; set; }
    }

    public partial class ListaFiliais
    {
        public static ListaFiliais FromJson(string json) => JsonConvert.DeserializeObject<ListaFiliais>(json, ListaFiliaisAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaFiliais self) => JsonConvert.SerializeObject(self, ListaFiliaisAPI.Converter.Settings);
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

