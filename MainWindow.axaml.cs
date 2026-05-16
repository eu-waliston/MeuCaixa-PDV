using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using MeuCaixaPDV.ViewModels;
using MeuCaixaPDV.Models;
using MeuCaixaPDV.Services;
using MeuCaixaPDV.Data;

namespace MeuCaixaPDV;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel = null!;
    private StackPanel _produtosContainer = null!;
    private StackPanel _resumoContainer = null!;
    private StackPanel _trocoPanel = null!;
    private TextBlock _totalText = null!;
    private TextBlock _statusText = null!;
    private TextBlock _trocoText = null!;
    private TextBox _codigoInput = null!;
    private TextBox _valorRecebidoInput = null!;
    
    public MainWindow()
    {
        InitializeComponent();
        
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        
        // Buscar controles
        _produtosContainer = this.FindControl<StackPanel>("ProdutosContainer")!;
        _resumoContainer = this.FindControl<StackPanel>("ResumoContainer")!;
        _trocoPanel = this.FindControl<StackPanel>("TrocoPanel")!;
        _totalText = this.FindControl<TextBlock>("TotalText")!;
        _statusText = this.FindControl<TextBlock>("StatusText")!;
        _trocoText = this.FindControl<TextBlock>("TrocoText")!;
        _codigoInput = this.FindControl<TextBox>("CodigoInput")!;
        _valorRecebidoInput = this.FindControl<TextBox>("ValorRecebidoInput")!;
        
        // Configurar botões
        var btnDinheiro = this.FindControl<Button>("BtnDinheiro")!;
        var btnPix = this.FindControl<Button>("BtnPix")!;
        var btnDebito = this.FindControl<Button>("BtnCartaoDebito")!;
        var btnCredito = this.FindControl<Button>("BtnCartaoCredito")!;
        var btnCalcularTroco = this.FindControl<Button>("BtnCalcularTroco")!;
        
        btnDinheiro.Click += (s, e) => SelecionarPagamento("Dinheiro");
        btnPix.Click += (s, e) => SelecionarPagamento("PIX");
        btnDebito.Click += (s, e) => SelecionarPagamento("Débito");
        btnCredito.Click += (s, e) => SelecionarPagamento("Crédito");
        btnCalcularTroco.Click += CalcularTroco;
        
        // Processar código de barras
        _codigoInput.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter)
            {
                var codigo = _codigoInput.Text;
                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    _viewModel.ProcessarCodigoExterno(codigo.Trim());
                    _codigoInput.Text = "";
                    AtualizarInterface();
                }
            }
        };
        
        // Atualizar interface quando a coleção mudar
        _viewModel.ItensVenda.CollectionChanged += (s, e) => AtualizarInterface();
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.Total))
            {
                _totalText.Text = _viewModel.Total.Replace("R$", "").Trim();
                AtualizarInterface();
            }
            if (e.PropertyName == nameof(_viewModel.MensagemStatus))
                _statusText.Text = _viewModel.MensagemStatus;
        };
        
        // Foco automático
        this.Loaded += (s, e) => _codigoInput?.Focus();
        
        // Inicializar
        AtualizarInterface();
        _statusText.Text = "✅ Sistema pronto! Passe o código do produto";
    }
    
    private void AtualizarInterface()
    {
        // Atualizar lista de produtos na esquerda
        _produtosContainer.Children.Clear();
        
        foreach (var item in _viewModel.ItensVenda)
        {
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0")),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0),
                Padding = new Thickness(15, 10, 15, 10)
            };
            
            var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto") };
            
            // Informações do produto
            var infoStack = new StackPanel();
            infoStack.Children.Add(new TextBlock { Text = item.Produto.Nome, FontWeight = FontWeight.Bold, FontSize = 14 });
            infoStack.Children.Add(new TextBlock { Text = $"Cód: {item.Produto.CodigoBarras}", FontSize = 11, Foreground = Brushes.Gray });
            
            // Preço e quantidade
            var detalhesStack = new StackPanel { Spacing = 5 };
            detalhesStack.Children.Add(new TextBlock { Text = $"R$ {item.Produto.Preco:F2}", FontSize = 14, Foreground = new SolidColorBrush(Color.Parse("#2C3E50")) });
            detalhesStack.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.Parse("#ECF0F1")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(8, 2, 8, 2),
                Child = new TextBlock { Text = $"Qtd: {item.Quantidade}", FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center }
            });
            
            // Botão remover
            var removeBtn = new Button
            {
                Content = "✕",
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.Parse("#E74C3C")),
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Cursor = new Cursor(StandardCursorType.Hand),
                Width = 30,
                Margin = new Thickness(10, 0, 0, 0)
            };
            
            var itemCapturado = item;
            removeBtn.Click += (s, e) => _viewModel.RemoverItemCommand.Execute(itemCapturado);
            
            Grid.SetColumn(infoStack, 0);
            Grid.SetColumn(detalhesStack, 1);
            Grid.SetColumn(removeBtn, 2);
            
            grid.Children.Add(infoStack);
            grid.Children.Add(detalhesStack);
            grid.Children.Add(removeBtn);
            
            card.Child = grid;
            _produtosContainer.Children.Add(card);
        }
        
        // Atualizar resumo na direita
        _resumoContainer.Children.Clear();
        
        foreach (var item in _viewModel.ItensVenda)
        {
            var linha = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
            linha.Children.Add(new TextBlock { Text = $"{item.Quantidade}x {item.Produto.Nome}", Foreground = Brushes.White, FontSize = 12 });
            linha.Children.Add(new TextBlock { Text = $"R$ {item.Subtotal:F2}", Foreground = Brushes.White, FontSize = 12, FontWeight = FontWeight.Bold });
            Grid.SetColumn(linha.Children[1], 1);
            _resumoContainer.Children.Add(linha);
        }
        
        if (!_viewModel.ItensVenda.Any())
        {
            _resumoContainer.Children.Add(new TextBlock { Text = "Nenhum item adicionado", Foreground = Brushes.Gray, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center });
        }
        
        _totalText.Text = _viewModel.ItensVenda.Sum(i => i.Subtotal).ToString("F2");
    }
    
    private void SelecionarPagamento(string tipo)
    {
        if (!_viewModel.ItensVenda.Any())
        {
            _statusText.Text = "❌ Adicione itens à venda primeiro!";
            return;
        }
        
        if (tipo == "Dinheiro")
        {
            _trocoPanel.IsVisible = true;
            _statusText.Text = "💰 Digite o valor recebido e calcule o troco";
            _valorRecebidoInput.Focus();
        }
        else
        {
            FinalizarVenda(tipo);
        }
    }
    
    private void CalcularTroco(object? sender, EventArgs e)
    {
        if (decimal.TryParse(_valorRecebidoInput.Text, out decimal valorRecebido))
        {
            var total = _viewModel.ItensVenda.Sum(i => i.Subtotal);
            var troco = valorRecebido - total;
            
            if (troco >= 0)
            {
                _trocoText.Text = $"Troco: R$ {troco:F2}";
                FinalizarVenda("Dinheiro", valorRecebido);
            }
            else
            {
                _trocoText.Text = "❌ Valor insuficiente!";
                _statusText.Text = "⚠️ Valor recebido é menor que o total";
            }
        }
        else
        {
            _statusText.Text = "❌ Digite um valor válido!";
        }
    }
    
    private async void FinalizarVenda(string tipo, decimal valorPago = 0)
    {
        var total = _viewModel.ItensVenda.Sum(i => i.Subtotal);
        var mensagem = tipo == "Dinheiro" ? 
            $"💵 Venda finalizada em DINHEIRO\nTotal: R$ {total:F2}\nValor pago: R$ {valorPago:F2}\nTroco: R$ {(valorPago - total):F2}" :
            $"✅ Venda finalizada em {tipo}\nTotal: R$ {total:F2}";
        
        // Dialog de confirmação
        var dialog = new Window
        {
            Title = "Venda Finalizada",
            Width = 400,
            Height = 250,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Children =
                {
                    new TextBlock { Text = "🛒 VENDA FINALIZADA!", FontSize = 18, FontWeight = FontWeight.Bold, Margin = new Thickness(0,0,0,15) },
                    new TextBlock { Text = mensagem, FontSize = 14, Margin = new Thickness(0,0,0,20) },
                    new Button { Content = "OK", Width = 100, HorizontalAlignment = HorizontalAlignment.Center }
                }
            }
        };
        
        ((Button)((StackPanel)dialog.Content).Children[2]).Click += (s, e) => dialog.Close();
        
        await dialog.ShowDialog(this);
        
        // Limpar venda
        _viewModel.ItensVenda.Clear();
        _trocoPanel.IsVisible = false;
        _trocoText.Text = "";
        _valorRecebidoInput.Text = "";
        _statusText.Text = "✅ Venda finalizada! Próximo cliente...";
        AtualizarInterface();
    }
}