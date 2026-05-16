using System.ComponentModel.DataAnnotations;

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