using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaPedidosCompraAPI
{
    public partial class ListaPedidosCompra
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
        [JsonProperty("pedidoc")]
        public long Pedidoc { get; set; }

        [JsonProperty("cod_pedidoc")]
        public string CodPedidoc { get; set; }

        [JsonProperty("data_emissao")]
        public string DataEmissao { get; set; }

        [JsonProperty("data_entrega")]
        public string DataEntrega { get; set; }

        [JsonProperty("fornecedor")]
        public string Fornecedor { get; set; }

        [JsonProperty("condicoes_pgto")]
        public string CondicoesPgto { get; set; }

        [JsonProperty("filial")]
        public long Filial { get; set; }

        [JsonProperty("aprovado")]
        public string Aprovado { get; set; }

        [JsonProperty("efetuado")]
        public string Efetuado { get; set; }

        [JsonProperty("tipo")]
        public string Tipo { get; set; }

        [JsonProperty("qtde_pedida")]
        public long QtdePedida { get; set; }

        [JsonProperty("qtde_entregar")]
        public long QtdeEntregar { get; set; }

        [JsonProperty("qtde_entregue")]
        public long QtdeEntregue { get; set; }

        [JsonProperty("qtde_cancelada")]
        public long QtdeCancelada { get; set; }

        [JsonProperty("valor_pedido")]
        public long ValorPedido { get; set; }

        [JsonProperty("valor_entregar")]
        public long ValorEntregar { get; set; }

        [JsonProperty("valor_entregue")]
        public long ValorEntregue { get; set; }

        [JsonProperty("valor_cancelado")]
        public long ValorCancelado { get; set; }
    }

    public partial class ListaPedidosCompra
    {
        public static ListaPedidosCompra FromJson(string json) => JsonConvert.DeserializeObject<ListaPedidosCompra>(json, ListaPedidosCompraAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaPedidosCompra self) => JsonConvert.SerializeObject(self, ListaPedidosCompraAPI.Converter.Settings);
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
