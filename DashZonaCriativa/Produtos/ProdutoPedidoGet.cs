using ListaProdutosPedidovAPI;
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
    class ProdutoPedidoGet
    {
        public ProdutoPedidoGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectProdutosPedidov = ConfigurationManager.AppSettings["ConnectProdutosPedidov"];
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
                MySqlConnection objConx12 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx12.Open();

                var command12 = objConx12.CreateCommand();
                command12.CommandText = "SELECT COUNT(*) AS N FROM PRODUTO_PEDIDOV";
                var retCommnad12 = command12.ExecuteReaderAsync();
                string fault12 = retCommnad12.Status.ToString();

                if (fault12 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela Produto Pedido Venda...");
                    var CommandInsert12 = objConx12.CreateCommand();
                    CommandInsert12.CommandText = "CREATE TABLE PRODUTO_PEDIDOV (PRODUTO_PV INT NOT NULL,PEDIDOV INT,PRODUTO INT,ESTAMPA INT,COR INT,TAMANHO VARCHAR(5),PRECO DECIMAL(14,2),QTDE_PEDIDA INT,QTDE_ENTREGAR INT,QTDE_ENTREGUE INT,QTDE_CANCELADA INT,VALOR_PEDIDO DECIMAL(14,2),"
                                               + "VALOR_ENTREGAR DECIMAL(14,2),VALOR_ENTREGUE DECIMAL(14,2),VALOR_CANCELADO DECIMAL(14,2));";
                    CommandInsert12.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela Produto Pedido Venda...");
                    var indiceProdpv = objConx12.CreateCommand();
                    indiceProdpv.CommandText = "CREATE INDEX IDX_PRODUTO_PEDIDOV ON PRODUTO_PEDIDOV(PRODUTO_PV,PEDIDOV,PRODUTO,ESTAMPA,COR,TAMANHO);";
                    indiceProdpv.ExecuteNonQuery();

                    objConx12.Close();
                }
                objConx12.Close();

                var requisicaoWeb12 = WebRequest.CreateHttp($"{ConnectProdutosPedidov}" + $"?data_inicial={DateIni}" + $"&data_final={DateFim}" + "&$format=json");
                requisicaoWeb12.Method = "GET";
                requisicaoWeb12.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb12.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb12.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Produtos Pedido Venda...");
                using (var resposta12 = requisicaoWeb12.GetResponse())
                {
                    var streamDados12 = resposta12.GetResponseStream();
                    StreamReader reader12 = new StreamReader(streamDados12);
                    object objResponse12 = reader12.ReadToEnd();
                    var statusCodigo12 = ((System.Net.HttpWebResponse)resposta12).StatusCode;

                    ListaProdutosPedidov prpv = JsonConvert.DeserializeObject<ListaProdutosPedidov>(objResponse12.ToString());

                    int pppv = 0;
                    Parallel.ForEach(prpv.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, porv =>
                    {
                        try
                        {
                            MySqlConnection objConx012 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx012.Open();

                            while (pppv < 1)
                            {
                                if (pppv == 1)
                                    break;
                                var commandelet = objConx012.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PRODUTO_PEDIDOV";
                                commandelet.ExecuteNonQuery();
                                pppv++;
                            }

                            var command012 = objConx012.CreateCommand();
                            command012.CommandText = "INSERT INTO PRODUTO_PEDIDOV (PRODUTO_PV,PEDIDOV,PRODUTO,ESTAMPA,COR,TAMANHO,PRECO,QTDE_PEDIDA,QTDE_ENTREGAR,QTDE_ENTREGUE,QTDE_CANCELADA,VALOR_PEDIDO,VALOR_ENTREGAR,VALOR_ENTREGUE,VALOR_CANCELADO)" +
                                                        $"VALUES({porv.ProdutoPv}," + $"{porv.Pedidov}," + $"{porv.Produto}," + $"{porv.Estampa}," + $"{porv.Cor}," + $"\"{porv.Tamanho}\"," + $"{porv.Preco}," + $"{porv.QtdePedida}," + $"{porv.QtdeEntregar},"
                                                        + $"{porv.QtdeEntregue}," + $"{porv.QtdeCancelada}," + $"{porv.ValorPedido}," + $"{porv.ValorEntregar}," + $"{porv.ValorEntregue}," + $"{porv.ValorCancelado}" + ")";

                            command012.ExecuteNonQuery();
                            objConx012.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Produto Pedido de Venda :  {porv.ProdutoPv} - Pedidov: {porv.Pedidov} - Erro: {etv.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto Pedido de Venda :  {porv.ProdutoPv} - Pedidov: {porv.Pedidov} - Erro: {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto Pedido de Venda :  {porv.ProdutoPv} - Pedidov: {porv.Pedidov} - Erro: {etv.Message}");
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
