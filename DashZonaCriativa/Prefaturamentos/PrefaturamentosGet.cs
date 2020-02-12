using ListaPrefaturamentosAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Prefaturamentos
{
    class PrefaturamentosGet
    {
        public PrefaturamentosGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectPrefaturamentos = ConfigurationManager.AppSettings["ConnectPrefaturamentos"];
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
                MySqlConnection objConx08 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx08.Open();

                var command08 = objConx08.CreateCommand();
                command08.CommandText = "SELECT COUNT(*) AS N FROM PREFATURAMENTOS";
                var retCommnad8 = command08.ExecuteReaderAsync();
                string fault8 = retCommnad8.Status.ToString();

                if (fault8 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Prefaturamentos...");
                    var CommandInsert8 = objConx08.CreateCommand();
                    CommandInsert8.CommandText = "CREATE TABLE PREFATURAMENTOS (PREFATURAMENTO INT NOT NULL,NUMERO VARCHAR(30),DATA DATE,FILIAL INT,CLIENTE INT,PEDIDOV INT,EXPEDICAO VARCHAR(1),DATA_EXPEDICAO VARCHAR(20),PODECONFERIR VARCHAR(1),DATA_PODECONFERIR VARCHAR(20),"
                                                + "CONFERINDO VARCHAR(1),CONFERIDO VARCHAR(1),DATACONFERIDO VARCHAR(20),ENTREGUE VARCHAR(1),TRANSPORTADORA VARCHAR(255),OBS_CLI_FAT VARCHAR(255));";
                    CommandInsert8.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Prefaturamentos...");
                    var indicePrefat = objConx08.CreateCommand();
                    indicePrefat.CommandText = "CREATE INDEX IDX_PREFATURAMENTOS ON PREFATURAMENTOS(PREFATURAMENTO,FILIAL,CLIENTE,PEDIDOV);";
                    indicePrefat.ExecuteNonQuery();

                    objConx08.Close();
                }
                objConx08.Close();

                var requisicaoWeb8 = WebRequest.CreateHttp($"{ConnectPrefaturamentos}" + "?$format=json");
                requisicaoWeb8.Method = "GET";
                requisicaoWeb8.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb8.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb8.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Prefaturamentos...");
                using (var resposta8 = requisicaoWeb8.GetResponse())
                {
                    var streamDados8 = resposta8.GetResponseStream();
                    StreamReader reader8 = new StreamReader(streamDados8);
                    object objResponse8 = reader8.ReadToEnd();
                    var statusCodigo8 = ((System.Net.HttpWebResponse)resposta8).StatusCode;

                    ListaPrefaturamentos pft = JsonConvert.DeserializeObject<ListaPrefaturamentos>(objResponse8.ToString());

                    Console.WriteLine("Listando e Incluindo Prefaturamentos...");
                    int pref = 0;
                    Parallel.ForEach(pft.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, ptf =>
                    {
                        try
                        {
                            MySqlConnection objConx8 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx8.Open();

                            while (pref < 1)
                            {
                                if (pref == 1)
                                    break;
                                var commandelet = objConx8.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PREFATURAMENTOS";
                                commandelet.ExecuteNonQuery();
                                pref++;
                            }

                            string iDateD = $"{ptf.Data}";
                            DateTime eDate = DateTime.Parse(iDateD);
                            string DateF = (eDate.Year + "-" + eDate.Month + "-" + eDate.Day);

                            var command8 = objConx8.CreateCommand();
                            command8.CommandText = "INSERT INTO PREFATURAMENTOS (PREFATURAMENTO,NUMERO,DATA,FILIAL,CLIENTE,PEDIDOV,EXPEDICAO,DATA_EXPEDICAO,PODECONFERIR,DATA_PODECONFERIR,CONFERINDO,CONFERIDO,DATACONFERIDO,ENTREGUE,TRANSPORTADORA,OBS_CLI_FAT)" +
                                                        $"VALUES({ptf.Prefaturamento}," + $"\"{ptf.Numero}\", " + $"\"{DateF}\", " + $"{ptf.Filial}," + $"{ptf.Cliente},"
                                                        + $"{ptf.Pedidov}," + $"\"{ptf.Expedicao}\"," + $"\"{ptf.DataExpedicao}\", " + $"\"{ptf.Podeconferir}\", " + $"\"{ptf.DataPodeconferir}\", " + $"\"{ptf.Conferindo}\","
                                                        + $"\"{ptf.Conferido}\"," + $"\"{ptf.Dataconferido}\"," + $"\"{ptf.Entregue}\"," + $"\"{ptf.Transportadora}\"," + $"\"{ptf.ObsCliFat}\")";

                            command8.ExecuteNonQuery();
                            objConx8.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Prefaturamento :  {ptf.Prefaturamento} - Cliente: {ptf.Cliente} - Erro: {etv.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Prefaturamento :  {ptf.Prefaturamento} - Cliente: {ptf.Cliente} - Erro: {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Prefaturamento :  {ptf.Prefaturamento} - Cliente: {ptf.Cliente} - Erro: {etv.Message}");
                                }
                            }

                        }
                    });
                }
            }
            catch (WebException wpf)
            {
                Console.WriteLine(wpf.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{wpf.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{wpf.Message}");
                    }
                }
            }
        }
    }
}
