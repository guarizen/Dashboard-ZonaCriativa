using ListaProdutosAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DashZonaCriativa.Produtos
{
    class ProdutosGet
    {
        public ProdutosGet()
        {
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectProdutos = ConfigurationManager.AppSettings["ConnectProdutos"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];

            string FileLog = @"C:\Dashboard-Log\";
            if (!File.Exists(FileLog))
            {
                //Criar Diretório de Log
                Directory.CreateDirectory(FileLog);
            }

            try
            {
                MySqlConnection objConx0 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                objConx0.Open();

                var command0 = objConx0.CreateCommand();
                command0.CommandText = "SELECT COUNT(*) AS N FROM PRODUTOS";
                var retcommnad = command0.ExecuteReaderAsync();
                string fault = retcommnad.Status.ToString();

                if (fault == "Faulted")
                {
                    Console.WriteLine("Criando Tabela de Produtos...");
                    var commandinsert1 = objConx0.CreateCommand();
                    commandinsert1.CommandText = "CREATE TABLE PRODUTOS (PRODUTO INT NOT NULL,COD_PRODUTO VARCHAR(30),DESCRICAO1 VARCHAR(255),REFERENCIA VARCHAR(255),ESTAMPA VARCHAR(255),COR VARCHAR(255),TAMANHO VARCHAR(5),COD_NCM VARCHAR(15),DEPARTAMENTOS VARCHAR(255),"
                                                 + "DIVISAO VARCHAR(255),GRUPO VARCHAR(255),TIPO VARCHAR(255),COLECAO VARCHAR(255),SUBCOLECAO VARCHAR(255),MARCA VARCHAR(255),CATEGORIA VARCHAR(255),STATUS VARCHAR(255),QMM INT,ALTURA VARCHAR(10),LARGURA VARCHAR(10),"
                                                 + "COMPRIMENTO VARCHAR(10),PEDIDO_BLOQUEADO VARCHAR(50),VENDA_BLOQUEADO VARCHAR(50),EAN13 VARCHAR(15),ESTOQUE_09 INT,ESTOQUE_11 INT,ESTOQUE_500 INT);";
                    commandinsert1.ExecuteNonQuery();

                    Console.WriteLine("Criando Indice Tabela de Produtos...");
                    var indiceProd = objConx0.CreateCommand();
                    indiceProd.CommandText = "CREATE INDEX IDX_PRODUTOS ON PRODUTOS(PRODUTO,ESTAMPA,COR,TAMANHO);";
                    indiceProd.ExecuteNonQuery();

                    objConx0.Close();
                }
                objConx0.Close();

                var requisicaoWeb = WebRequest.CreateHttp($"{ConnectProdutos}" + "?$format=json");
                requisicaoWeb.Method = "GET";
                requisicaoWeb.Headers.Add("Authorization", $"{Authorization}");
                requisicaoWeb.UserAgent = "RequisicaoAPIGET";
                requisicaoWeb.Timeout = 1300000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                Console.WriteLine("Carregando Produtos...");
                using (var resposta = requisicaoWeb.GetResponse())
                {
                    var streamDados = resposta.GetResponseStream();
                    StreamReader reader = new StreamReader(streamDados);
                    object objResponse = reader.ReadToEnd();
                    var statusCodigo = ((System.Net.HttpWebResponse)resposta).StatusCode;

                    ListaProdutos Prod = JsonConvert.DeserializeObject<ListaProdutos>(objResponse.ToString());

                    Console.WriteLine("Listando e Incluindo Produtos...");
                    int i = 0;
                    Parallel.ForEach(Prod.Value, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, prof =>
                    {
                        try
                        {
                            MySqlConnection objConx = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                            objConx.Open();

                            while (i < 1)
                            {
                                if (i == 1)
                                    break;
                                var commandelet = objConx.CreateCommand();
                                commandelet.CommandText = "DELETE FROM PRODUTOS";
                                commandelet.ExecuteNonQuery();
                                i++;
                            }

                            var command = objConx.CreateCommand();
                            command.CommandText = "INSERT INTO PRODUTOS (PRODUTO,COD_PRODUTO,DESCRICAO1,REFERENCIA,ESTAMPA,COR,TAMANHO,COD_NCM,DEPARTAMENTOS,DIVISAO,GRUPO,TIPO,COLECAO,SUBCOLECAO,MARCA,CATEGORIA,STATUS,QMM,ALTURA,LARGURA,COMPRIMENTO,PEDIDO_BLOQUEADO,VENDA_BLOQUEADO,EAN13,ESTOQUE_09,ESTOQUE_11,ESTOQUE_500)" +
                                                        $"VALUES({prof.Produto}," + $"\"{prof.CodProduto}\", " + $"\"{prof.Descricao1}\", " + $"\"{prof.Referencia}\", " + $"\"{prof.Estampa}\", " + $"\"{prof.Cor}\", " + $"\"{prof.Tamanho}\", "
                                                                 + $"\"{prof.CodNcm}\", " + $"\"{prof.Departamentos}\", " + $"\"{prof.Divisao}\", " + $"\"{prof.Grupo}\", " + $"\"{prof.Tipo}\", " + $"\"{prof.Colecao}\", " + $"\"{prof.Subcolecao}\", " + $"\"{prof.Marca}\", "
                                                                 + $"\"{prof.Categoria}\", " + $"\"{prof.Status}\", " + $"\"{prof.Qmm}\", " + $"\"{prof.Altura}\", " + $"\"{prof.Largura}\", " + $"\"{prof.Comprimento}\", " + $"\"{prof.PedidoBloqueado}\", " + $"\"{prof.VendaBloqueado}\", " + $"\"{prof.Ean13}\", " + $"\"{prof.Estoque09}\", " + $"\"{prof.Estoque11}\", " + $"\"{prof.Estoque500}\")";

                            command.ExecuteNonQuery();
                            objConx.Close();
                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine($"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - Erro: {e.Message}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - Erro: {e.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now:dd/MM/yyyy HH:mm:ss}" + " - " + $"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - Erro: {e.Message}");
                                }
                            }
                        }
                    });
                }
            }
            catch (WebException ef)
            {
                Console.WriteLine(ef.Message);

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
                    writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{ef.Message}");
                    writer1.Close();

                }
                //Verifica se o arquivo de Log já existe e inclui as informações.
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"{ef.Message}");
                    }
                }

            }
        }
    }
}
