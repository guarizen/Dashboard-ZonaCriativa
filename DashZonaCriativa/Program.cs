using DashZonaCriativa.Produtos;
using DashZonaCriativa.Filiais;
using DashZonaCriativa.Clientes;
using DashZonaCriativa.Representantes;
using DashZonaCriativa.Movimentacoes;
using DashZonaCriativa.ProdutosEventos;
using DashZonaCriativa.Prefaturamentos;
using DashZonaCriativa.ProdutoPrefaturamento;
using DashZonaCriativa.TiposPedidos;
using DashZonaCriativa.PedidosVenda;
using DashZonaCriativa.NotasFiscais;
using ListaPrecosAPI;
using DashZonaCriativa.PedidoCompra;

namespace DashZonaCriativa
{
    class Program
    {
        static void Main(string[] args)
        {
            //Buscar Produtos e Incluir no Banco de dados MySQL.
            ProdutosGet po1 = new ProdutosGet();

            //Buscar Precos e Incluir no Banco de dados MySQL.
            ListaPrecosGet pr2 = new ListaPrecosGet();

            //Buscar Filiais e Incluir no Banco de dados MySQL.
            FiliaisGet fl3 = new FiliaisGet();

            //Buscar Clientes e Incluir no Banco de dados MySQL.
            ClientesGet cl4 = new ClientesGet();

            //Buscar Representantes e Incluir no Banco de dados MySQL.
            RepresentantesGet rp5 = new RepresentantesGet();

            //Buscar Movimentações de Saidas e Incluir no Banco de dados MySQL.
            SaidasGet sd6 = new SaidasGet();

            //Buscar Movimentações de Entradas e Incluir no Banco de dados MySQL.
            EntradasGet ed7 = new EntradasGet();

            //Buscar os Produtos Eventos (Saidas / Entradas) e Incluir no Banco de dados MySQL.
            ProdutosEventosGet pe8 = new ProdutosEventosGet();

            //Buscar os Prefaturamentos e Incluir no Banco de dados MySQL.
            PrefaturamentosGet pf9 = new PrefaturamentosGet();

            //Buscar os Produtos Prefaturamento e Incluir no Banco de dados MySQL.
            ProdutoPrefaturamentoGet ppf10 = new ProdutoPrefaturamentoGet();

            //Buscar os Tipos dos Pedidos e Incluir no Banco de dados MySQL.
            TipoPedidoGet tp11 = new TipoPedidoGet();

            //Busca Pedidos de Venda e Inclui no Banco de Dados MYSQL.
            PedidoVendaGet pv12 = new PedidoVendaGet();

            //Busca Produto dos Pedidos de Venda e Inclui no Banco de Dados MYSQL.
            ProdutoPedidoVendaGet ppv13 = new ProdutoPedidoVendaGet();

            //Busca Notas Fiscais e Inclui no Banco de Dados MYSQL.
            NotasFiscaisGet nf14 = new NotasFiscaisGet();

            //Busca Pedidos de Compra e Inclui no Banco de dados MYSQL.
            PedidoCompraGet pc15 = new PedidoCompraGet();

            //Busca Produto dos Pedidos de Compra e Inclui no Banco de Dados MYSQL.
            ProdutoPedidoCompraGet ppc16 = new ProdutoPedidoCompraGet();
        }
    }
}