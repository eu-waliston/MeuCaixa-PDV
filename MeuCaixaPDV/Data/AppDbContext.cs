using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MeuCaixaPDV.Models;

namespace MeuCaixaPDV.Data;

public class AppDbContext : DbContext
{
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Venda> Vendas { get; set; }
    public DbSet<ItemVendaDB> ItensVenda { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=minimercado.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar a relação entre Venda e ItensVenda
        modelBuilder.Entity<ItemVendaDB>()
            .HasOne<Venda>()
            .WithMany(v => v.Itens)
            .HasForeignKey(i => i.VendaId);
    }
}

public class Venda
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public decimal Total { get; set; }
    public string FormaPagamento { get; set; } = "";
    public List<ItemVendaDB> Itens { get; set; } = new();
}

public class ItemVendaDB
{
    public int Id { get; set; }
    public string ProdutoCodigo { get; set; } = "";
    public string ProdutoNome { get; set; } = "";
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
    public int VendaId { get; set; }
}