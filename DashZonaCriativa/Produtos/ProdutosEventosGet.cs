using ListaProdutosEventosAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.ProdutosEventos
{
    class ProdutosEventosGet
    {
        public ProdutosEventosGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectProdutosEventos = ConfigurationManager.AppSettings["ConnectProdutosEventos"];
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
                MySqlConnection objConx07 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx07.Open();

                var command07 = objConx07.CreateCommand();
                command07.CommandText = "SELECT COUNT(*) AS N FROM PRODUTOS_EVENTOS";
                var retCommnad7 = command07.ExecuteReaderAsync();
                string fault7 = retCommnad7.Status.ToString();

                if (fault7 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Produtos Eventos...");
                    var CommandInsert7 = objConx07.CreateCommand();
                    CommandInsert7.CommandText = "CREATE TABLE PRODUTOS_EVENTOS (PRODUTO_EVENTO INT NOT NULL,COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),PEDIDO INT, PRE_FATURAMENTO INT,PRODUTO INT,ESTAMPA INT,COR INT,TAMANHO VARCHAR(5),QUANTIDADE INT NOT NULL,"
                                                + "PRECO DECIMAL(14,2),DESCONTO DECIMAL(14,2),V_ICMSS DECIMAL(14,2),V_ICMS DECIMAL(14,2),V_IPI DECIMAL(14,2),V_ISS DECIMAL(14,2),V_PIS DECIMAL(14,2),V_CONFINS DECIMAL(14,2));";
                    CommandInsert7.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Produtos Eventos...");
                    var indiceProde = objConx07.CreateCommand();
                    indiceProde.CommandText = "CREATE INDEX IDX_PRODUTOS_EVENTOS ON PRODUTOS_EVENTOS(PRODUTO_EVENTO,COD_OPERACAO,PEDIDO,PRE_FATURAMENTO,PRODUTO,ESTAMPA,COR,TAMANHO);";
                    indiceProde.ExecuteNonQuery();

                    objConx07.Close();
                }
                objConx07.Close();

                var requisicaoWeb7 = WebRequest.CreateHttp($"{ConnectProdutosEventos}" + $"?data_inicial={DateIni}" + $"&data_final={DateFim}" + "&$format=json");
                requisicaoWeb7.Method = "GET";
                requisicaoWeb7.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb7.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb7.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Produtos Eventos...");
                using (var resposta7 = requisicaoWeb7.GetResponse())
                {
                    var streamDados7 = resposta7.GetResponseStream();
                    StreamReader reader7 = new StreamReader(streamDados7);
                    object objResponse7 = reader7.ReadToEnd();
                    var statusCodigo7 = ((System.Net.HttpWebResponse)resposta7).StatusCode;

                    ListaProdutosEventos pdv = JsonConvert.DeserializeObject<ListaProdutosEventos>(objResponse7.ToString());

                    Console.WriteLine("Listando e Incluindo Produtos Eventos (Entradas/Saidas)...");
                    int pev = 0;
                    Parallel.ForEach(pdv.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, ppo =>
                    {
                        try
                        {
                            MySqlConnection objConx7 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx7.Open();

                            while (pev < 1)
                            {
                                if (pev == 1)
                                    break;
                                var commandelet = objConx7.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PRODUTOS_EVENTOS";
                                commandelet.ExecuteNonQuery();
                                pev++;
                            }

                            var command7 = objConx7.CreateCommand();
                            command7.CommandText = "INSERT INTO PRODUTOS_EVENTOS (PRODUTO_EVENTO,COD_OPERACAO,TIPO_OPERACAO,PEDIDO,PRE_FATURAMENTO,PRODUTO,ESTAMPA,COR,TAMANHO,QUANTIDADE,PRECO,DESCONTO,V_ICMSS,V_ICMS,V_IPI,V_ISS,V_PIS,V_CONFINS)" +
                                                        $"VALUES({ppo.ProdutoEvento}," + $"{ppo.CodOperacao}," + $"\"{ppo.TipoOperacao}\", " + $"{ppo.Pedido}," + $"{ppo.PreFaturamento}," + $"{ppo.Produto}," + $"{ppo.Estampa},"
                                                        + $"{ppo.Cor}," + $"\"{ppo.Tamanho}\"," + $"{ppo.Quantidade}," + $"\"{ppo.Preco}\", " + $"\"{ppo.Desconto}\", " + $"\"{ppo.VIcmss}\", "
                                                        + $"\"{ppo.VIcms}\", " + $"\"{ppo.VIpi}\", " + $"\"{ppo.VIss}\", " + $"\"{ppo.VPis}\", " + $"\"{ppo.VConfins}\")";

                            command7.ExecuteNonQuery();
                            objConx7.Close();

                        }
                        catch (MySqlException etv)
                        {
                            Console.WriteLine($"ID Produtos Eventos :  {ppo.CodOperacao} - Produto: {ppo.Produto} - Erro: {etv.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produtos Eventos :  {ppo.CodOperacao} - Produto: {ppo.Produto} - {etv.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produtos Eventos :  {ppo.CodOperacao} - Produto: {ppo.Produto} - {etv.Message}");
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
