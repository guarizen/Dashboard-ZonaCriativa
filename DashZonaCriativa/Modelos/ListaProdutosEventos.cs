using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaProdutosEventosAPI
{
    public partial class ListaProdutosEventos
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
        [JsonProperty("produto_evento")]
        public string ProdutoEvento { get; set; }

        [JsonProperty("cod_operacao")]
        public string CodOperacao { get; set; }

        [JsonProperty("tipo_operacao")]
        public string TipoOperacao { get; set; }

        [JsonProperty("pedido")]
        public string Pedido { get; set; }

        [JsonProperty("pre_faturamento")]
        public string PreFaturamento { get; set; }

        [JsonProperty("produto")]
        public string Produto { get; set; }

        [JsonProperty("estampa")]
        public string Estampa { get; set; }

        [JsonProperty("cor")]
        public string Cor { get; set; }

        [JsonProperty("tamanho")]
        public string Tamanho { get; set; }

        [JsonProperty("quantidade")]
        public string Quantidade { get; set; }

        [JsonProperty("preco")]
        public string Preco { get; set; }

        [JsonProperty("desconto")]
        public string Desconto { get; set; }

        [JsonProperty("v_icmss")]
        public string VIcmss { get; set; }

        [JsonProperty("v_icms")]
        public string VIcms { get; set; }

        [JsonProperty("v_ipi")]
        public string VIpi { get; set; }

        [JsonProperty("v_iss")]
        public string VIss { get; set; }

        [JsonProperty("v_pis")]
        public string VPis { get; set; }

        [JsonProperty("v_confins")]
        public string VConfins { get; set; }
    }

    public partial class ListaProdutosEventos
    {
        public static ListaProdutosEventos FromJson(string json) => JsonConvert.DeserializeObject<ListaProdutosEventos>(json, ListaProdutosEventosAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaProdutosEventos self) => JsonConvert.SerializeObject(self, ListaProdutosEventosAPI.Converter.Settings);
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
