using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaSaidasAPI
{
    public partial class ListaMovSaidas
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
        [JsonProperty("cod_operacao")]
        public int CodOperacao { get; set; }

        [JsonProperty("tipo_operacao")]
        public string TipoOperacao { get; set; }

        [JsonProperty("evento")]
        public int Evento { get; set; }

        [JsonProperty("romaneio")]
        public string Romaneio { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("cliente")]
        public int Cliente { get; set; }

        [JsonProperty("cod_endereco")]
        public int CodigoEndereco { get; set; }

        [JsonProperty("condicoes_pgto")]
        public string CondicoesPgto { get; set; }

        [JsonProperty("filial")]
        public string Filial { get; set; }

        [JsonProperty("conta")]
        public string Conta { get; set; }

        [JsonProperty("representante")]
        public string Representante { get; set; }

        [JsonProperty("transportadora")]
        public string Transportadora { get; set; }

        [JsonProperty("peso_b")]
        public string PesoB { get; set; }

        [JsonProperty("peso_l")]
        public string PesoL { get; set; }

        [JsonProperty("qtde")]
        public int Qtde { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("v_frete")]
        public string VFrete { get; set; }

        [JsonProperty("valor_juros")]
        public string ValorJuros { get; set; }

        [JsonProperty("valor_final")]
        public string ValorFinal { get; set; }

        [JsonProperty("especie_volume")]
        public string EspecieVolume { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }
    }

    public partial class ListaMovSaidas
    {
        public static ListaMovSaidas FromJson(string json) => JsonConvert.DeserializeObject<ListaMovSaidas>(json, ListaSaidasAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaMovSaidas self) => JsonConvert.SerializeObject(self, ListaSaidasAPI.Converter.Settings);
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
