using ListaNotasFiscaisAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.NotasFiscais
{
    class NotasFiscaisGet
    {
        public NotasFiscaisGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectNotasFiscais = ConfigurationManager.AppSettings["ConnectNotasFiscais"];
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
                MySqlConnection objConx13 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx13.Open();

                var command13 = objConx13.CreateCommand();
                command13.CommandText = "SELECT COUNT(*) AS N FROM NF";
                var retCommnad13 = command13.ExecuteReaderAsync();
                string fault13 = retCommnad13.Status.ToString();

                if (fault13 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Notas Fiscais...");
                    var CommandInsert13 = objConx13.CreateCommand();
                    CommandInsert13.CommandText = "CREATE TABLE NF (COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),DATA DATE,DATA_HORA VARCHAR(20),NOTA INT,SERIE VARCHAR(5),STATUS INT,VALOR DECIMAL(14, 2),ICMS DECIMAL(14,2),V_ICMS DECIMAL(14,2),ICMSS DECIMAL(14,2),"
                                                + "V_ICMSS DECIMAL(14,2),IPI DECIMAL(14,2),V_IPI DECIMAL(14,2),FILIAL INT,IDNFE VARCHAR(80),CIDADE VARCHAR(255),ESTADO VARCHAR(5));";
                    CommandInsert13.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela NF...");
                    var indiceNF = objConx13.CreateCommand();
                    indiceNF.CommandText = "CREATE INDEX IDX_NF ON NF(COD_OPERACAO);";
                    indiceNF.ExecuteNonQuery();

                    objConx13.Close();
                }
                objConx13.Close();

                var requisicaoWeb13 = WebRequest.CreateHttp($"{ConnectNotasFiscais}" + "?$format=json");
                requisicaoWeb13.Method = "GET";
                requisicaoWeb13.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb13.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb13.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Listando e Incluindo Notas Fiscais...");
                using (var resposta13 = requisicaoWeb13.GetResponse())
                {
                    var streamDados13 = resposta13.GetResponseStream();
                    StreamReader reader13 = new StreamReader(streamDados13);
                    object objResponse13 = reader13.ReadToEnd();
                    var statusCodigo13 = ((System.Net.HttpWebResponse)resposta13).StatusCode;

                    ListaNotasFicais nf = JsonConvert.DeserializeObject<ListaNotasFicais>(objResponse13.ToString());

                    int nfo = 0;
                    Parallel.ForEach(nf.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, nfl =>
                    {
                        try
                        {
                            MySqlConnection objConx013 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx013.Open();

                            while (nfo < 1)
                            {
                                if (nfo == 1)
                                    break;
                                var commandelet = objConx013.CreateCommand();
                                commandelet.CommandText = "DELETE FROM NF";
                                commandelet.ExecuteNonQuery();
                                nfo++;
                            }

                            string iDateD = $"{nfl.Data}";
                            DateTime eDate = DateTime.Parse(iDateD);
                            string DateN = (eDate.Year + "-" + eDate.Month + "-" + eDate.Day);

                            var command013 = objConx013.CreateCommand();
                            command013.CommandText = "INSERT INTO NF (COD_OPERACAO,TIPO_OPERACAO,DATA,DATA_HORA,NOTA,SERIE,STATUS,VALOR,ICMS,V_ICMS,ICMSS,V_ICMSS,IPI,V_IPI,FILIAL,IDNFE,CIDADE,ESTADO)" +
                                                        $"VALUES({nfl.CodOperacao}," + $"\"{nfl.TipoOperacao}\"," + $"\"{DateN}\"," + $"\"{nfl.DataHora}\"," + $"{nfl.Nota}," + $"\"{nfl.Serie}\"," + $"{nfl.Status}," + $"{nfl.Valor}," + $"{nfl.Icms}," + $"{nfl.VIcms},"
                                                        + $"{nfl.Icmss}," + $"{nfl.VIcmss}," + $"{nfl.Ipi}," + $"{nfl.VIpi}," + $"{nfl.Filial}," + $"\"{nfl.Idnfe}\"," + $"\"{nfl.Cidade}\"," + $"\"{nfl.Estado}\")";

                            command013.ExecuteNonQuery();
                            objConx013.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Nota Fiscal :  {nfl.CodOperacao} - Nota: {nfl.Nota} - Erro: {etv.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Nota Fiscal :  {nfl.CodOperacao} - Nota: {nfl.Nota} - Erro: {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Nota Fiscal :  {nfl.CodOperacao} - Nota: {nfl.Nota} - Erro: {etv.Message}");
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
