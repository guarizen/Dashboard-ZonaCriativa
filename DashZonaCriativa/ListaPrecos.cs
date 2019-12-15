using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaPrecosAPI
{

    public class ListaPrecosGet
    {
      
        [JsonProperty("odata.metadata")]
        public string OdataMetadata { get; set; }

        [JsonProperty("odata.count")]
        public int OdataCount { get; set; }

        [JsonProperty("value")]
        public Value[] Value { get; set; }
    }

    public class Value
    {

        [JsonProperty("produto")]
        public int Produto { get; set; }

        [JsonProperty("cod_produto")]
        public string CodProduto { get; set; }

        [JsonProperty("descricao1")]
        public string Descricao1 { get; set; }

        [JsonProperty("cod_est")]
        public string CodEst { get; set; }

        [JsonProperty("estampa")]
        public string Estampa { get; set; }

        [JsonProperty("cod_cor")]
        public string CodCor { get; set; }

        [JsonProperty("cor")]
        public string Cor { get; set; }

        [JsonProperty("tamanho")]
        public string Tamanho { get; set; }

        [JsonProperty("preco_214")]
        public string Preco214 { get; set; }

        [JsonProperty("preco_187")]
        public string Preco187 { get; set; }

        [JsonProperty("preco_224")]
        public string Preco224 { get; set; }

        [JsonProperty("preco_204")]
        public string Preco204 { get; set; }

        [JsonProperty("preco_205")]
        public string Preco205 { get; set; }
    }
    public partial class ListaPrecos
    {
        public static ListaPrecos FromJson(string json) => JsonConvert.DeserializeObject<ListaPrecos>(json, ListaPrecosAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaPrecos self) => JsonConvert.SerializeObject(self, ListaPrecosAPI.Converter.Settings);
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
