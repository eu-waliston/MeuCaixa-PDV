using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MeuCaixaPDV.Models;

public class Produto
{
    [Key]
    public int Id { get; set; }
    public string CodigoBarras { get; set; } = "";
    public string Nome { get; set; } = "";
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
}

public class ItemVenda
{
    public Produto Produto { get; set; } = new();
    public int Quantidade { get; set; } = 1;
    public decimal Subtotal => Produto.Preco * Quantidade;
}

public class RelatorioVendas
{
    public DateTime Data { get; set; }
    public int TotalVendas { get; set; }
    public decimal ValorTotal { get; set; }
    public Dictionary<string, decimal> VendasPorFormaPagamento { get; set; } = new();
    public List<VendaResumo> Vendas { get; set; } = new();
}

public class VendaResumo
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public decimal Total { get; set; }
    public string FormaPagamento { get; set; } = "";
    public int QuantidadeItens { get; set; }
}