using ListaClientesAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Clientes
{
    class ClientesGet
    {
        public ClientesGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectClientes = ConfigurationManager.AppSettings["ConnectClientes"];
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
                MySqlConnection objConx03 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx03.Open();

                var command03 = objConx03.CreateCommand();
                command03.CommandText = "SELECT COUNT(*) AS N FROM CLIENTES";
                var retCommnad3 = command03.ExecuteReaderAsync();
                string fault3 = retCommnad3.Status.ToString();

                if (fault3 == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Clientes...");
                    var CommandInsert3 = objConx03.CreateCommand();
                    CommandInsert3.CommandText = "CREATE TABLE CLIENTES (CLIENTE INT NOT NULL,COD_CLIENTE VARCHAR(30),NOME VARCHAR(255),FANTASIA VARCHAR(255),CGC VARCHAR(30),CNPJ VARCHAR(30),IE VARCHAR(30),PF_PJ VARCHAR(5),TIPO_EMPRESA VARCHAR(60),"
                                                + "E_MAIL VARCHAR(255),E_MAIL_NFE VARCHAR(255),GRUPO_LOJA VARCHAR(255),REPRESENTANTE INT,COD_ENDERECO INT,ENDERECO VARCHAR(255),BAIRRO VARCHAR(255),CIDADE VARCHAR(255),ESTADO VARCHAR(2),CEP VARCHAR(12),"
                                                + "PAIS VARCHAR(255),REGIAO VARCHAR(255));";
                    CommandInsert3.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Clientes...");
                    var indiceCli = objConx03.CreateCommand();
                    indiceCli.CommandText = "CREATE INDEX IDX_CLIENTES ON CLIENTES(CLIENTE,COD_CLIENTE,COD_ENDERECO);";
                    indiceCli.ExecuteNonQuery();

                    objConx03.Close();
                }
                objConx03.Close();

                var requisicaoWeb3 = WebRequest.CreateHttp($"{ConnectClientes}" + "?$format=json");
                requisicaoWeb3.Method = "GET";
                requisicaoWeb3.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb3.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb3.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Clientes...");
                using (var resposta3 = requisicaoWeb3.GetResponse())
                {
                    var streamDados3 = resposta3.GetResponseStream();
                    StreamReader reader3 = new StreamReader(streamDados3);
                    object objResponse3 = reader3.ReadToEnd();
                    var statusCodigo3 = ((System.Net.HttpWebResponse)resposta3).StatusCode;

                    ListaClientes Cli = JsonConvert.DeserializeObject<ListaClientes>(objResponse3.ToString());

                    Console.WriteLine("Listando e Incluindo Clientes...");
                    int c = 0;
                    Parallel.ForEach(Cli.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, Cl =>
                    {
                        try
                        {
                            MySqlConnection objConx3 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx3.Open();

                            while (c < 1)
                            {
                                if (c == 1)
                                    break;
                                var commandelet = objConx3.CreateCommand();
                                commandelet.CommandText = "DELETE FROM CLIENTES";
                                commandelet.ExecuteNonQuery();
                                c++;
                            }

                            var command3 = objConx3.CreateCommand();
                            command3.CommandText = "INSERT INTO CLIENTES (CLIENTE,COD_CLIENTE,NOME,FANTASIA,CGC,CNPJ,IE,PF_PJ,TIPO_EMPRESA,E_MAIL,E_MAIL_NFE,GRUPO_LOJA,REPRESENTANTE,COD_ENDERECO,ENDERECO,BAIRRO,CIDADE,ESTADO,CEP,PAIS,REGIAO)" +
                                                        $"VALUES({Cl.Cliente}," + $"\"{Cl.CodCliente}\"," + $"\"{Cl.Nome}\", " + $"\"{Cl.Fantasia}\", "
                                                        + $"\"{Cl.Cgc}\", " + $"\"{Cl.Cnpj}\", " + $"\"{Cl.Ie}\", " + $"\"{Cl.PfPj}\"," + $"\"{Cl.TipoEmpresa}\", " + $"\"{Cl.EMail}\"," + $"\"{Cl.EMailNfe}\", "
                                                        + $"\"{Cl.GrupoLoja}\", " + $"\"{Cl.Representante}\"," + $"\"{Cl.CodEndereco}\"," + $"\"{Cl.Endereco}\"," + $"\"{Cl.Bairro}\"," + $"\"{Cl.Cidade}\","
                                                        + $"\"{Cl.Estado}\"," + $"\"{Cl.Cep}\"," + $"\"{Cl.Pais}\"," + $"\"{Cl.Regiao}\")";

                            command3.ExecuteNonQuery();
                            objConx3.Close();

                        }
                        catch (MySqlException ep)
                        {
                            Console.WriteLine($"ID Cliente :  {Cl.CodCliente} - {Cl.Nome} - {Cl.Cnpj} - Erro: {ep.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Cliente :  {Cl.CodCliente} - {Cl.Nome} - {Cl.Cnpj} - Erro: {ep.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Cliente :  {Cl.CodCliente} - {Cl.Nome} - {Cl.Cnpj} - Erro: {ep.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch (WebException ec)
            {
                Console.WriteLine(ec.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{ec.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{ec.Message}");
                    }
                }

            }
        }
    }
}
