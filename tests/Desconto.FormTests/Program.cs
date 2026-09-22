using PVHSAUDE.Domain.Entities;

var desconto = new Desconto("  Desconto de consulta  ");
if (desconto.Nome != "Desconto de consulta" || !desconto.Ativo)
    throw new Exception("O desconto deve guardar apenas um nome normalizado e começar ativo.");

desconto.Atualizar("  Desconto de exame  ", false);
if (desconto.Nome != "Desconto de exame" || desconto.Ativo)
    throw new Exception("A exclusão lógica deve preservar o desconto e atualizar seu estado.");

Console.WriteLine("Contrato de desconto: OK");
