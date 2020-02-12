using ListaEntradasAPI;
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
    class EntradasGet
    {
        public EntradasGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectMovEntradas = ConfigurationManager.AppSettings["ConnectMovEntradas"];
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
                MySqlConnection objConx06 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx06.Open();

                var command06 = objConx06.CreateCommand();
                command06.CommandText = "SELECT COUNT(*) AS N FROM ENTRADAS";
                var retCommnad6 = command06.ExecuteReaderAsync();
                string fault6 = retCommnad6.Status.ToString();

                if (fault6 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Entradas...");
                    var CommandInsert6 = objConx06.CreateCommand();
                    CommandInsert6.CommandText = "CREATE TABLE ENTRADAS (COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),EVENTO INT,ROMANEIO VARCHAR(60),DATA DATE,CLIENTE INT,FORNECEDOR INT,COD_ENDERECO INT,CONDICOES_PGTO VARCHAR(255),FILIAL INT,"
                                                + "CONTA INT,REPRESENTANTE INT,TRANSPORTADORA VARCHAR(255),PESO_B VARCHAR(25),PESO_L VARCHAR(25),QTDE INT,TOTAL DECIMAL(14, 2),V_FRETE DECIMAL(14,2),VALOR_JUROS DECIMAL(14,2),VALOR_FINAL DECIMAL(14,2),"
                                                + "ESPECIE_VOLUME VARCHAR(255),VOLUME VARCHAR(25));";
                    CommandInsert6.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Entradas...");
                    var indiceEnt = objConx06.CreateCommand();
                    indiceEnt.CommandText = "CREATE INDEX IDX_ENTRADAS ON ENTRADAS(COD_OPERACAO,CLIENTE,EVENTO);";
                    indiceEnt.ExecuteNonQuery();

                    objConx06.Close();
                }
                objConx06.Close();

                var requisicaoWeb6 = WebRequest.CreateHttp($"{ConnectMovEntradas}" + "?$format=json");
                requisicaoWeb6.Method = "GET";
                requisicaoWeb6.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb6.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb6.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Movimentacoes de Entradas...");
                using (var resposta6 = requisicaoWeb6.GetResponse())
                {
                    var streamDados6 = resposta6.GetResponseStream();
                    StreamReader reader6 = new StreamReader(streamDados6);
                    object objResponse6 = reader6.ReadToEnd();
                    var statusCodigo6 = ((System.Net.HttpWebResponse)resposta6).StatusCode;

                    ListaMovEntradas Entre = JsonConvert.DeserializeObject<ListaMovEntradas>(objResponse6.ToString());

                    Console.WriteLine("Listando e Incluindo Movimentacoes de Entradas...");
                    int en = 0;
                    Parallel.ForEach(Entre.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, ed =>
                    {
                        try
                        {
                            MySqlConnection objConx6 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx6.Open();

                            while (en < 1)
                            {
                                if (en == 1)
                                    break;
                                var commandelet = objConx6.CreateCommand();
                                commandelet.CommandText = "DELETE FROM ENTRADAS";
                                commandelet.ExecuteNonQuery();
                                en++;
                            }

                            string iDateD = $"{ed.Data}";
                            DateTime eDate = DateTime.Parse(iDateD);
                            string DateD = (eDate.Year + "-" + eDate.Month + "-" + eDate.Day);

                            var command6 = objConx6.CreateCommand();
                            command6.CommandText = "INSERT INTO ENTRADAS (COD_OPERACAO,TIPO_OPERACAO,EVENTO,ROMANEIO,DATA,CLIENTE,FORNECEDOR,COD_ENDERECO,CONDICOES_PGTO,FILIAL,CONTA,REPRESENTANTE,TRANSPORTADORA,PESO_B,PESO_L,QTDE,TOTAL,V_FRETE,VALOR_JUROS,VALOR_FINAL,ESPECIE_VOLUME,VOLUME)" +
                                                        $"VALUES({ed.CodOperacao}," + $"\"{ed.TipoOperacao}\"," + $"\"{ed.Evento}\", " + $"\"{ed.Romaneio}\", " + $"\"{DateD}\", "
                                                        + $"\"{ed.Cliente}\", " + $"\"{ed.Fornecedor}\", " + $"\"{ed.CodigoEndereco}\", " + $"\"{ed.CondicoesPgto}\", " + $"{ed.Filial}, " + $"{ed.Conta}," + $"{ed.Representante},"
                                                        + $"\"{ed.Transportadora}\"," + $"\"{ed.PesoB}\"," + $"\"{ed.PesoL}\"," + $"{ed.Qtde}," + $"\"{ed.Total}\", "
                                                        + $"\"{ed.VFrete}\"," + $"\"{ed.ValorJuros}\"," + $"\"{ed.ValorFinal}\"," + $"\"{ed.EspecieVolume}\"," + $"\"{ed.Volume}\")";

                            command6.ExecuteNonQuery();
                            objConx6.Close();

                        }
                        catch (MySqlException ent)
                        {
                            Console.WriteLine($"ID Entrada :  {ed.CodOperacao} - Romaneio: {ed.Romaneio} - Cliente: {ed.Cliente} - Erro: {ent.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Entrada :  {ed.CodOperacao} - Romaneio: {ed.Romaneio} - Cliente: {ed.Cliente} - Erro: {ent.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Entrada :  {ed.CodOperacao} - Romaneio: {ed.Romaneio} - Cliente: {ed.Cliente} - Erro: { ent.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch (WebException ee)
            {
                Console.WriteLine(ee.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{ee.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{ee.Message}");
                    }
                }
            }
        }
    }
}
