using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ListaClientesAPI
{

    public partial class ListaClientes
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
        [JsonProperty("cliente")]
        public long Cliente { get; set; }

        [JsonProperty("cod_cliente")]
        public string CodCliente { get; set; }

        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("fantasia")]
        public string Fantasia { get; set; }

        [JsonProperty("cgc")]
        public object Cgc { get; set; }

        [JsonProperty("cnpj")]
        public object Cnpj { get; set; }

        [JsonProperty("ie")]
        public object Ie { get; set; }

        [JsonProperty("ufie")]
        public object Ufie { get; set; }

        [JsonProperty("pf_pj")]
        public string PfPj { get; set; }

        [JsonProperty("tipo_empresa")]
        public string TipoEmpresa { get; set; }

        [JsonProperty("e_mail")]
        public string EMail { get; set; }

        [JsonProperty("e_mail_nfe")]
        public object EMailNfe { get; set; }

        [JsonProperty("grupo_loja")]
        public string GrupoLoja { get; set; }

        [JsonProperty("representante")]
        public string Representante { get; set; }

        [JsonProperty("cod_endereco")]
        public string CodEndereco { get; set; }

        [JsonProperty("endereco")]
        public object Endereco { get; set; }

        [JsonProperty("bairro")]
        public object Bairro { get; set; }

        [JsonProperty("cidade")]
        public object Cidade { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("cep")]
        public object Cep { get; set; }

        [JsonProperty("pais")]
        public object Pais { get; set; }
    }

    public partial class ListaClientes
    {
        public static ListaClientes FromJson(string json) => JsonConvert.DeserializeObject<ListaClientes>(json, ListaClientesAPI.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ListaClientes self) => JsonConvert.SerializeObject(self, ListaClientesAPI.Converter.Settings);
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

