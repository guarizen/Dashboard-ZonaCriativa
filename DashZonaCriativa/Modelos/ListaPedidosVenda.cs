using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaPedidosVendaAPI
{
    public partial class ListaPedidosVenda
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
        [JsonProperty("pedidov")]
        public long Pedidov { get; set; }

        [JsonProperty("cod_pedidov")]
        public string CodPedidov { get; set; }

        [JsonProperty("tipo_pedido")]
        public long TipoPedido { get; set; }

        [JsonProperty("cliente")]
        public long Cliente { get; set; }

        [JsonProperty("cidade")]
        public string Cidade { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("cod_endereco")]
        public string Cod_Endereco { get; set; }

        [JsonProperty("representante")]
        public long? Representante { get; set; }

        [JsonProperty("data_emissao")]
        public string DataEmissao { get; set; }

        [JsonProperty("data_entrega")]
        public string DataEntrega { get; set; }

        [JsonProperty("orcamento")]
        public string Orcamento { get; set; }

        [JsonProperty("aprovado")]
        public string Aprovado { get; set; }

        [JsonProperty("efetuado")]
        public string Efetuado { get; set; }

        [JsonProperty("qtde_pedida")]
        public long QtdePedida { get; set; }

        [JsonProperty("qtde_entregar")]
        public long QtdeEntregar { get; set; }

        [JsonProperty("qtde_entregue")]
        public long QtdeEntregue { get; set; }

        [JsonProperty("qtde_cancelada")]
        public long QtdeCancelada { get; set; }

        [JsonProperty("valor_pedido")]
        public string ValorPedido { get; set; }

        [JsonProperty("valor_entregar")]
        public string ValorEntregar { get; set; }

        [JsonProperty("valor_entregue")]
        public string ValorEntregue { get; set; }

        [JsonProperty("valor_cancelado")]
        public string ValorCancelado { get; set; }
    }

    public partial class ListaPedidosVenda
    {
        public static ListaPedidosVenda FromJson(string json) => JsonConvert.DeserializeObject<ListaPedidosVenda>(json, ListaPedidosVendaAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaPedidosVenda self) => JsonConvert.SerializeObject(self, ListaPedidosVendaAPI.Converter.Settings);
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
