using ListaFiliaisAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Filiais
{
    class FiliaisGet
    {
        public FiliaisGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectFiliais = ConfigurationManager.AppSettings["ConnectFiliais"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];

            string FileLog = @"C:\Dashboard-Log\";
            if (!File.Exists(FileLog))
            {
                //Criar Diretório de Log
                Directory.CreateDirectory(FileLog);
            }

            try
            {
                MySqlConnection objConx02 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx02.Open();

                var command02 = objConx02.CreateCommand();
                command02.CommandText = "SELECT COUNT(*) AS N FROM FILIAIS";
                var retCommnad2 = command02.ExecuteReaderAsync();
                string fault2 = retCommnad2.Status.ToString();

                if (fault2 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Filiais...");
                    var CommandInsert3 = objConx02.CreateCommand();
                    CommandInsert3.CommandText = "CREATE TABLE FILIAIS (FILIAL INT NOT NULL,COD_FILIAL VARCHAR(30),NOME VARCHAR(255),FANTASIA VARCHAR(255),CGC VARCHAR(30),CNPJ VARCHAR(30),IE VARCHAR(30),CONTA INT,"
                                               + "TIPO_EMPRESA VARCHAR(60),E_MAIL VARCHAR(255),ENDERECO VARCHAR(255),BAIRRO VARCHAR(255),CIDADE VARCHAR(255),ESTADO VARCHAR(2),CEP VARCHAR(12));";
                    CommandInsert3.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Filiais...");
                    var indiceFili = objConx02.CreateCommand();
                    indiceFili.CommandText = "CREATE INDEX IDX_FILIAIS ON FILIAIS(FILIAL);";
                    indiceFili.ExecuteNonQuery();

                    objConx02.Close();
                }
                objConx02.Close();

                var requisicaoWeb2 = WebRequest.CreateHttp($"{ConnectFiliais}" + "?$format=json");
                requisicaoWeb2.Method = "GET";
                requisicaoWeb2.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb2.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb2.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Filiais...");
                using (var resposta2 = requisicaoWeb2.GetResponse())
                {
                    var streamDados2 = resposta2.GetResponseStream();
                    StreamReader reader2 = new StreamReader(streamDados2);
                    object objResponse2 = reader2.ReadToEnd();
                    var statusCodigo2 = ((System.Net.HttpWebResponse)resposta2).StatusCode;

                    ListaFiliais Fil = JsonConvert.DeserializeObject<ListaFiliais>(objResponse2.ToString());

                    Console.WriteLine("Listando e Incluindo Filiais...");
                    int f = 0;
                    Parallel.ForEach(Fil.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, fili =>
                    {
                        try
                        {
                            MySqlConnection objConx2 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx2.Open();

                            while (f < 1)
                            {
                                if (f == 1)
                                    break;
                                var commandelet = objConx2.CreateCommand();
                                commandelet.CommandText = "DELETE FROM FILIAIS";
                                commandelet.ExecuteNonQuery();
                                f++;
                            }

                            var command2 = objConx2.CreateCommand();
                            command2.CommandText = "INSERT INTO FILIAIS (FILIAL,COD_FILIAL,NOME,FANTASIA,CGC,CNPJ,IE,CONTA,TIPO_EMPRESA,E_MAIL,ENDERECO,BAIRRO,CIDADE,ESTADO,CEP)" +
                                                        $"VALUES({fili.Filial}," + $"\"{fili.CodFilial}\"," + $"\"{fili.Nome}\", " + $"\"{fili.Fantasia}\", "
                                                        + $"\"{fili.Cgc}\", " + $"\"{fili.Cnpj}\", " + $"\"{fili.Ie}\", " + $"{fili.Conta}," + $"\"{fili.TipoEmpresa}\","
                                                        + $"\"{fili.EMail}\", " + $"\"{fili.Endereco}\"," + $"\"{fili.Bairro}\", " + $"\"{fili.Cidade}\", " + $"\"{fili.Estado}\", " + $"\"{fili.Cep}\")";

                            command2.ExecuteNonQuery();
                            objConx2.Close();

                        }
                        catch (MySqlException ep)
                        {
                            Console.WriteLine($"ID Filial :  {fili.CodFilial} - {fili.Nome} - {fili.Cnpj} - Erro: {ep.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Filial :  {fili.CodFilial} - {fili.Nome} - {fili.Cnpj} - Erro: {ep.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Filial :  {fili.CodFilial} - {fili.Nome} - {fili.Cnpj} - Erro: {ep.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch(WebException e)
            {
                Console.WriteLine(e.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{e.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{e.Message}");
                    }
                }

            }
        }
    }
}
