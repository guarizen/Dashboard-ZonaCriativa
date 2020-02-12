using ListaPrecosAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Precos
{
    class PrecosGet
    {
        public PrecosGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectPrecos = ConfigurationManager.AppSettings["ConnectPrecos"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];

            string FileLog = @"C:\Dashboard-Log\";
            if (!File.Exists(FileLog))
            {
                //Criar Diretório de Log
                Directory.CreateDirectory(FileLog);
            }
            try
            {
                MySqlConnection objConx01 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx01.Open();

                var command01 = objConx01.CreateCommand();
                command01.CommandText = "SELECT COUNT(*) AS N FROM PRECOS";
                var retCommnad1 = command01.ExecuteReaderAsync();
                string fault1 = retCommnad1.Status.ToString();

                if (fault1 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Precos...");
                    var CommandInsert2 = objConx01.CreateCommand();
                    CommandInsert2.CommandText = "CREATE TABLE PRECOS (PRODUTO INT NOT NULL,COD_PRODUTO VARCHAR(30),DESCRICAO1 VARCHAR(255),ESTAMPA VARCHAR(255),COR VARCHAR(255),"
                                                + "TAMANHO VARCHAR(5),PRECO_214 DECIMAL(14,2),PRECO_187 DECIMAL(14,2),PRECO_224 DECIMAL(14,2),PRECO_204 DECIMAL(14,2),PRECO_205 DECIMAL(14,2));";
                    CommandInsert2.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Precos...");
                    var indicePrec = objConx01.CreateCommand();
                    indicePrec.CommandText = "CREATE INDEX IDX_PRECOS ON PRECOS(PRODUTO,ESTAMPA,COR,TAMANHO);";
                    indicePrec.ExecuteNonQuery();

                    objConx01.Close();
                }
                objConx01.Close();

                var requisicaoWeb1 = WebRequest.CreateHttp($"{ConnectPrecos}" + "?$format=json");
                requisicaoWeb1.Method = "GET";
                requisicaoWeb1.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb1.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb1.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Precos...");
                using (var resposta1 = requisicaoWeb1.GetResponse())
                {
                    var streamDados1 = resposta1.GetResponseStream();
                    StreamReader reader1 = new StreamReader(streamDados1);
                    object objResponse1 = reader1.ReadToEnd();
                    var statusCodigo1 = ((System.Net.HttpWebResponse)resposta1).StatusCode;

                    ListaPrecosGet Prec = JsonConvert.DeserializeObject<ListaPrecosGet>(objResponse1.ToString());

                    Console.WriteLine("Listando e Incluindo Precos...");
                    int p = 0;
                    Parallel.ForEach(Prec.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, pre =>
                    {
                        try
                        {
                            MySqlConnection objConx1 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx1.Open();

                            while (p < 1)
                            {
                                if (p == 1)
                                    break;
                                var commandelet = objConx1.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PRECOS";
                                commandelet.ExecuteNonQuery();
                                p++;
                            }

                            var command1 = objConx1.CreateCommand();
                            command1.CommandText = "INSERT INTO PRECOS (PRODUTO,COD_PRODUTO,DESCRICAO1,ESTAMPA,COR,TAMANHO,PRECO_214,PRECO_187,PRECO_224,PRECO_204,PRECO_205)" +
                                                        $"VALUES({pre.Produto}," + $"\"{pre.CodProduto}\", " + $"\"{pre.Descricao1}\", " + $"\"{pre.Estampa}\", " + $"\"{pre.Cor}\", " + $"\"{pre.Tamanho}\", "
                                                                 + $"{pre.Preco214}" + "," + $"{pre.Preco187}" + "," + $"{pre.Preco224}" + "," + $"{pre.Preco204}" + "," + $"{pre.Preco205}" + ")";

                            command1.ExecuteNonQuery();
                            objConx1.Close();

                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine($"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - Erro: {ex.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - Erro: {ex.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - Erro: {ex.Message}");
                                }
                            }

                        }
                    });
                }
            }
            catch (WebException eb)
            {

                Console.WriteLine(eb.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{eb.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{eb.Message}");
                    }
                }
            }
        }
    }
}
