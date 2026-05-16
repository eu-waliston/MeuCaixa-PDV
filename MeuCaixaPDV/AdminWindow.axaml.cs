using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MeuCaixaPDV.Models;
using MeuCaixaPDV.Services;

namespace MeuCaixaPDV;

public partial class AdminWindow : Window
{
    private ProdutoService _produtoService;
    private ObservableCollection<Produto> _produtos = new();
    private Produto? _produtoSelecionado = null;
    private ListBox _listaProdutos;
    
    public AdminWindow()
    {
        InitializeComponent();
        
        _produtoService = new ProdutoService();
        _listaProdutos = this.FindControl<ListBox>("ListaProdutos")!;
        
        CarregarProdutos();
        
        // Configurar eventos
        var btnBuscar = this.FindControl<Button>("BtnBuscar");
        var txtBusca = this.FindControl<TextBox>("TxtBusca");
        var btnSalvar = this.FindControl<Button>("BtnSalvar");
        var btnCancelar = this.FindControl<Button>("BtnCancelar");
        
        if (btnBuscar != null)
            btnBuscar.Click += (s, e) => BuscarProdutos(txtBusca?.Text);
            
        if (txtBusca != null)
            txtBusca.TextChanged += (s, e) => BuscarProdutos(txtBusca.Text);
            
        if (btnSalvar != null)
            btnSalvar.Click += SalvarProduto;
            
        if (btnCancelar != null)
            btnCancelar.Click += LimparFormulario;
        
        _listaProdutos.SelectionChanged += (s, e) =>
        {
            if (_listaProdutos.SelectedItem is Produto produto)
            {
                CarregarProdutoNoFormulario(produto);
            }
        };
    }
    
    private void CarregarProdutos()
    {
        _produtos = new ObservableCollection<Produto>(_produtoService.ListarTodos());
        AtualizarListaVisual();
    }
    
    private void AtualizarListaVisual()
    {
        _listaProdutos.Items.Clear();
        
        foreach (var produto in _produtos)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Avalonia.Thickness(5) };
            
            var nomeBlock = new TextBlock 
            { 
                Text = produto.Nome, 
                FontWeight = FontWeight.Bold,
                Width = 250
            };
            
            var codigoBlock = new TextBlock 
            { 
                Text = produto.CodigoBarras, 
                FontSize = 11,
                Foreground = Brushes.Gray,
                Width = 150
            };
            
            var precoBlock = new TextBlock 
            { 
                Text = $"R$ {produto.Preco:F2}", 
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#27AE60")),
                Width = 100
            };
            
            panel.Children.Add(nomeBlock);
            panel.Children.Add(codigoBlock);
            panel.Children.Add(precoBlock);
            
            _listaProdutos.Items.Add(panel);
        }
    }
    
    private void BuscarProdutos(string? termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
        {
            CarregarProdutos();
            return;
        }
        
        var resultados = _produtoService.ListarTodos()
            .Where(p => p.Nome.ToLower().Contains(termo.ToLower()) ||
                       p.CodigoBarras.Contains(termo))
            .ToList();
        
        _produtos = new ObservableCollection<Produto>(resultados);
        AtualizarListaVisual();
    }
    
    private void CarregarProdutoNoFormulario(Produto produto)
    {
        _produtoSelecionado = produto;
        
        var txtCodigo = this.FindControl<TextBox>("TxtCodigo");
        var txtNome = this.FindControl<TextBox>("TxtNome");
        var txtPreco = this.FindControl<TextBox>("TxtPreco");
        var txtEstoque = this.FindControl<TextBox>("TxtEstoque");
        
        if (txtCodigo != null) txtCodigo.Text = produto.CodigoBarras;
        if (txtNome != null) txtNome.Text = produto.Nome;
        if (txtPreco != null) txtPreco.Text = produto.Preco.ToString("F2");
        if (txtEstoque != null) txtEstoque.Text = produto.Estoque.ToString();
    }
    
    private void SalvarProduto(object? sender, EventArgs e)
    {
        var txtCodigo = this.FindControl<TextBox>("TxtCodigo");
        var txtNome = this.FindControl<TextBox>("TxtNome");
        var txtPreco = this.FindControl<TextBox>("TxtPreco");
        var txtEstoque = this.FindControl<TextBox>("TxtEstoque");
        
        // Validar campos
        if (string.IsNullOrWhiteSpace(txtCodigo?.Text))
        {
            MostrarMensagem("Erro", "Digite o código de barras!");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(txtNome?.Text))
        {
            MostrarMensagem("Erro", "Digite o nome do produto!");
            return;
        }
        
        if (!decimal.TryParse(txtPreco?.Text, out decimal preco))
        {
            MostrarMensagem("Erro", "Digite um preço válido!");
            return;
        }
        
        if (!int.TryParse(txtEstoque?.Text, out int estoque))
        {
            estoque = 0;
        }
        
        var produto = new Produto
        {
            CodigoBarras = txtCodigo!.Text,
            Nome = txtNome!.Text,
            Preco = preco,
            Estoque = estoque
        };
        
        if (_produtoSelecionado != null)
        {
            produto.Id = _produtoSelecionado.Id;
        }
        
        _produtoService.SalvarProduto(produto);
        
        MostrarMensagem("Sucesso", "Produto salvo com sucesso!");
        LimparFormulario(null, null);
        CarregarProdutos();
    }
    
    private void LimparFormulario(object? sender, EventArgs? e)
    {
        var txtCodigo = this.FindControl<TextBox>("TxtCodigo");
        var txtNome = this.FindControl<TextBox>("TxtNome");
        var txtPreco = this.FindControl<TextBox>("TxtPreco");
        var txtEstoque = this.FindControl<TextBox>("TxtEstoque");
        
        if (txtCodigo != null) txtCodigo.Text = "";
        if (txtNome != null) txtNome.Text = "";
        if (txtPreco != null) txtPreco.Text = "";
        if (txtEstoque != null) txtEstoque.Text = "";
        
        _produtoSelecionado = null;
        _listaProdutos.SelectedItem = null;
    }
    
    private async void MostrarMensagem(string titulo, string mensagem)
    {
        var dialog = new Window
        {
            Title = titulo,
            Width = 300,
            Height = 150,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Children =
                {
                    new TextBlock { Text = mensagem, Margin = new Avalonia.Thickness(0,0,0,20) },
                    new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Center }
                }
            }
        };
        
        ((Button)((StackPanel)dialog.Content).Children[1]).Click += (s, e) => dialog.Close();
        await dialog.ShowDialog(this);
    }
}
