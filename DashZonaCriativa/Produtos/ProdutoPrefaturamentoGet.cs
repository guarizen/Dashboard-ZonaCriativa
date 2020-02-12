using ListaProdutosPrefatAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.ProdutoPrefaturamento
{
    class ProdutoPrefaturamentoGet
    {
        public ProdutoPrefaturamentoGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectProdutosPrefat = ConfigurationManager.AppSettings["ConnectProdutosPrefat"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];
            var Data_Inicial = ConfigurationManager.AppSettings["Data_Inicial"];

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

            try
            {
                MySqlConnection objConx09 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx09.Open();

                var command09 = objConx09.CreateCommand();
                command09.CommandText = "SELECT COUNT(*) AS N FROM PRODUTO_PREFAT";
                var retCommnad9 = command09.ExecuteReaderAsync();
                string fault9 = retCommnad9.Status.ToString();

                if (fault9 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Produto Prefaturamento...");
                    var CommandInsert9 = objConx09.CreateCommand();
                    CommandInsert9.CommandText = "CREATE TABLE PRODUTO_PREFAT (PRODUTO_PREFAT INT NOT NULL,PREFATURAMENTO INT,PRODUTO INT,ESTAMPA INT,COR INT,TAMANHO VARCHAR(5),QUANTIDADE INT,ENTREGUE INT,SAIDA INT);";
                    CommandInsert9.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Produto Prefaturamento...");
                    var indicePpfat = objConx09.CreateCommand();
                    indicePpfat.CommandText = "CREATE INDEX IDX_PRODUTO_PREFAT ON PRODUTO_PREFAT(PRODUTO_PREFAT,PREFATURAMENTO,PRODUTO,ESTAMPA,COR,TAMANHO);";
                    indicePpfat.ExecuteNonQuery();

                    objConx09.Close();
                }
                objConx09.Close();

                var requisicaoWeb9 = WebRequest.CreateHttp($"{ConnectProdutosPrefat}" + $"?data_inicial={DateIni}" + "&$format=json");
                requisicaoWeb9.Method = "GET";
                requisicaoWeb9.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb9.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb9.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Produtos Prefaturamentos...");
                using (var resposta9 = requisicaoWeb9.GetResponse())
                {
                    var streamDados9 = resposta9.GetResponseStream();
                    StreamReader reader9 = new StreamReader(streamDados9);
                    object objResponse9 = reader9.ReadToEnd();
                    var statusCodigo9 = ((System.Net.HttpWebResponse)resposta9).StatusCode;

                    ListaProdutosPrefat ppft = JsonConvert.DeserializeObject<ListaProdutosPrefat>(objResponse9.ToString());

                    int pppref = 0;
                    Parallel.ForEach(ppft.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, pptf =>
                    {
                        try
                        {
                            MySqlConnection objConx9 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx9.Open();

                            while (pppref < 1)
                            {
                                if (pppref == 1)
                                    break;
                                var commandelet = objConx9.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PRODUTO_PREFAT";
                                commandelet.ExecuteNonQuery();
                                pppref++;
                            }

                            var command9 = objConx9.CreateCommand();
                            command9.CommandText = "INSERT INTO PRODUTO_PREFAT (PRODUTO_PREFAT,PREFATURAMENTO,PRODUTO,ESTAMPA,COR,TAMANHO,QUANTIDADE,ENTREGUE,SAIDA)" +
                                                        $"VALUES({pptf.ProdutoPrefat}," + $"{pptf.Prefaturamento}," + $"{pptf.Produto}," + $"{pptf.Estampa}," + $"{pptf.Cor}," + $"\"{pptf.Tamanho}\","
                                                        + $"{pptf.Quantidade}," + $"{pptf.Entregue}," + $"{pptf.Saida}" + ")";

                            command9.ExecuteNonQuery();
                            objConx9.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Produto Prefat :  {pptf.ProdutoPrefat} - Prefaturamento: {pptf.Prefaturamento} - Erro: {etv.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto Prefat :  {pptf.ProdutoPrefat} - Prefaturamento: {pptf.Prefaturamento} - Erro: {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto Prefat :  {pptf.ProdutoPrefat} - Prefaturamento: {pptf.Prefaturamento} - Erro: {etv.Message}");
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
