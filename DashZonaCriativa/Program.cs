﻿using ListaClientesAPI;
using ListaEntradasAPI;
using ListaFiliaisAPI;
using ListaPrecosAPI;
using ListaPrefaturamentosAPI;
using ListaProdutosAPI;
using ListaProdutosEventosAPI;
using ListaProdutosPrefatAPI;
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
            var ConnectPrefaturamentos = ConfigurationManager.AppSettings["ConnectPrefaturamentos"];
            var ConnectProdutosPrefat = ConfigurationManager.AppSettings["ConnectProdutosPrefat"];
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
                    Console.WriteLine("Criando Tabela de Produtos...");
                    var commandinsert1 = objConx0.CreateCommand();
                    commandinsert1.CommandText = "CREATE TABLE PRODUTOS (PRODUTO INT NOT NULL,COD_PRODUTO VARCHAR(30),DESCRICAO1 VARCHAR(255),REFERENCIA VARCHAR(255),ESTAMPA VARCHAR(255),COR VARCHAR(255),TAMANHO VARCHAR(5),COD_NCM VARCHAR(15),DEPARTAMENTOS VARCHAR(255),"
                                                 + "DIVISAO VARCHAR(255),GRUPO VARCHAR(255),TIPO VARCHAR(255),COLECAO VARCHAR(255),SUBCOLECAO VARCHAR(255),MARCA VARCHAR(255),CATEGORIA VARCHAR(255),STATUS VARCHAR(255),QMM INT,ALTURA VARCHAR(10),LARGURA VARCHAR(10),"
                                                 + "COMPRIMENTO VARCHAR(10),PEDIDO_BLOQUEADO VARCHAR(50),VENDA_BLOQUEADO VARCHAR(50),EAN13 VARCHAR(15),ESTOQUE_09 INT,ESTOQUE_11 INT,ESTOQUE_500 INT);";

                    commandinsert1.ExecuteNonQuery();
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
                                writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - Erro: {e.Message}");
                                writer1.Close();

                            }
                            //Verifica se o arquivo de Log já existe e inclui as informações.
                            else
                            {
                                using (StreamWriter sw = File.AppendText(path))
                                {
                                    sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto :  {prof.CodProduto} - {prof.Descricao1} - Erro: {e.Message}");
                                }
                            }
                        }
                    }

                    //Buscar Precos e Incluir no Banco de dados MySQL.
                    try
                    {
                        MySqlConnection objConx01 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                        objConx01.Open();

                        var command01 = objConx01.CreateCommand();
                        command01.CommandText = "SELECT COUNT(*) AS N FROM PRECOS";
                        var retCommnad1 = command01.ExecuteReaderAsync();
                        string fault1 = retCommnad1.Status.ToString();

                        if (fault1 == "Faulted")
                        {
                            Console.WriteLine("Criando Tabela de Precos...");
                            var CommandInsert2 = objConx01.CreateCommand();
                            CommandInsert2.CommandText = "CREATE TABLE PRECOS (PRODUTO INT NOT NULL,COD_PRODUTO VARCHAR(30),DESCRICAO1 VARCHAR(255),ESTAMPA VARCHAR(255),COR VARCHAR(255),"
                                                        + "TAMANHO VARCHAR(5),PRECO_214 DECIMAL(14,2),PRECO_187 DECIMAL(14,2),PRECO_224 DECIMAL(14,2),PRECO_204 DECIMAL(14,2),PRECO_205 DECIMAL(14,2));";

                            CommandInsert2.ExecuteNonQuery();
                            objConx01.Close();
                        }
                        objConx01.Close();

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
                                    command1.CommandText = "INSERT INTO PRECOS (PRODUTO,COD_PRODUTO,DESCRICAO1,ESTAMPA,COR,TAMANHO,PRECO_214,PRECO_187,PRECO_224,PRECO_204,PRECO_205)" +
                                                                $"VALUES({pre.Produto}," + $"\"{pre.CodProduto}\", " + $"\"{pre.Descricao1}\", " + $"\"{pre.Estampa}\", " + $"\"{pre.Cor}\", " + $"\"{pre.Tamanho}\", "
                                                                         + $"{pre.Preco214}" + "," + $"{pre.Preco187}" + "," + $"{pre.Preco224}" + "," + $"{pre.Preco204}" + "," + $"{pre.Preco205}" + ")";

                                    command1.ExecuteNonQuery();
                                    objConx1.Close();

                                }
                                catch (MySqlException ex)
                                {
                                    Console.WriteLine($"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - Erro: {ex.Message}");

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
                                        writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - Erro: {ex.Message}");
                                        writer1.Close();

                                    }
                                    //Verifica se o arquivo de Log já existe e inclui as informações.
                                    else
                                    {
                                        using (StreamWriter sw = File.AppendText(path))
                                        {
                                            sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Preco :  {pre.CodProduto} - {pre.Descricao1} - Erro: {ex.Message}");
                                        }
                                    }

                                }
                            }

                            //Buscar Filiais e Incluir no Banco de dados MySQL.
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
                                    }

                                    //Buscar Clientes e Incluir no Banco de dados MySQL.
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
                                                                        + "PAIS VARCHAR(255));";
                                            CommandInsert3.ExecuteNonQuery();
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
                                            }

                                            //Buscar Representantes e Incluir no Banco de dados MySQL.
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
                                                    }

                                                    //Buscar Movimentações de Saidas e Incluir no Banco de dados MySQL.
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
                                                            Console.WriteLine("Criando Tabela de Movimentacoes de Saidas...");
                                                            var CommandInsert5 = objConx05.CreateCommand();
                                                            CommandInsert5.CommandText = "CREATE TABLE SAIDAS (COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),EVENTO INT,ROMANEIO VARCHAR(60),DATA VARCHAR(25),CLIENTE INT,COD_ENDERECO INT,CONDICOES_PGTO VARCHAR(255),"
                                                                                        + "FILIAL INT,CONTA INT,REPRESENTANTE INT,TRANSPORTADORA VARCHAR(255),PESO_B VARCHAR(25),PESO_L VARCHAR(25),QTDE INT,TOTAL DECIMAL(14, 2),V_FRETE DECIMAL(14,2),VALOR_JUROS DECIMAL(14,2),"
                                                                                        + "VALOR_FINAL DECIMAL(14,2),ESPECIE_VOLUME VARCHAR(255),VOLUME VARCHAR(25));";
                                                            CommandInsert5.ExecuteNonQuery();
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
                                                            }

                                                            //Buscar Movimentações de Entradas e Incluir no Banco de dados MySQL.
                                                            try
                                                            {
                                                                MySqlConnection objConx06 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                                                                objConx06.Open();

                                                                var command06 = objConx06.CreateCommand();
                                                                command06.CommandText = "SELECT COUNT(*) AS N FROM SAIDAS";
                                                                var retCommnad6 = command06.ExecuteReaderAsync();
                                                                string fault6 = retCommnad6.Status.ToString();

                                                                if (fault6 == "Faulted")
                                                                {
                                                                    Console.WriteLine("Criando Tabela de Movimentacoes de Saidas...");
                                                                    var CommandInsert6 = objConx06.CreateCommand();
                                                                    CommandInsert6.CommandText = "CREATE TABLE ENTRADAS (COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),EVENTO INT,ROMANEIO VARCHAR(60),DATA VARCHAR(25),CLIENTE INT,COD_ENDERECO INT,CONDICOES_PGTO VARCHAR(255),FILIAL INT,"
                                                                                                + "CONTA INT,REPRESENTANTE INT,TRANSPORTADORA VARCHAR(255),PESO_B VARCHAR(25),PESO_L VARCHAR(25),QTDE INT,TOTAL DECIMAL(14, 2),V_FRETE DECIMAL(14,2),VALOR_JUROS DECIMAL(14,2),VALOR_FINAL DECIMAL(14,2),"
                                                                                                + "ESPECIE_VOLUME VARCHAR(255),VOLUME VARCHAR(25));";
                                                                    CommandInsert6.ExecuteNonQuery();
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
                                                                    }
                                                                    //Buscar os Produtos Eventos (Saidas / Entradas) e Incluir no Banco de dados MySQL.
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
                                                                            CommandInsert7.CommandText = "CREATE TABLE PRODUTOS_EVENTOS (PRODUTO_EVENTO INT NOT NULL,COD_OPERACAO INT NOT NULL,TIPO_OPERACAO VARCHAR(5),PRODUTO INT,ESTAMPA INT,COR INT,TAMANHO VARCHAR(5),QUANTIDADE INT NOT NULL,"
                                                                                                        + "PRECO DECIMAL(14,2),DESCONTO DECIMAL(14,2),V_ICMSS DECIMAL(14,2),V_ICMS DECIMAL(14,2),V_IPI DECIMAL(14,2),V_ISS DECIMAL(14,2),V_PIS DECIMAL(14,2),V_CONFINS DECIMAL(14,2));";
                                                                            CommandInsert7.ExecuteNonQuery();
                                                                            objConx07.Close();
                                                                        }
                                                                        objConx07.Close();

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
                                                                            }
                                                                            //Buscar os Prefaturamentos e Incluir no Banco de dados MySQL.
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
                                                                                    CommandInsert8.CommandText = "CREATE TABLE PREFATURAMENTOS (PREFATURAMENTO INT NOT NULL,NUMERO VARCHAR(30),DATA VARCHAR(20),FILIAL INT,CLIENTE INT,PEDIDOV INT,EXPEDICAO VARCHAR(1),DATA_EXPEDICAO VARCHAR(20),PODECONFERIR VARCHAR(1),DATA_PODECONFERIR VARCHAR(20),"
                                                                                                                + "CONFERINDO VARCHAR(1),CONFERIDO VARCHAR(1),DATACONFERIDO VARCHAR(20),ENTREGUE VARCHAR(1),TRANSPORTADORA VARCHAR(255),OBS_CLI_FAT VARCHAR(255));";
                                                                                    CommandInsert8.ExecuteNonQuery();
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
                                                                                    foreach (var ptf in pft.Value)
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

                                                                                            var command8 = objConx8.CreateCommand();
                                                                                            command8.CommandText = "INSERT INTO PREFATURAMENTOS (PREFATURAMENTO,NUMERO,DATA,FILIAL,CLIENTE,PEDIDOV,EXPEDICAO,DATA_EXPEDICAO,PODECONFERIR,DATA_PODECONFERIR,CONFERINDO,CONFERIDO,DATACONFERIDO,ENTREGUE,TRANSPORTADORA,OBS_CLI_FAT)" +
                                                                                                                        $"VALUES({ptf.Prefaturamento}," + $"\"{ptf.Numero}\", " + $"\"{ptf.Data}\", " + $"{ptf.Filial}," + $"{ptf.Cliente},"
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
                                                                                    }
                                                                                    //Buscar os Produtos Prefaturamento e Incluir no Banco de dados MySQL.
                                                                                    try
                                                                                    {
                                                                                        MySqlConnection objConx09 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                                                                                        objConx09.Open();

                                                                                        var command09 = objConx09.CreateCommand();
                                                                                        command09.CommandText = "SELECT COUNT(*) AS N FROM PRODUTO_PREFAT";
                                                                                        var retCommnad9 = command09.ExecuteReaderAsync();
                                                                                        string fault9 = retCommnad9.Status.ToString();

                                                                                        if (fault9 == "Faulted")
                                                                                        {
                                                                                            Console.WriteLine("Criando Tabela de Produto Prefaturamento...");
                                                                                            var CommandInsert9 = objConx09.CreateCommand();
                                                                                            CommandInsert9.CommandText = "CREATE TABLE PRODUTO_PREFAT (PRODUTO_PREFAT INT NOT NULL,PREFATURAMENTO INT,PRODUTO INT,ESTAMPA INT,COR INT,TAMANHO VARCHAR(5),QUANTIDADE INT,ENTREGUE INT,SAIDA INT);";
                                                                                            CommandInsert9.ExecuteNonQuery();
                                                                                            objConx09.Close();
                                                                                        }
                                                                                        objConx09.Close();

                                                                                        var requisicaoWeb9 = WebRequest.CreateHttp($"{ConnectProdutosPrefat}" + "?$format=json");
                                                                                        requisicaoWeb9.Method = "GET";
                                                                                        requisicaoWeb9.Headers.Add("Authorization", $"{Authorization}");
                                                                                        requisicaoWeb9.UserAgent = "RequisicaoAPIGET";
                                                                                        requisicaoWeb9.Timeout = 1300000;
                                                                                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                                                                                        Console.WriteLine("Listando e Incluindo Produtos Prefaturamentos...");
                                                                                        using (var resposta9 = requisicaoWeb9.GetResponse())
                                                                                        {
                                                                                            var streamDados9 = resposta9.GetResponseStream();
                                                                                            StreamReader reader9 = new StreamReader(streamDados9);
                                                                                            object objResponse9 = reader9.ReadToEnd();
                                                                                            var statusCodigo9 = ((System.Net.HttpWebResponse)resposta9).StatusCode;

                                                                                            ListaProdutosPrefat ppft = JsonConvert.DeserializeObject<ListaProdutosPrefat>(objResponse9.ToString());

                                                                                            int pppref = 0;
                                                                                            foreach (var pptf in ppft.Value)
                                                                                            {
                                                                                                try
                                                                                                {
                                                                                                    MySqlConnection objConx9 = new MySqlConnection($"Server={ConnectMySQLDB};Database={DatabaseMySQLDB};Uid={UserMySQLDB};Pwd={PassMySQLDB}");
                                                                                                    objConx9.Open();

                                                                                                    while (pppref < 1)
                                                                                                    {
                                                                                                        if (pppref == 1)
                                                                                                            break;
                                                                                                        var commandelet = objConx9.CreateCommand();
                                                                                                        commandelet.CommandText = "DELETE FROM PRODUTO_PREFAT";
                                                                                                        commandelet.ExecuteNonQuery();
                                                                                                        pppref++;
                                                                                                    }

                                                                                                    var command9 = objConx9.CreateCommand();
                                                                                                    command9.CommandText = "INSERT INTO PRODUTO_PREFAT (PRODUTO_PREFAT,PREFATURAMENTO,PRODUTO,ESTAMPA,COR,TAMANHO,QUANTIDADE,ENTREGUE,SAIDA)" +
                                                                                                                                $"VALUES({pptf.ProdutoPrefat}," + $"{pptf.Prefaturamento}," + $"{pptf.Produto}," + $"{pptf.Estampa}," + $"{pptf.Cor}," + $"\"{pptf.Tamanho}\","
                                                                                                                                + $"{pptf.Quantidade}," + $"{pptf.Entregue}," + $"{pptf.Saida}" + ")";

                                                                                                    command9.ExecuteNonQuery();
                                                                                                    objConx9.Close();

                                                                                                }
                                                                                                catch (MySqlException etv)
                                                                                                {
                                                                                                    Console.WriteLine($"ID Produto Prefat :  {pptf.ProdutoPrefat} - Prefaturamento: {pptf.Prefaturamento} - Erro: {etv.Message}");

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
                                                                                                        writer1.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto Prefat :  {pptf.ProdutoPrefat} - Prefaturamento: {pptf.Prefaturamento} - Erro: {etv.Message}");
                                                                                                        writer1.Close();

                                                                                                    }
                                                                                                    //Verifica se o arquivo de Log já existe e inclui as informações.
                                                                                                    else
                                                                                                    {
                                                                                                        using (StreamWriter sw = File.AppendText(path))
                                                                                                        {
                                                                                                            sw.WriteLine($"Data: {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss")}" + " - " + $"ID Produto Prefat :  {pptf.ProdutoPrefat} - Prefaturamento: {pptf.Prefaturamento} - Erro: {etv.Message}");
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
