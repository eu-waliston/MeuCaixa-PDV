using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MeuCaixaPDV.Models;
using MeuCaixaPDV.Data;
using System;


namespace MeuCaixaPDV.Services;

public class ProdutoService
{
    private readonly AppDbContext _context;

    public ProdutoService()
    {
        _context = new AppDbContext();
        
        // Criar o banco de dados se não existir
        _context.Database.EnsureCreated();
        
        // Adicionar produtos de exemplo se o banco estiver vazio
        if (!_context.Produtos.Any())
        {
            _context.Produtos.AddRange(new List<Produto>
            {
                new Produto { CodigoBarras = "789100001", Nome = "Arroz 5kg", Preco = 25.90m, Estoque = 10 },
                new Produto { CodigoBarras = "789100002", Nome = "Feijão 1kg", Preco = 8.50m, Estoque = 20 },
                new Produto { CodigoBarras = "789100003", Nome = "Açúcar 1kg", Preco = 4.75m, Estoque = 15 },
                new Produto { CodigoBarras = "789100004", Nome = "Café 500g", Preco = 12.90m, Estoque = 8 },
                new Produto { CodigoBarras = "789100005", Nome = "Leite 1L", Preco = 5.49m, Estoque = 12 },
                new Produto { CodigoBarras = "789100006", Nome = "Pão Francês", Preco = 0.75m, Estoque = 50 },
                new Produto { CodigoBarras = "789100007", Nome = "Margarina 500g", Preco = 7.90m, Estoque = 8 },
                new Produto { CodigoBarras = "789100008", Nome = "Refrigerante 2L", Preco = 9.99m, Estoque = 25 },
            });
            _context.SaveChanges();
        }
    }

    public Produto? BuscarPorCodigo(string codigo)
    {
        return _context.Produtos.FirstOrDefault(p => p.CodigoBarras == codigo);
    }

    public void AtualizarEstoque(string codigo, int quantidadeVendida)
    {
        var produto = BuscarPorCodigo(codigo);
        if (produto != null)
        {
            produto.Estoque -= quantidadeVendida;
            _context.SaveChanges();
        }
    }
    
    public List<Produto> ListarTodos()
    {
        return _context.Produtos.ToList();
    }
    
    public void SalvarProduto(Produto produto)
    {
        if (produto.Id == 0)
            _context.Produtos.Add(produto);
        else
            _context.Produtos.Update(produto);
        
        _context.SaveChanges();
    }
    
    public void SalvarVenda(Venda venda)
    {
        _context.Vendas.Add(venda);
        _context.SaveChanges();
    }
    
    public List<Venda> ListarVendas(DateTime? data = null)
    {
        var query = _context.Vendas.Include(v => v.Itens).AsQueryable();
        
        if (data.HasValue)
            query = query.Where(v => v.Data.Date == data.Value.Date);
        
        return query.OrderByDescending(v => v.Data).ToList();
    }
    
    public Venda? BuscarVenda(int id)
    {
        return _context.Vendas.Include(v => v.Itens).FirstOrDefault(v => v.Id == id);
    }
}