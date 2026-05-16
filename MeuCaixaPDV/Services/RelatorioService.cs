using System;
using System.Collections.Generic;
using System.Linq;
using MeuCaixaPDV.Data;
using MeuCaixaPDV.Models;

namespace MeuCaixaPDV.Services;

public class RelatorioService
{
    private readonly AppDbContext _context;
    
    public RelatorioService()
    {
        _context = new AppDbContext();
        _context.Database.EnsureCreated();
    }
    
    public RelatorioVendas GerarRelatorioDia(DateTime data)
    {
        var vendas = _context.Vendas
            .Where(v => v.Data.Date == data.Date)
            .ToList();
        
        var relatorio = new RelatorioVendas
        {
            Data = data,
            TotalVendas = vendas.Count,
            ValorTotal = vendas.Sum(v => v.Total),
            VendasPorFormaPagamento = vendas
                .GroupBy(v => v.FormaPagamento)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total)),
            Vendas = vendas.Select(v => new VendaResumo
            {
                Id = v.Id,
                Data = v.Data,
                Total = v.Total,
                FormaPagamento = v.FormaPagamento,
                QuantidadeItens = v.Itens?.Count ?? 0
            }).ToList()
        };
        
        return relatorio;
    }
    
    public RelatorioVendas GerarRelatorioSemana(DateTime inicio)
    {
        var fim = inicio.AddDays(7);
        var vendas = _context.Vendas
            .Where(v => v.Data.Date >= inicio.Date && v.Data.Date < fim.Date)
            .ToList();
        
        return new RelatorioVendas
        {
            Data = inicio,
            TotalVendas = vendas.Count,
            ValorTotal = vendas.Sum(v => v.Total),
            VendasPorFormaPagamento = vendas
                .GroupBy(v => v.FormaPagamento)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total))
        };
    }
    
    public RelatorioVendas GerarRelatorioMes(int ano, int mes)
    {
        var vendas = _context.Vendas
            .Where(v => v.Data.Year == ano && v.Data.Month == mes)
            .ToList();
        
        return new RelatorioVendas
        {
            Data = new DateTime(ano, mes, 1),
            TotalVendas = vendas.Count,
            ValorTotal = vendas.Sum(v => v.Total),
            VendasPorFormaPagamento = vendas
                .GroupBy(v => v.FormaPagamento)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Total))
        };
    }
    
    public List<(DateTime Data, decimal Total)> GerarVendasUltimos7Dias()
    {
        var resultado = new List<(DateTime, decimal)>();
        
        for (int i = 6; i >= 0; i--)
        {
            var data = DateTime.Today.AddDays(-i);
            var total = _context.Vendas
                .Where(v => v.Data.Date == data.Date)
                .Sum(v => v.Total);
            
            resultado.Add((data, total));
        }
        
        return resultado;
    }
}