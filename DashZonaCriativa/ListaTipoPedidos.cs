using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaTiposPedidosAPI
{
    public partial class ListaTipoPedidos
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
        [JsonProperty("tipo_pedido")]
        public long TipoPedido { get; set; }

        [JsonProperty("descricao")]
        public string Descricao { get; set; }

        [JsonProperty("fixo")]
        public bool Fixo { get; set; }

        [JsonProperty("reducao_comissao")]
        public object ReducaoComissao { get; set; }

        [JsonProperty("comissao_minima")]
        public object ComissaoMinima { get; set; }

        [JsonProperty("pvadiantamento")]
        public bool Pvadiantamento { get; set; }

        [JsonProperty("tipopvadiantamento")]
        public object Tipopvadiantamento { get; set; }

        [JsonProperty("pvadiantamento_default")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long PvadiantamentoDefault { get; set; }

        [JsonProperty("orcamento")]
        public bool Orcamento { get; set; }

        [JsonProperty("permite_venda_sem_estoque")]
        public string PermiteVendaSemEstoque { get; set; }

        [JsonProperty("tipo")]
        public string Tipo { get; set; }

        [JsonProperty("referenciar_nf_saida")]
        public bool ReferenciarNfSaida { get; set; }

        [JsonProperty("desc_permite_venda_sem_estoque")]
        public string DescPermiteVendaSemEstoque { get; set; }

        [JsonProperty("dias_minimos_validade_estoque")]
        public object DiasMinimosValidadeEstoque { get; set; }
    }

    public partial class ListaTipoPedidos
    {
        public static ListaTipoPedidos FromJson(string json) => JsonConvert.DeserializeObject<ListaTipoPedidos>(json, ListaTiposPedidosAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaTipoPedidos self) => JsonConvert.SerializeObject(self, ListaTiposPedidosAPI.Converter.Settings);
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

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
