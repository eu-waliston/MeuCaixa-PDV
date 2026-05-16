using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MeuCaixaPDV.Models;
using MeuCaixaPDV.Services;

namespace MeuCaixaPDV.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ProdutoService _produtoService = new();
    private string _codigoBarras = "";
    private string _mensagemStatus = "Aguardando leitura...";
    private string _total = "R$ 0,00";

    public ObservableCollection<ItemVenda> ItensVenda { get; } = new();

    public string CodigoBarras
    {
        get => _codigoBarras;
        set
        {
            _codigoBarras = value;
            OnPropertyChanged();
        }
    }

    public string MensagemStatus
    {
        get => _mensagemStatus;
        set { _mensagemStatus = value; OnPropertyChanged(); }
    }

    public string Total
    {
        get => _total;
        set { _total = value; OnPropertyChanged(); }
    }

    public ICommand ProcessarCodigoCommand { get; }
    public ICommand RemoverItemCommand { get; }
    public ICommand FinalizarVendaCommand { get; }

    public MainWindowViewModel()
    {
        ProcessarCodigoCommand = new RelayCommand<string>(ProcessarCodigo);
        RemoverItemCommand = new RelayCommand<ItemVenda>(RemoverItem);
        FinalizarVendaCommand = new RelayCommand(FinalizarVenda);
    }

    private void ProcessarCodigo(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return;

        var produto = _produtoService.BuscarPorCodigo(codigo);

        if (produto == null)
        {
            MensagemStatus = $"❌ Produto não encontrado: {codigo}";
            return;
        }

        if (produto.Estoque <= 0)
        {
            MensagemStatus = $"⚠️ Produto sem estoque: {produto.Nome}";
            return;
        }

        var itemExistente = ItensVenda.FirstOrDefault(i => i.Produto.CodigoBarras == codigo);

        if (itemExistente != null)
        {
            itemExistente.Quantidade++;
        }
        else
        {
            ItensVenda.Add(new ItemVenda { Produto = produto });
        }

        AtualizarTotal();
        MensagemStatus = $"✅ {produto.Nome} adicionado!";
    }

    private void RemoverItem(ItemVenda? item)
    {
        if (item != null)
        {
            ItensVenda.Remove(item);
            AtualizarTotal();
            MensagemStatus = $"Item removido: {item.Produto.Nome}";
        }
    }

    private void AtualizarTotal()
    {
        var totalCalculado = ItensVenda.Sum(i => i.Subtotal);
        Total = totalCalculado.ToString("C");
    }

    private void FinalizarVenda()
    {
        if (!ItensVenda.Any())
        {
            MensagemStatus = "Não há itens na venda!";
            return;
        }

        MensagemStatus = $"💰 Total: {Total} - Aguardando pagamento...";
    }

    // Método público para ser chamado pelo MainWindow
    public void ProcessarCodigoExterno(string codigo)
    {
        ProcessarCodigo(codigo);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void AtualizarEstoque(string codigo, int quantidade)
    {
        _produtoService.AtualizarEstoque(codigo, quantidade);
    }
}

// Helper para comandos com parâmetro
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
    public void Execute(object? parameter) => _execute((T?)parameter);

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}

// Helper para comandos sem parâmetro
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}

// Classe auxiliar para o CommandManager
public static class CommandManager
{
    public static event EventHandler? RequerySuggested;

    public static void InvalidateRequerySuggested()
    {
        RequerySuggested?.Invoke(null, EventArgs.Empty);
    }
}

