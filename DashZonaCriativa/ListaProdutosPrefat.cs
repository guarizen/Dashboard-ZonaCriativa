using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaProdutosPrefatAPI
{
    public partial class ListaProdutosPrefat
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
        [JsonProperty("produto_prefat")]
        public long ProdutoPrefat { get; set; }

        [JsonProperty("prefaturamento")]
        public long Prefaturamento { get; set; }

        [JsonProperty("produto")]
        public long Produto { get; set; }

        [JsonProperty("estampa")]
        public long Estampa { get; set; }

        [JsonProperty("cor")]
        public long Cor { get; set; }

        [JsonProperty("tamanho")]
        public string Tamanho { get; set; }

        [JsonProperty("quantidade")]
        public long Quantidade { get; set; }

        [JsonProperty("entregue")]
        public long Entregue { get; set; }

        [JsonProperty("saida")]
        public object Saida { get; set; }
    }

    public partial class ListaProdutosPrefat
    {
        public static ListaProdutosPrefat FromJson(string json) => JsonConvert.DeserializeObject<ListaProdutosPrefat>(json, ListaProdutosPrefatAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaProdutosPrefat self) => JsonConvert.SerializeObject(self, ListaProdutosPrefatAPI.Converter.Settings);
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
