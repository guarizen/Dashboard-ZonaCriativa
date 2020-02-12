using ListaRepresentantesAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Representantes
{
    class RepresentantesGet
    {
        public RepresentantesGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectRepresentantes = ConfigurationManager.AppSettings["ConnectRepresentantes"];
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
                MySqlConnection objConx04 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx04.Open();

                var command04 = objConx04.CreateCommand();
                command04.CommandText = "SELECT COUNT(*) AS N FROM REPRESENTANTES";
                var retCommnad4 = command04.ExecuteReaderAsync();
                string fault4 = retCommnad4.Status.ToString();

                if (fault4 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Representantes...");
                    var CommandInsert4 = objConx04.CreateCommand();
                    CommandInsert4.CommandText = "CREATE TABLE REPRESENTANTES (REPRESENTANTE INT NOT NULL,COD_REPRESENTANTE VARCHAR(30),NOME VARCHAR(255),FANTASIA VARCHAR(255),CGC VARCHAR(30),CNPJ VARCHAR(30),IE VARCHAR(30),"
                                                + "PF_PJ VARCHAR(5),E_MAIL VARCHAR(255),COD_ENDERECO INT,ENDERECO VARCHAR(255),BAIRRO VARCHAR(255),CIDADE VARCHAR(255),ESTADO VARCHAR(2),CEP VARCHAR(12),PAIS VARCHAR(255));";
                    CommandInsert4.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Representantes...");
                    var indiceRep = objConx04.CreateCommand();
                    indiceRep.CommandText = "CREATE INDEX IDX_REPRESENTANTES ON REPRESENTANTES(REPRESENTANTE);";
                    indiceRep.ExecuteNonQuery();

                    objConx04.Close();
                }
                objConx04.Close();

                var requisicaoWeb4 = WebRequest.CreateHttp($"{ConnectRepresentantes}" + "?$format=json");
                requisicaoWeb4.Method = "GET";
                requisicaoWeb4.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb4.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb4.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Representantes...");
                using (var resposta4 = requisicaoWeb4.GetResponse())
                {
                    var streamDados4 = resposta4.GetResponseStream();
                    StreamReader reader4 = new StreamReader(streamDados4);
                    object objResponse4 = reader4.ReadToEnd();
                    var statusCodigo4 = ((System.Net.HttpWebResponse)resposta4).StatusCode;

                    ListaRepresentantes Rep = JsonConvert.DeserializeObject<ListaRepresentantes>(objResponse4.ToString());

                    Console.WriteLine("Listando e Incluindo Representantes...");
                    int r = 0;
                    Parallel.ForEach(Rep.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, Rp =>
                    {
                        try
                        {
                            MySqlConnection objConx4 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx4.Open();

                            while (r < 1)
                            {
                                if (r == 1)
                                    break;
                                var commandelet = objConx4.CreateCommand();
                                commandelet.CommandText = "DELETE FROM REPRESENTANTES";
                                commandelet.ExecuteNonQuery();
                                r++;
                            }

                            var command4 = objConx4.CreateCommand();
                            command4.CommandText = "INSERT INTO REPRESENTANTES (REPRESENTANTE,COD_REPRESENTANTE,NOME,FANTASIA,CGC,CNPJ,IE,PF_PJ,E_MAIL,COD_ENDERECO,ENDERECO,BAIRRO,CIDADE,ESTADO,CEP,PAIS)" +
                                                        $"VALUES({Rp.Representante}," + $"\"{Rp.CodRepresentante}\"," + $"\"{Rp.Nome}\", " + $"\"{Rp.Fantasia}\", "
                                                        + $"\"{Rp.Cgc}\", " + $"\"{Rp.Cnpj}\", " + $"\"{Rp.Ie}\", " + $"\"{Rp.PfPj}\"," + $"\"{Rp.EMail}\","
                                                        + $"\"{Rp.CodEndereco}\"," + $"\"{Rp.Endereco}\"," + $"\"{Rp.Bairro}\"," + $"\"{Rp.Cidade}\","
                                                        + $"\"{Rp.Estado}\"," + $"\"{Rp.Cep}\"," + $"\"{Rp.Pais}\")";

                            command4.ExecuteNonQuery();
                            objConx4.Close();

                        }
                        catch (MySqlException ep)
                        {
                            Console.WriteLine($"ID Representante :  {Rp.CodRepresentante} - {Rp.Nome} - {Rp.Cnpj} - Erro: {ep.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Representante :  {Rp.CodRepresentante} - {Rp.Nome} - {Rp.Cnpj} - Erro: {ep.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Representante :  {Rp.CodRepresentante} - {Rp.Nome} - {Rp.Cnpj} - Erro: {ep.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch (WebException er)
            {
                Console.WriteLine(er.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{er.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{er.Message}");
                    }
                }

            }
        }
    }
}
