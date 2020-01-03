using ListaClientesAPI;
using ListaEntradasAPI;
using ListaFiliaisAPI;
using ListaPrecosAPI;
using ListaProdutosAPI;
using ListaProdutosEventosAPI;
using ListaRepresentantesAPI;
using ListaSaidasAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace DashZonaCriativa
{
    class Program
    {
        static void Main(string[] args)
        {
            //Buscar se existe a pasta.
            string FileLog = @"C:\Dashboard-Log\";
            if (!File.Exists(FileLog))
            {
                //Criar Diretório de Log
                Directory.CreateDirectory(FileLog);
            }

            //ConnectAPI api = new ConnectAPI();
            var ConnectMySQLDB = ConfigurationManager.AppSettings["ConnectMySQLDB"];
            var DatabaseMySQLDB = ConfigurationManager.AppSettings["DatabaseMySQLDB"];
            var UserMySQLDB = ConfigurationManager.AppSettings["UserMySQLDB"];
            var PassMySQLDB = ConfigurationManager.AppSettings["PassMySQLDB"];
            var ConnectProdutos = ConfigurationManager.AppSettings["ConnectProdutos"];
            var ConnectPrecos = ConfigurationManager.AppSettings["ConnectPrecos"];
            var ConnectFiliais = ConfigurationManager.AppSettings["ConnectFiliais"];
            var ConnectClientes = ConfigurationManager.AppSettings["ConnectClientes"];
            var ConnectRepresentantes = ConfigurationManager.AppSettings["ConnectRepresentantes"];
            var ConnectMovSaidas = ConfigurationManager.AppSettings["ConnectMovSaidas"];
            var ConnectMovEntradas = ConfigurationManager.AppSettings["ConnectMovEntradas"];
            var ConnectProdutosEventos = ConfigurationManager.AppSettings["ConnectProdutosEventos"];
            var Authorization = ConfigurationManager.AppSettings["Authorization"];

            //Buscar Produtos e Incluir no Banco de dados MySQL.
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
                    var commandinsert1 = objConx0.CreateCommand();
                    commandinsert1.CommandText = "CREATE TABLE PRODUTOS (PRODUTO INT NOT NULL,COD_PRODUTO VARCHAR(30),DESCRICAO1 VARCHAR(255),REFERENCIA VARCHAR(255),ESTAMPA VARCHAR(255),COR VARCHAR(255),TAMANHO VARCHAR(5),COD_NCM VARCHAR(15),DEPARTAMENTOS VARCHAR(255),"
                                                 + "DIVISAO VARCHAR(255),GRUPO VARCHAR(255),TIPO VARCHAR(255),COLECAO VARCHAR(255),SUBCOLECAO VARCHAR(255),MARCA VARCHAR(255),CATEGORIA VARCHAR(255),STATUS VARCHAR(255),QMM INT,ALTURA VARCHAR(10),LARGURA VARCHAR(10),"
                                                 + "COMPRIMENTO VARCHAR(10),PEDIDO_BLOQUEADO VARCHAR(50),VENDA_BLOQUEADO VARCHAR(50),EAN13 VARCHAR(15),ESTOQUE_09 INT,ESTOQUE_11 INT,ESTOQUE_500 INT);";

                    commandinsert1.ExecuteNonQuery();
                    objConx0.Close();
                }

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
                    foreach (var prof in Prod.Value)
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
                            Console.WriteLine(e.Message);
                            Console.WriteLine($"ID Produto :  {prof.CodProduto} - {prof.Descricao1}");

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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - {e.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - {e.Message}");
                                }
                            }
                        }
                    }

                    //Buscar Precos e Incluir no Banco de dados MySQL.
                    try
                    {
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
                            foreach (var pre in Prec.Value)
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
                                    command1.CommandText = "INSERT INTO PRECOS (PRODUTO,COD_PRODUTO,DESCRICAO1,COD_EST,ESTAMPA,COD_COR,COR,TAMANHO,PRECO_214,PRECO_187,PRECO_224,PRECO_204,PRECO_205)" +
                                                                $"VALUES({pre.Produto}," + $"\"{pre.CodProduto}\", " + $"\"{pre.Descricao1}\", " + $"\"{pre.CodEst}\", " + $"\"{pre.Estampa}\", " + $"\"{pre.CodCor}\", " + $"\"{pre.Cor}\", " + $"\"{pre.Tamanho}\", "
                                                                         + $"{pre.Preco214}" + "," + $"{pre.Preco187}" + "," + $"{pre.Preco224}" + "," + $"{pre.Preco204}" + "," + $"{pre.Preco205}" + ")";

                                    command1.ExecuteNonQuery();
                                    objConx1.Close();

                                }
                                catch (MySqlException ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine($"ID Preco :  {pre.CodProduto} - {pre.Descricao1}");

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
                                        writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - {ex.Message}");
                                        writer1.Close();

                                    }
                                    //Verifica se o arquivo de Log já existe e inclui as informações.
                                    else
                                    {
                                        using (StreamWriter sw = File.AppendText(path))
                                        {
                                            sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - {ex.Message}");
                                        }
                                    }

                                }
                            }

                            //Buscar Filiais e Incluir no Banco de dados MySQL.
                            try
                            {
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
                                    foreach (var fili in Fil.Value)
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
                                            Console.WriteLine(ep.Message);
                                            Console.WriteLine($"ID Filial :  {fili.CodFilial} - {fili.Nome} - {fili.Cnpj}");

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
                                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Filial :  {fili.CodFilial} - {fili.Nome} - {fili.Cnpj} - {ep.Message}");
                                                writer1.Close();

                                            }
                                            //Verifica se o arquivo de Log já existe e inclui as informações.
                                            else
                                            {
                                                using (StreamWriter sw = File.AppendText(path))
                                                {
                                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Filial :  {fili.CodFilial} - {fili.Nome} - {fili.Cnpj} - {ep.Message}");
                                                }
                                            }
                                        }
                                    }

                                    //Buscar Clientes e Incluir no Banco de dados MySQL.
                                    try
                                    {
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
                                            foreach (var Cl in Cli.Value)
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
                                                    command3.CommandText = "INSERT INTO CLIENTES (CLIENTE,COD_CLIENTE,NOME,FANTASIA,CGC,CNPJ,IE,PF_PJ,TIPO_EMPRESA,E_MAIL,E_MAIL_NFE,GRUPO_LOJA,REPRESENTANTE,COD_ENDERECO,ENDERECO,BAIRRO,CIDADE,ESTADO,CEP,PAIS)" +
                                                                                $"VALUES({Cl.Cliente}," + $"\"{Cl.CodCliente}\"," + $"\"{Cl.Nome}\", " + $"\"{Cl.Fantasia}\", "
                                                                                + $"\"{Cl.Cgc}\", " + $"\"{Cl.Cnpj}\", " + $"\"{Cl.Ie}\", " + $"\"{Cl.PfPj}\"," + $"\"{Cl.TipoEmpresa}\", " + $"\"{Cl.EMail}\"," + $"\"{Cl.EMailNfe}\", "
                                                                                + $"\"{Cl.GrupoLoja}\", " + $"\"{Cl.Representante}\"," + $"\"{Cl.CodEndereco}\"," + $"\"{Cl.Endereco}\"," + $"\"{Cl.Bairro}\"," + $"\"{Cl.Cidade}\","
                                                                                + $"\"{Cl.Estado}\"," + $"\"{Cl.Cep}\"," + $"\"{Cl.Pais}\")";

                                                    command3.ExecuteNonQuery();
                                                    objConx3.Close();

                                                }
                                                catch (MySqlException ep)
                                                {
                                                    Console.WriteLine(ep.Message);
                                                    Console.WriteLine($"ID Cliente :  {Cl.CodCliente} - {Cl.Nome} - {Cl.Cnpj}");

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
                                                        writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Cliente :  {Cl.CodCliente} - {Cl.Nome} - {Cl.Cnpj} - {ep.Message}");
                                                        writer1.Close();

                                                    }
                                                    //Verifica se o arquivo de Log já existe e inclui as informações.
                                                    else
                                                    {
                                                        using (StreamWriter sw = File.AppendText(path))
                                                        {
                                                            sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Cliente :  {Cl.CodCliente} - {Cl.Nome} - {Cl.Cnpj} - {ep.Message}");
                                                        }
                                                    }
                                                }
                                            }

                                            //Buscar Representantes e Incluir no Banco de dados MySQL.
                                            try
                                            {
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
                                                    foreach (var Rp in Rep.Value)
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
                                                            Console.WriteLine(ep.Message);
                                                            Console.WriteLine($"ID Representante :  {Rp.CodRepresentante} - {Rp.Nome} - {Rp.Cnpj}");

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
                                                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Representante :  {Rp.CodRepresentante} - {Rp.Nome} - {Rp.Cnpj} - {ep.Message}");
                                                                writer1.Close();

                                                            }
                                                            //Verifica se o arquivo de Log já existe e inclui as informações.
                                                            else
                                                            {
                                                                using (StreamWriter sw = File.AppendText(path))
                                                                {
                                                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Representante :  {Rp.CodRepresentante} - {Rp.Nome} - {Rp.Cnpj} - {ep.Message}");
                                                                }
                                                            }
                                                        }
                                                    }

                                                    //Buscar Movimentações de Saidas e Incluir no Banco de dados MySQL.
                                                    try
                                                    {
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
                                                            foreach (var sd in Sale.Value)
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

                                                                    var command5 = objConx5.CreateCommand();
                                                                    command5.CommandText = "INSERT INTO SAIDAS (COD_OPERACAO,TIPO_OPERACAO,EVENTO,ROMANEIO,DATA,CLIENTE,COD_ENDERECO,CONDICOES_PGTO,FILIAL,CONTA,REPRESENTANTE,TRANSPORTADORA,PESO_B,PESO_L,QTDE,TOTAL,V_FRETE,VALOR_JUROS,VALOR_FINAL,ESPECIE_VOLUME,VOLUME)" +
                                                                                                $"VALUES({sd.CodOperacao}," + $"\"{sd.TipoOperacao}\"," + $"\"{sd.Evento}\", " + $"\"{sd.Romaneio}\", " + $"\"{sd.Data}\", "
                                                                                                + $"\"{sd.Cliente}\", " + $"\"{sd.CodigoEndereco}\", " + $"\"{sd.CondicoesPgto}\", " + $"{sd.Filial}, " + $"{sd.Conta}," + $"{sd.Representante},"
                                                                                                + $"\"{sd.Transportadora}\"," + $"\"{sd.PesoB}\"," + $"\"{sd.PesoL}\"," + $"{sd.Qtde}," + $"\"{sd.Total}\", "
                                                                                                + $"\"{sd.VFrete}\"," + $"\"{sd.ValorJuros}\"," + $"\"{sd.ValorFinal}\"," + $"\"{sd.EspecieVolume}\"," + $"\"{sd.Volume}\")";

                                                                    command5.ExecuteNonQuery();
                                                                    objConx5.Close();

                                                                }
                                                                catch (MySqlException ep)
                                                                {
                                                                    Console.WriteLine(ep.Message);
                                                                    Console.WriteLine($"ID Saida :  {sd.CodOperacao} - Romaneio: {sd.Romaneio} - Cliente: {sd.Cliente}");

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
                                                                        writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Saida :  {sd.CodOperacao} - Romaneio: {sd.Romaneio} - Cliente: {sd.Cliente} - {ep.Message}");
                                                                        writer1.Close();

                                                                    }
                                                                    //Verifica se o arquivo de Log já existe e inclui as informações.
                                                                    else
                                                                    {
                                                                        using (StreamWriter sw = File.AppendText(path))
                                                                        {
                                                                            sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Saida :  {sd.CodOperacao} - Romaneio: {sd.Romaneio} - Cliente: {sd.Cliente} - {ep.Message}");
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            //Buscar Movimentações de Entradas e Incluir no Banco de dados MySQL.
                                                            try
                                                            {
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
                                                                    foreach (var ed in Entre.Value)
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

                                                                            var command6 = objConx6.CreateCommand();
                                                                            command6.CommandText = "INSERT INTO ENTRADAS (COD_OPERACAO,TIPO_OPERACAO,EVENTO,ROMANEIO,DATA,CLIENTE,COD_ENDERECO,CONDICOES_PGTO,FILIAL,CONTA,REPRESENTANTE,TRANSPORTADORA,PESO_B,PESO_L,QTDE,TOTAL,V_FRETE,VALOR_JUROS,VALOR_FINAL,ESPECIE_VOLUME,VOLUME)" +
                                                                                                        $"VALUES({ed.CodOperacao}," + $"\"{ed.TipoOperacao}\"," + $"\"{ed.Evento}\", " + $"\"{ed.Romaneio}\", " + $"\"{ed.Data}\", "
                                                                                                        + $"\"{ed.Cliente}\", " + $"\"{ed.CodigoEndereco}\", " + $"\"{ed.CondicoesPgto}\", " + $"{ed.Filial}, " + $"{ed.Conta}," + $"{ed.Representante},"
                                                                                                        + $"\"{ed.Transportadora}\"," + $"\"{ed.PesoB}\"," + $"\"{ed.PesoL}\"," + $"{ed.Qtde}," + $"\"{ed.Total}\", "
                                                                                                        + $"\"{ed.VFrete}\"," + $"\"{ed.ValorJuros}\"," + $"\"{ed.ValorFinal}\"," + $"\"{ed.EspecieVolume}\"," + $"\"{ed.Volume}\")";

                                                                            command6.ExecuteNonQuery();
                                                                            objConx6.Close();

                                                                        }
                                                                        catch (MySqlException ent)
                                                                        {
                                                                            Console.WriteLine(ent.Message);
                                                                            Console.WriteLine($"ID Entrada :  {ed.CodOperacao} - Romaneio: {ed.Romaneio} - Cliente: {ed.Cliente}");

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
                                                                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Entrada :  {ed.CodOperacao} - Romaneio: {ed.Romaneio} - Cliente: {ed.Cliente} - {ent.Message}");
                                                                                writer1.Close();

                                                                            }
                                                                            //Verifica se o arquivo de Log já existe e inclui as informações.
                                                                            else
                                                                            {
                                                                                using (StreamWriter sw = File.AppendText(path))
                                                                                {
                                                                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Entrada :  {ed.CodOperacao} - Romaneio: {ed.Romaneio} - Cliente: {ed.Cliente} - { ent.Message}");
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    //Buscar os Produtos Eventos (Saidas / Entradas) e Incluir no Banco de dados MySQL.
                                                                    try
                                                                    {
                                                                        var requisicaoWeb7 = WebRequest.CreateHttp($"{ConnectProdutosEventos}" + "?$format=json");
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
                                                                            foreach (var ppo in pdv.Value)
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
                                                                                    command7.CommandText = "INSERT INTO PRODUTOS_EVENTOS (PRODUTO_EVENTO,COD_OPERACAO,TIPO_OPERACAO,PRODUTO,ESTAMPA,COR,TAMANHO,QUANTIDADE,PRECO,DESCONTO,V_ICMSS,V_ICMS,V_IPI,V_ISS,V_PIS,V_CONFINS)" +
                                                                                                                $"VALUES({ppo.ProdutoEvento}," + $"{ppo.CodOperacao}," + $"\"{ppo.TipoOperacao}\", " + $"{ppo.Produto}," + $"{ppo.Estampa}," 
                                                                                                                + $"{ppo.Cor}," + $"\"{ppo.Tamanho}\"," + $"{ppo.Quantidade}," + $"\"{ppo.Preco}\", " + $"\"{ppo.Desconto}\", " + $"\"{ppo.VIcmss}\", " 
                                                                                                                + $"\"{ppo.VIcms}\", " + $"\"{ppo.VIpi}\", " + $"\"{ppo.VIss}\", " + $"\"{ppo.VPis}\", " + $"\"{ppo.VConfins}\")";

                                                                                    command7.ExecuteNonQuery();
                                                                                    objConx7.Close();

                                                                                }
                                                                                catch (MySqlException etv)
                                                                                {
                                                                                    Console.WriteLine(etv.Message);
                                                                                    Console.WriteLine($"ID Produtos Eventos :  {ppo.CodOperacao} - Produto: {ppo.Produto}");

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
                                                                            }
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
                            catch (WebException e)
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
                        Console.WriteLine("Finalizando...");
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
                    Console.WriteLine("Concluído...");
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
