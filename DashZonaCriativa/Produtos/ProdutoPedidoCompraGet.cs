using ListaProdutosPedidocAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Produtos
{
    class ProdutoPedidoCompraGet
    {
        public ProdutoPedidoCompraGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectProdutosPedidoc = ConfigurationManager.AppSettings["ConnectProdutosPedidoc"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];
            var Data_Inicial = ConfigurationManager.AppSettings["Data_Inicial"];
            var Data_Final = ConfigurationManager.AppSettings["Data_Final"];

            //Buscar se existe a pasta.
            string FileLog = @"C:\Dashboard-Log\";
            if (!File.Exists(FileLog))
            {
                //Criar Diretório de Log
                Directory.CreateDirectory(FileLog);
            }

            string iDateIni = $"{Data_Inicial}";
            DateTime oDate = DateTime.Parse(iDateIni);
            string DateIni = (oDate.Year + "-" + oDate.Month + "-" + oDate.Day);

            string iDateFim = $"{Data_Final}";
            DateTime oDateF = DateTime.Parse(iDateFim);
            string DateFim = (oDateF.Year + "-" + oDateF.Month + "-" + oDateF.Day);

            try
            {
                MySqlConnection objConx = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx.Open();

                var command11 = objConx.CreateCommand();
                command11.CommandText = "SELECT COUNT(*) AS N FROM PRODUTO_PEDIDOC";
                var retCommnad11 = command11.ExecuteReaderAsync();
                string fault11 = retCommnad11.Status.ToString();

                if (fault11 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela Produto Pedido de Compra...");
                    var CommandInsert11 = objConx.CreateCommand();
                    CommandInsert11.CommandText = "CREATE TABLE PRODUTO_PEDIDOC (PRODUTO_PC INT NOT NULL,PEDIDOC INT,PRODUTO INT,ESTAMPA INT,COR INT,TAMANHO VARCHAR(5),PRECO DECIMAL(14,2),QTDE_PEDIDA INT,QTDE_ENTREGAR INT,QTDE_ENTREGUE INT,QTDE_CANCELADA INT,VALOR_PEDIDO DECIMAL(14,2),"
                                               + "VALOR_ENTREGAR DECIMAL(14,2),VALOR_ENTREGUE DECIMAL(14,2),VALOR_CANCELADO DECIMAL(14,2));";
                    CommandInsert11.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Produto Pedido de Compra...");
                    var indicePedidoc = objConx.CreateCommand();
                    indicePedidoc.CommandText = "CREATE INDEX IDX_PRODUTO_PEDIDOC ON PRODUTO_PEDIDOC(PRODUTO_PC,PEDIDOC,PRODUTO,ESTAMPA,COR,TAMANHO);";
                    indicePedidoc.ExecuteNonQuery();

                    objConx.Close();
                }
                objConx.Close();

                var requisicaoWeb11 = WebRequest.CreateHttp($"{ConnectProdutosPedidoc}" + $"?data_inicial={DateIni}" + $"&data_final={DateFim}" + "&$format=json");
                requisicaoWeb11.Method = "GET";
                requisicaoWeb11.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb11.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb11.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Produtos Pedidos de Compra...");
                using (var resposta11 = requisicaoWeb11.GetResponse())
                {
                    var streamDados11 = resposta11.GetResponseStream();
                    StreamReader reader11 = new StreamReader(streamDados11);
                    object objResponse11 = reader11.ReadToEnd();
                    var statusCodigo11 = ((System.Net.HttpWebResponse)resposta11).StatusCode;

                    ListaProdutosPedidoc ppc = JsonConvert.DeserializeObject<ListaProdutosPedidoc>(objResponse11.ToString());

                    int pvv = 0;
                    Parallel.ForEach(ppc.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, pcp =>
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
                                commandelet.CommandText = "DELETE FROM PRODUTO_PEDIDOC";
                                commandelet.ExecuteNonQuery();
                                pvv++;
                            }

                            var command011 = objConx011.CreateCommand();
                            command011.CommandText = "INSERT INTO PRODUTO_PEDIDOC (PRODUTO_PC,PEDIDOC,PRODUTO,ESTAMPA,COR,TAMANHO,PRECO,QTDE_PEDIDA,QTDE_ENTREGAR,QTDE_ENTREGUE,QTDE_CANCELADA,VALOR_PEDIDO,VALOR_ENTREGAR,VALOR_ENTREGUE,VALOR_CANCELADO)" +
                                                        $"VALUES({pcp.ProdutoPc}," + $"{pcp.Pedidoc}," + $"{pcp.Produto}," + $"{pcp.Estampa}," + $"{pcp.Cor}," + $"\"{pcp.Tamanho}\"," + $"{pcp.Preco}," + $"{pcp.QtdePedida}," + $"{pcp.QtdeEntregar},"
                                                        + $"{pcp.QtdeEntregue}," + $"{pcp.QtdeCancelada}," + $"{pcp.ValorPedido}," + $"{pcp.ValorEntregar}," + $"{pcp.ValorEntregue}," + $"{pcp.ValorCancelado}" + ")";

                            command011.ExecuteNonQuery();
                            objConx011.Close();

                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine($"ID Produto Pedido de Compra :  {pcp.ProdutoPc} - Pedidov: {pcp.Pedidoc} - Erro: {e.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Produto Pedido de Compra :  {pcp.ProdutoPc} - Pedidoc: {pcp.Pedidoc} - Erro: {e.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Produto Pedido de Compra :  {pcp.ProdutoPc} - Pedidov: {pcp.Pedidoc} - Erro: {e.Message}");
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
