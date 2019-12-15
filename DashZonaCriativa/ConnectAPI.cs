using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using FaturamentoAutomatico;

namespace DashZonaCriativa
{
    class ConnectAPI
    {
        public string Numero { get; set; }
        public int Prefaturamento { get; set; }

        public ConnectAPI()
        {
            var ConnectData = ConfigurationManager.AppSettings["ConnectData"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];

            var requisicaoWeb = WebRequest.CreateHttp("http://189.113.4.250:888/api/millenium!pillow/prefaturamentos/lista_fat_auto" + "?$format=json");
            requisicaoWeb.Method = "GET";
            requisicaoWeb.Headers.Add("Authorization", "Basic YWRtaW5pc3RyYXRvcjp2dGFUUFJAMjAxOSoq");
            requisicaoWeb.UserAgent = "RequisicaoAPIGET";
            requisicaoWeb.Timeout = 1300000;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
        }
    }
}
