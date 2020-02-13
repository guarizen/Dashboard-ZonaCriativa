using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaPrefaturamentosAPI
{
    public partial class ListaPrefaturamentos
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
        [JsonProperty("prefaturamento")]
        public long Prefaturamento { get; set; }

        [JsonProperty("numero")]
        public string Numero { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("filial")]
        public long Filial { get; set; }

        [JsonProperty("cliente")]
        public long Cliente { get; set; }

        [JsonProperty("pedidov")]
        public long Pedidov { get; set; }

        [JsonProperty("expedicao")]
        public string Expedicao { get; set; }

        [JsonProperty("data_expedicao")]
        public object DataExpedicao { get; set; }

        [JsonProperty("podeconferir")]
        public string Podeconferir { get; set; }

        [JsonProperty("data_podeconferir")]
        public object DataPodeconferir { get; set; }

        [JsonProperty("conferindo")]
        public string Conferindo { get; set; }

        [JsonProperty("conferido")]
        public string Conferido { get; set; }

        [JsonProperty("dataconferido")]
        public object Dataconferido { get; set; }

        [JsonProperty("entregue")]
        public string Entregue { get; set; }

        [JsonProperty("transportadora")]
        public object Transportadora { get; set; }

        [JsonProperty("obs_cli_fat")]
        public object ObsCliFat { get; set; }
    }

    public partial class ListaPrefaturamentos
    {
        public static ListaPrefaturamentos FromJson(string json) => JsonConvert.DeserializeObject<ListaPrefaturamentos>(json, ListaPrefaturamentosAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaPrefaturamentos self) => JsonConvert.SerializeObject(self, ListaPrefaturamentosAPI.Converter.Settings);
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
