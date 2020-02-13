using ListaPedidosCompraAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.PedidoCompra
{
    class PedidoCompraGet
    {
        public PedidoCompraGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectPedidosCompra = ConfigurationManager.AppSettings["ConnectPedidosCompra"];
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
                MySqlConnection objConx = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx.Open();

                var command11 = objConx.CreateCommand();
                command11.CommandText = "SELECT COUNT(*) AS N FROM PEDIDO_COMPRA";
                var retCommnad11 = command11.ExecuteReaderAsync();
                string fault11 = retCommnad11.Status.ToString();

                if (fault11 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela Pedido de Compra...");
                    var CommandInsert11 = objConx.CreateCommand();
                    CommandInsert11.CommandText = "CREATE TABLE PEDIDO_COMPRA (PEDIDOC INT NOT NULL,COD_PEDIDOC VARCHAR(30),DATA_EMISSAO DATE,DATA_ENTREGA DATE,FORNECEDOR VARCHAR(255),CONDICOES_PGTO VARCHAR(255),"
                                                  + "FILIAL INT,APROVADO VARCHAR(1),EFETUADO VARCHAR(1),TIPO VARCHAR(5),QTDE_PEDIDA INT,QTDE_ENTREGAR INT,QTDE_ENTREGUE INT,QTDE_CANCELADA INT,VALOR_PEDIDO DECIMAL(14,2),"
                                                  + "VALOR_ENTREGAR DECIMAL(14,2),VALOR_ENTREGUE DECIMAL(14,2),VALOR_CANCELADO DECIMAL(14,2));";
                    CommandInsert11.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Pedido de Compra...");
                    var indicePedidoc = objConx.CreateCommand();
                    indicePedidoc.CommandText = "CREATE INDEX IDX_PEDIDO_COMPRA ON PEDIDO_COMPRA(PEDIDOC, FILIAL);";
                    indicePedidoc.ExecuteNonQuery();

                    objConx.Close();
                }
                objConx.Close();

                var requisicaoWeb11 = WebRequest.CreateHttp($"{ConnectPedidosCompra}" + "?$format=json");
                requisicaoWeb11.Method = "GET";
                requisicaoWeb11.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb11.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb11.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Pedidos de Compra...");
                using (var resposta11 = requisicaoWeb11.GetResponse())
                {
                    var streamDados11 = resposta11.GetResponseStream();
                    StreamReader reader11 = new StreamReader(streamDados11);
                    object objResponse11 = reader11.ReadToEnd();
                    var statusCodigo11 = ((System.Net.HttpWebResponse)resposta11).StatusCode;

                    ListaPedidosCompra pc = JsonConvert.DeserializeObject<ListaPedidosCompra>(objResponse11.ToString());

                    int pvv = 0;
                    Parallel.ForEach(pc.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, pvc =>
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
                                commandelet.CommandText = "DELETE FROM PEDIDO_COMPRA";
                                commandelet.ExecuteNonQuery();
                                pvv++;
                            }

                            string iDateEmissao = $"{pvc.DataEmissao}";
                            DateTime eDate = DateTime.Parse(iDateEmissao);
                            string DateEmi = (eDate.Year + "-" + eDate.Month + "-" + eDate.Day);

                            string iDateEntrega = $"{pvc.DataEntrega}";
                            DateTime gDate = DateTime.Parse(iDateEntrega);
                            string DateEnt = (gDate.Year + "-" + gDate.Month + "-" + gDate.Day);


                            var command011 = objConx011.CreateCommand();
                            command011.CommandText = "INSERT INTO PEDIDO_COMPRA (PEDIDOC,COD_PEDIDOC,DATA_EMISSAO,DATA_ENTREGA,FORNECEDOR,CONDICOES_PGTO,FILIAL,APROVADO,EFETUADO,TIPO,QTDE_PEDIDA,QTDE_ENTREGAR,QTDE_ENTREGUE,QTDE_CANCELADA,VALOR_PEDIDO,VALOR_ENTREGAR,VALOR_ENTREGUE,VALOR_CANCELADO)" +
                                                        $"VALUES({pvc.Pedidoc}," + $"\"{pvc.CodPedidoc}\"," + $"\"{DateEmi}\"," + $"\"{DateEnt}\"," + $"\"{pvc.Fornecedor}\"," + $"\"{pvc.CondicoesPgto}\"," + $"{pvc.Filial}," + $"\"{pvc.Aprovado}\"," + $"\"{pvc.Efetuado}\"," + $"\"{pvc.Tipo}\"," + $"{pvc.QtdePedida},"
                                                        + $"{pvc.QtdeEntregar}," + $"{pvc.QtdeEntregue}," + $"{pvc.QtdeCancelada}," + $"{pvc.ValorPedido}," + $"{pvc.ValorEntregar}," + $"{pvc.ValorEntregue}," + $"{pvc.ValorCancelado}" + ")";

                            command011.ExecuteNonQuery();
                            objConx011.Close();

                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine($"ID Pedido de Compra :  {pvc.Pedidoc} - Codigo Pedido: {pvc.CodPedidoc} - Erro: {e.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Pedido de Compra :  {pvc.Pedidoc} - Codigo Pedido: {pvc.CodPedidoc} - Erro: {e.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Pedido de Compra :  {pvc.Pedidoc} - Codigo Pedido: {pvc.CodPedidoc} - Erro: {e.Message}");
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
