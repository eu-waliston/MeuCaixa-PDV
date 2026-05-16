using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MeuCaixaPDV.Services;

namespace MeuCaixaPDV;

public partial class RelatorioWindow : Window
{
    private RelatorioService _relatorioService;
    private ListBox _listaVendas;
    private TextBlock _totalVendasText;
    private TextBlock _valorTotalText;
    private TextBlock _ticketMedioText;
    private StackPanel _pagamentosContainer;
    
    public RelatorioWindow()
    {
        InitializeComponent();
        
        _relatorioService = new RelatorioService();
        
        // Buscar controles
        _listaVendas = this.FindControl<ListBox>("ListaVendas")!;
        _totalVendasText = this.FindControl<TextBlock>("TotalVendasText")!;
        _valorTotalText = this.FindControl<TextBlock>("ValorTotalText")!;
        _ticketMedioText = this.FindControl<TextBlock>("TicketMedioText")!;
        _pagamentosContainer = this.FindControl<StackPanel>("PagamentosContainer")!;
        
        // Configurar botões
        var btnHoje = this.FindControl<Button>("BtnHoje")!;
        var btnSemana = this.FindControl<Button>("BtnSemana")!;
        var btnMes = this.FindControl<Button>("BtnMes")!;
        
        btnHoje.Click += (s, e) => CarregarRelatorioHoje();
        btnSemana.Click += (s, e) => CarregarRelatorioSemana();
        btnMes.Click += (s, e) => CarregarRelatorioMes();
        
        // Carregar relatório do dia atual
        CarregarRelatorioHoje();
    }
    
    private void CarregarRelatorioHoje()
    {
        var relatorio = _relatorioService.GerarRelatorioDia(DateTime.Today);
        AtualizarInterface(relatorio);
    }
    
    private void CarregarRelatorioSemana()
    {
        var inicio = DateTime.Today.AddDays(-6);
        var relatorio = _relatorioService.GerarRelatorioSemana(inicio);
        AtualizarInterface(relatorio);
    }
    
    private void CarregarRelatorioMes()
    {
        var relatorio = _relatorioService.GerarRelatorioMes(DateTime.Now.Year, DateTime.Now.Month);
        AtualizarInterface(relatorio);
    }
    
    private void AtualizarInterface(Models.RelatorioVendas relatorio)
    {
        // Atualizar cards
        _totalVendasText.Text = relatorio.TotalVendas.ToString();
        _valorTotalText.Text = $"R$ {relatorio.ValorTotal:F2}";
        
        var ticketMedio = relatorio.TotalVendas > 0 ? relatorio.ValorTotal / relatorio.TotalVendas : 0;
        _ticketMedioText.Text = $"R$ {ticketMedio:F2}";
        
        // Atualizar formas de pagamento
        _pagamentosContainer.Children.Clear();
        
        foreach (var pagamento in relatorio.VendasPorFormaPagamento)
        {
            var percentual = relatorio.ValorTotal > 0 ? (pagamento.Value / relatorio.ValorTotal) * 100 : 0;
            
            var linha = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto") };
            
            linha.Children.Add(new TextBlock { Text = pagamento.Key, FontSize = 13 });
            linha.Children.Add(new TextBlock { Text = $"R$ {pagamento.Value:F2}", FontWeight = FontWeight.Bold, HorizontalAlignment = HorizontalAlignment.Right });
            linha.Children.Add(new TextBlock { Text = $"({percentual:F1}%)", FontSize = 11, Foreground = Brushes.Gray, Margin = new Avalonia.Thickness(10,0,0,0) });
            
            Grid.SetColumn(linha.Children[1], 1);
            Grid.SetColumn(linha.Children[2], 2);
            
            _pagamentosContainer.Children.Add(linha);
        }
        
        if (!relatorio.VendasPorFormaPagamento.Any())
        {
            _pagamentosContainer.Children.Add(new TextBlock { Text = "Nenhuma venda no período", Foreground = Brushes.Gray });
        }
        
        // Atualizar lista de vendas
        _listaVendas.Items.Clear();
        
        foreach (var venda in relatorio.Vendas)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Avalonia.Thickness(5) };
            
            panel.Children.Add(new TextBlock { Text = venda.Data.ToString("dd/MM HH:mm"), Width = 100 });
            panel.Children.Add(new TextBlock { Text = $"{venda.QuantidadeItens} itens", Width = 80 });
            panel.Children.Add(new TextBlock { Text = venda.FormaPagamento, Width = 100 });
            panel.Children.Add(new TextBlock { Text = $"R$ {venda.Total:F2}", FontWeight = FontWeight.Bold, Width = 100 });
            
            _listaVendas.Items.Add(panel);
        }
        
        if (!relatorio.Vendas.Any())
        {
            _listaVendas.Items.Add(new TextBlock { Text = "Nenhuma venda encontrada", Foreground = Brushes.Gray });
        }
    }
}