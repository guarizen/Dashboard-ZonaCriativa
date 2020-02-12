using ListaTiposPedidosAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.TiposPedidos
{
    class TipoPedidoGet
    {
        public TipoPedidoGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectTipoPedidos = ConfigurationManager.AppSettings["ConnectTipoPedidos"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];

            //Buscar se existe a pasta.
            string FileLog = @"C:\Dashboard-Log\";
            if (!File.Exists(FileLog))
            {
                //Criar Diretório de Log
                Directory.CreateDirectory(FileLog);
            }

            try
            {
                MySqlConnection objConx10 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx10.Open();

                var command10 = objConx10.CreateCommand();
                command10.CommandText = "SELECT COUNT(*) AS N FROM TIPOS_PEDIDO";
                var retCommnad10 = command10.ExecuteReaderAsync();
                string fault10 = retCommnad10.Status.ToString();

                if (fault10 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela Tipos Pedido...");
                    var CommandInsert10 = objConx10.CreateCommand();
                    CommandInsert10.CommandText = "CREATE TABLE TIPOS_PEDIDO (TIPO_PEDIDO INT NOT NULL,DESCRICAO VARCHAR(255));";
                    CommandInsert10.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Tipos Pedido...");
                    var indiceTipop = objConx10.CreateCommand();
                    indiceTipop.CommandText = "CREATE INDEX IDX_TIPOS_PEDIDO ON TIPOS_PEDIDO(TIPO_PEDIDO);";
                    indiceTipop.ExecuteNonQuery();

                    objConx10.Close();
                }
                objConx10.Close();

                var requisicaoWeb10 = WebRequest.CreateHttp($"{ConnectTipoPedidos}" + "?$format=json");
                requisicaoWeb10.Method = "GET";
                requisicaoWeb10.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb10.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb10.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Tipos Pedido...");
                using (var resposta10 = requisicaoWeb10.GetResponse())
                {
                    var streamDados10 = resposta10.GetResponseStream();
                    StreamReader reader10 = new StreamReader(streamDados10);
                    object objResponse10 = reader10.ReadToEnd();
                    var statusCodigo10 = ((System.Net.HttpWebResponse)resposta10).StatusCode;

                    ListaTipoPedidos tp = JsonConvert.DeserializeObject<ListaTipoPedidos>(objResponse10.ToString());

                    int tps = 0;
                    Parallel.ForEach(tp.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, ttp =>
                    {
                        try
                        {
                            MySqlConnection objConx010 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx010.Open();

                            while (tps < 1)
                            {
                                if (tps == 1)
                                    break;
                                var commandelet = objConx010.CreateCommand();
                                commandelet.CommandText = "DELETE FROM TIPOS_PEDIDO";
                                commandelet.ExecuteNonQuery();
                                tps++;
                            }

                            var command010 = objConx010.CreateCommand();
                            command010.CommandText = "INSERT INTO TIPOS_PEDIDO (TIPO_PEDIDO,DESCRICAO)" +
                                                        $"VALUES({ttp.TipoPedido}," + $"\"{ttp.Descricao}\")";

                            command010.ExecuteNonQuery();
                            objConx010.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Tipos Pedido :  {ttp.TipoPedido} - Descricao: {ttp.Descricao} - Erro: {etv.Message}");

                            //Verificando a pasta de Log. 
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            string path = @"C:\Dashboard-Log\" + DateTimeOffset.Now.ToString("ddMMyyyy") + ".log";

                            //Verifica se o arquivo de Log não existe e inclui as informações.
                            if (!File.Exists(path))
                            {
                                DirectoryInfo dir = new DirectoryInfo(FileLog);

                                foreach (FileInfo fi in dir.GetFiles())
                                {
                                    fi.Delete();
                                }

                                string nomeArquivo1 = @"C:\Dashboard-Log\" + DateTimeOffset.Now.ToString("ddMMyyyy") + ".log";
                                StreamWriter writer1 = new StreamWriter(nomeArquivo1);
                                writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Tipos Pedido :  {ttp.TipoPedido} - Descricao: {ttp.Descricao} - Erro: {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Tipos Pedido :  {ttp.TipoPedido} - Descricao: {ttp.Descricao} - Erro: {etv.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch (WebException epp)
            {
                Console.WriteLine(epp.Message);

                //Verificando a pasta de Log. 
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path = @"C:\Dashboard-Log\" + DateTimeOffset.Now.ToString("ddMMyyyy") + ".log";

                //Verifica se o arquivo de Log não existe e inclui as informações.
                if (!File.Exists(path))
                {
                    DirectoryInfo dir = new DirectoryInfo(FileLog);

                    foreach (FileInfo fi in dir.GetFiles())
                    {
                        fi.Delete();
                    }

                    string nomeArquivo1 = @"C:\Dashboard-Log\" + DateTimeOffset.Now.ToString("ddMMyyyy") + ".log";
                    StreamWriter writer1 = new StreamWriter(nomeArquivo1);
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{epp.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{epp.Message}");
                    }
                }
            }
        }
    }
}
