using ListaPedidosVendaAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.PedidosVenda
{
    class PedidoVendaGet
    {
        public PedidoVendaGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectPedidosVenda = ConfigurationManager.AppSettings["ConnectPedidosVenda"];
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
                MySqlConnection objConx111 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx111.Open();

                var command11 = objConx111.CreateCommand();
                command11.CommandText = "SELECT COUNT(*) AS N FROM PEDIDO_VENDA";
                var retCommnad11 = command11.ExecuteReaderAsync();
                string fault11 = retCommnad11.Status.ToString();

                if (fault11 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela Pedido de Venda...");
                    var CommandInsert11 = objConx111.CreateCommand();
                    CommandInsert11.CommandText = "CREATE TABLE PEDIDO_VENDA (PEDIDOV INT NOT NULL,COD_PEDIDOV VARCHAR(30),TIPO_PEDIDO INT,CLIENTE INT,CIDADE VARCHAR(255),ESTADO VARCHAR(5),COD_ENDERECO INT,REPRESENTANTE INT,DATA_EMISSAO DATE,DATA_ENTREGA DATE,ORCAMENTO VARCHAR(1),"
                                                 + "APROVADO VARCHAR(1),EFETUADO VARCHAR(1),QTDE_PEDIDA INT,QTDE_ENTREGAR INT,QTDE_ENTREGUE INT,QTDE_CANCELADA INT,VALOR_PEDIDO DECIMAL(14,2),VALOR_ENTREGAR DECIMAL(14,2),VALOR_ENTREGUE DECIMAL(14,2),VALOR_CANCELADO DECIMAL(14,2));";
                    CommandInsert11.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Pedido de Venda...");
                    var indicePedidov = objConx111.CreateCommand();
                    indicePedidov.CommandText = "CREATE INDEX IDX_PEDIDO_VENDA ON PEDIDO_VENDA(PEDIDOV,TIPO_PEDIDO,CLIENTE,REPRESENTANTE,COD_ENDERECO);";
                    indicePedidov.ExecuteNonQuery();

                    objConx111.Close();
                }
                objConx111.Close();

                var requisicaoWeb11 = WebRequest.CreateHttp($"{ConnectPedidosVenda}" + "?$format=json");
                requisicaoWeb11.Method = "GET";
                requisicaoWeb11.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb11.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb11.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Pedidos de Venda...");
                using (var resposta11 = requisicaoWeb11.GetResponse())
                {
                    var streamDados11 = resposta11.GetResponseStream();
                    StreamReader reader11 = new StreamReader(streamDados11);
                    object objResponse11 = reader11.ReadToEnd();
                    var statusCodigo11 = ((System.Net.HttpWebResponse)resposta11).StatusCode;

                    ListaPedidosVenda pv = JsonConvert.DeserializeObject<ListaPedidosVenda>(objResponse11.ToString());

                    int pvv = 0;
                    Parallel.ForEach(pv.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, pvp =>
                    {
                        try
                        {
                            MySqlConnection objConx011 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx011.Open();

                            while (pvv < 1)
                            {
                                if (pvv == 1)
                                    break;
                                var commandelet = objConx011.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PEDIDO_VENDA";
                                commandelet.ExecuteNonQuery();
                                pvv++;
                            }

                            string iDateEmissao = $"{pvp.DataEmissao}";
                            DateTime eDate = DateTime.Parse(iDateEmissao);
                            string DateEmi = (eDate.Year + "-" + eDate.Month + "-" + eDate.Day);

                            string iDateEntrega = $"{pvp.DataEntrega}";
                            DateTime gDate = DateTime.Parse(iDateEntrega);
                            string DateEnt = (gDate.Year + "-" + gDate.Month + "-" + gDate.Day);


                            var command011 = objConx011.CreateCommand();
                            command011.CommandText = "INSERT INTO PEDIDO_VENDA (PEDIDOV,COD_PEDIDOV,TIPO_PEDIDO,CLIENTE,CIDADE,ESTADO,COD_ENDERECO,REPRESENTANTE,DATA_EMISSAO,DATA_ENTREGA,ORCAMENTO,APROVADO,EFETUADO,QTDE_PEDIDA,QTDE_ENTREGAR,QTDE_ENTREGUE,QTDE_CANCELADA,VALOR_PEDIDO,VALOR_ENTREGAR,VALOR_ENTREGUE,VALOR_CANCELADO)" +
                                                        $"VALUES({pvp.Pedidov}," + $"\"{pvp.CodPedidov}\"," + $"{pvp.TipoPedido}," + $"{pvp.Cliente}," + $"\"{pvp.Cidade}\"," + $"\"{pvp.Estado}\"," + $"{pvp.Cod_Endereco}," + $"{pvp.Representante},"
                                                        + $"\"{DateEmi}\"," + $"\"{DateEnt}\"," + $"\"{pvp.Orcamento}\"," + $"\"{pvp.Aprovado}\"," + $"\"{pvp.Efetuado}\"," + $"{pvp.QtdePedida},"
                                                        + $"{pvp.QtdeEntregar}," + $"{pvp.QtdeEntregue}," + $"{pvp.QtdeCancelada}," + $"{pvp.ValorPedido}," + $"{pvp.ValorEntregar}," + $"{pvp.ValorEntregue}," + $"{pvp.ValorCancelado}" + ")";

                            command011.ExecuteNonQuery();
                            objConx011.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Pedido de Venda :  {pvp.Pedidov} - Codigo Pedido: {pvp.CodPedidov} - Erro: {etv.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Pedido de Venda :  {pvp.Pedidov} - Codigo Pedido: {pvp.CodPedidov} - Erro: {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Pedido de Venda :  {pvp.Pedidov} - Codigo Pedido: {pvp.CodPedidov} - Erro: {etv.Message}");
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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"{epp.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"{epp.Message}");
                    }
                }
            }
        }
    }
}
