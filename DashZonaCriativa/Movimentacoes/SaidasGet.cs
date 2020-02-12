using ListaSaidasAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Movimentacoes
{
    class SaidasGet
    {
        public SaidasGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectMovSaidas = ConfigurationManager.AppSettings["ConnectMovSaidas"];
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
                MySqlConnection objConx05 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx05.Open();

                var command05 = objConx05.CreateCommand();
                command05.CommandText = "SELECT COUNT(*) AS N FROM SAIDAS";
                var retCommnad5 = command05.ExecuteReaderAsync();
                string fault5 = retCommnad5.Status.ToString();

                if (fault5 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Saidas...");
                    var CommandInsert5 = objConx05.CreateCommand();
                    CommandInsert5.CommandText = "CREATE TABLE SAIDAS (COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),EVENTO INT,ROMANEIO VARCHAR(60),DATA DATE,CLIENTE INT,COD_ENDERECO INT,CONDICOES_PGTO VARCHAR(255),"
                                                + "FILIAL INT,CONTA INT,REPRESENTANTE INT,TRANSPORTADORA VARCHAR(255),PESO_B VARCHAR(25),PESO_L VARCHAR(25),QTDE INT,TOTAL DECIMAL(14, 2),V_FRETE DECIMAL(14,2),VALOR_JUROS DECIMAL(14,2),"
                                                + "VALOR_FINAL DECIMAL(14,2),ESPECIE_VOLUME VARCHAR(255),VOLUME VARCHAR(25));";
                    CommandInsert5.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Saidas...");
                    var indiceSd = objConx05.CreateCommand();
                    indiceSd.CommandText = "CREATE INDEX IDX_SAIDAS ON SAIDAS(COD_OPERACAO,EVENTO,CLIENTE,COD_ENDERECO,FILIAL,REPRESENTANTE);";
                    indiceSd.ExecuteNonQuery();

                    objConx05.Close();
                }
                objConx05.Close();

                var requisicaoWeb5 = WebRequest.CreateHttp($"{ConnectMovSaidas}" + "?$format=json");
                requisicaoWeb5.Method = "GET";
                requisicaoWeb5.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb5.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb5.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Movimentacoes de Saidas...");
                using (var resposta5 = requisicaoWeb5.GetResponse())
                {
                    var streamDados5 = resposta5.GetResponseStream();
                    StreamReader reader5 = new StreamReader(streamDados5);
                    object objResponse5 = reader5.ReadToEnd();
                    var statusCodigo5 = ((System.Net.HttpWebResponse)resposta5).StatusCode;

                    ListaMovSaidas Sale = JsonConvert.DeserializeObject<ListaMovSaidas>(objResponse5.ToString());

                    Console.WriteLine("Listando e Incluindo Movimentacoes de Saidas...");
                    int s = 0;
                    Parallel.ForEach(Sale.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, sd =>
                    {
                        try
                        {
                            MySqlConnection objConx5 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx5.Open();

                            while (s < 1)
                            {
                                if (s == 1)
                                    break;
                                var commandelet = objConx5.CreateCommand();
                                commandelet.CommandText = "DELETE FROM SAIDAS";
                                commandelet.ExecuteNonQuery();
                                s++;
                            }

                            string iDateD = $"{sd.Data}";
                            DateTime eDate = DateTime.Parse(iDateD);
                            string DateD = (eDate.Year + "-" + eDate.Month + "-" + eDate.Day);

                            var command5 = objConx5.CreateCommand();
                            command5.CommandText = "INSERT INTO SAIDAS (COD_OPERACAO,TIPO_OPERACAO,EVENTO,ROMANEIO,DATA,CLIENTE,COD_ENDERECO,CONDICOES_PGTO,FILIAL,CONTA,REPRESENTANTE,TRANSPORTADORA,PESO_B,PESO_L,QTDE,TOTAL,V_FRETE,VALOR_JUROS,VALOR_FINAL,ESPECIE_VOLUME,VOLUME)" +
                                                        $"VALUES({sd.CodOperacao}," + $"\"{sd.TipoOperacao}\"," + $"\"{sd.Evento}\", " + $"\"{sd.Romaneio}\", " + $"\"{DateD}\", "
                                                        + $"\"{sd.Cliente}\", " + $"\"{sd.CodigoEndereco}\", " + $"\"{sd.CondicoesPgto}\", " + $"{sd.Filial}, " + $"{sd.Conta}," + $"{sd.Representante},"
                                                        + $"\"{sd.Transportadora}\"," + $"\"{sd.PesoB}\"," + $"\"{sd.PesoL}\"," + $"{sd.Qtde}," + $"\"{sd.Total}\", "
                                                        + $"\"{sd.VFrete}\"," + $"\"{sd.ValorJuros}\"," + $"\"{sd.ValorFinal}\"," + $"\"{sd.EspecieVolume}\"," + $"\"{sd.Volume}\")";

                            command5.ExecuteNonQuery();
                            objConx5.Close();

                        }
                        catch (MySqlException ep)
                        {
                            Console.WriteLine($"ID Saida :  {sd.CodOperacao} - Romaneio: {sd.Romaneio} - Cliente: {sd.Cliente} - Erro: {ep.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Saida :  {sd.CodOperacao} - Romaneio: {sd.Romaneio} - Cliente: {sd.Cliente} - Erro: {ep.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Saida :  {sd.CodOperacao} - Romaneio: {sd.Romaneio} - Cliente: {sd.Cliente} - Erro: {ep.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch (WebException es)
            {
                Console.WriteLine(es.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{es.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{es.Message}");
                    }
                }
            }
        }
    }
}
