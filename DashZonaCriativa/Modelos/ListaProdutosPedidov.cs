﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaProdutosPedidovAPI
{
    public partial class ListaProdutosPedidov
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
        [JsonProperty("produto_pv")]
        public long ProdutoPv { get; set; }

        [JsonProperty("pedidov")]
        public long Pedidov { get; set; }

        [JsonProperty("produto")]
        public long Produto { get; set; }

        [JsonProperty("estampa")]
        public long Estampa { get; set; }

        [JsonProperty("cor")]
        public long Cor { get; set; }

        [JsonProperty("tamanho")]
        public string Tamanho { get; set; }

        [JsonProperty("preco")]
        public string Preco { get; set; }

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

    public partial class ListaProdutosPedidov
    {
        public static ListaProdutosPedidov FromJson(string json) => JsonConvert.DeserializeObject<ListaProdutosPedidov>(json, ListaProdutosPedidovAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaProdutosPedidov self) => JsonConvert.SerializeObject(self, ListaProdutosPedidovAPI.Converter.Settings);
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
