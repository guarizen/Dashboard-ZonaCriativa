using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaProdutosAPI
{
    public partial class ListaProdutos
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
        [JsonProperty("produto")]
        public long Produto { get; set; }

        [JsonProperty("cod_produto")]
        public string CodProduto { get; set; }

        [JsonProperty("descricao1")]
        public string Descricao1 { get; set; }

        [JsonProperty("referencia")]
        public string Referencia { get; set; }

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

        [JsonProperty("cod_ncm")]
        public string CodNcm { get; set; }

        [JsonProperty("departamentos")]
        public string Departamentos { get; set; }

        [JsonProperty("divisao")]
        public string Divisao { get; set; }

        [JsonProperty("grupo")]
        public string Grupo { get; set; }

        [JsonProperty("tipo")]
        public string Tipo { get; set; }

        [JsonProperty("colecao")]
        public string Colecao { get; set; }

        [JsonProperty("subcolecao")]
        public string Subcolecao { get; set; }

        [JsonProperty("marca")]
        public string Marca { get; set; }

        [JsonProperty("categoria")]
        public string Categoria { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("qmm")]
        public long Qmm { get; set; }

        [JsonProperty("altura")]
        public string Altura { get; set; }

        [JsonProperty("largura")]
        public string Largura { get; set; }

        [JsonProperty("comprimento")]
        public string Comprimento { get; set; }

        [JsonProperty("peso")]
        public string Peso { get; set; }

        [JsonProperty("pedido_bloqueado")]
        public string PedidoBloqueado { get; set; }

        [JsonProperty("venda_bloqueado")]
        public string VendaBloqueado { get; set; }

        [JsonProperty("ean13")]
        public string Ean13 { get; set; }

        [JsonProperty("estoque_09")]
        public object Estoque09 { get; set; }

        [JsonProperty("estoque_11")]
        public object Estoque11 { get; set; }

        [JsonProperty("estoque_500")]
        public long Estoque500 { get; set; }
    }

    public partial class ListaProdutos
    {
        public static ListaProdutos FromJson(string json) => JsonConvert.DeserializeObject<ListaProdutos>(json, ListaProdutosAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaProdutos self) => JsonConvert.SerializeObject(self, ListaProdutosAPI.Converter.Settings);
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