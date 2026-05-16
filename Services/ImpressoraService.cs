using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MeuCaixaPDV.Models;

namespace MeuCaixaPDV.Services;

public class ImpressoraService
{
    private string _nomeLoja = "MERCADO DO SEU ZÉ";
    private string _enderecoLoja = "Rua das Flores, 123 - centro";
    private string _telefoneLoja = "(55) 999999-9999";
    private string _cnpjLoja = "00.000.000/0001-00";

    // Comando ESC/POS
    private const byte ESC = 0x1B;
    private const byte GS = 0x1D;
    private const byte LF = 0x0A;

    // Método para imprimir cupom
    public void ImprimirCupom(List<ItemVenda> itens, decimal total, string formaPagamento, decimal valorPago = 0)
    {
        var builder = new StringBuilder();

        //Cabeçalho
        builder.AppendLine("\x18\x61\x01"); //centralizar
        builder.AppendLine("=".PadRight(48, '='));
        builder.AppendLine($"\x18\x45{_nomeLoja}\x18\x46"); //negrito
        builder.AppendLine(_enderecoLoja);
        builder.AppendLine($"Tel: {_telefoneLoja}");
        builder.AppendLine($"CNPJ: {_cnpjLoja}");
        builder.AppendLine("=".PadRight(48, '='));

        // Data e hora
        builder.AppendLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        builder.AppendLine($"CUPOM FISCAL - NÂO FISCAL");
        builder.AppendLine("-".PadRight(48, '-'));

        //Itens
        builder.AppendLine($"{"ITEM",-5} {"DESCRIÇÂO",-25} {"QTD",-5} {"R$",10}");
        builder.AppendLine("-".PadRight(48, '-'));

        int itemNum = 1;
        foreach (var item in itens)
        {
            string descricao = item.Produto.Nome.Length > 22 ? item.Produto.Nome.Substring(0, 22) : item.Produto.Nome.PadRight(22);

            string linha = $"{itemNum,-5} {descricao} {item.Quantidade,-5} {item.Subtotal,10:F2}";
            builder.AppendLine(linha);
            itemNum++;
        }

        builder.AppendLine("-".PadRight(48, '-'));

        // Totais
        builder.AppendLine($"{"SUBTOTAL:",-40} {total,8:F2}");

        if (formaPagamento == "Dinheiro" && valorPago > 0)
        {
            var troco = valorPago - total;
            builder.AppendLine($"{"VALOR PAGO",-40} {valorPago,8:F2}");
            builder.AppendLine($"{"TROCO:",-40} {troco,8:F2}");
        }

        builder.AppendLine($"{"TOTAL",-40} {total,8:F2}");
        builder.AppendLine("=".PadRight(48, '='));

        //Forma de pagamento
        builder.AppendLine($"FORAM DE PAGAMENTO: {formaPagamento}");
        builder.AppendLine("=".PadRight(48, '='));

        //Rodapé
        builder.AppendLine("OBRIGADO PELA COMPRA!");
        builder.AppendLine("VOLTE SEMPRE!");
        builder.AppendLine("=".PadRight(48, '='));
        builder.AppendLine("\x1B\x61\x00"); // alinhamento esquerdo

        // Linhas em branco para corte
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine();

        //Enviar para impressora
        EnviarParaImpressora(builder.ToString());
    }

    private void EnviarParaImpressora(string conteudo)
    {
        // Em Linux, geralmente a impressora térmica está em /dev/usb/lp0
        // Em Windows, é uma porta COM ou USB

        // Por enquanto, salvar em arquivo para teste
        string caminhoArquivo = $"cumpom_{DateTime.Now:yyyyMMdd_HHmmss}.text";
        File.WriteAllText(caminhoArquivo, conteudo, Encoding.UTF8);

        System.Diagnostics.Debug.WriteLine($"Cumpo salvo em: {caminhoArquivo}");
        System.Diagnostics.Debug.WriteLine(conteudo);

        // TODO: Implementar envio real para impressora
        // Em Linux: File.WriteAllText("/dev/usb/lp0", conteudo);
        // Em Windows: Usar RawPrinterHelper
    }

    // Método para testar impressão
    public void TestarImpressao()
    {
        var conteudo = new StringBuilder();
        conteudo.AppendLine("=".PadRight(48, '='));
        conteudo.AppendLine("TESTE DE IMPRESSÃO");
        conteudo.AppendLine("=".PadRight(48, '='));
        conteudo.AppendLine("Se você está vendo isso,");
        conteudo.AppendLine("a impressora está funcionando!");
        conteudo.AppendLine("=".PadRight(48, '='));
        conteudo.AppendLine();
        conteudo.AppendLine();

        EnviarParaImpressora(conteudo.ToString());
    }
}