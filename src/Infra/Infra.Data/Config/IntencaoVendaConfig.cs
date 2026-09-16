using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;

public class IntencaoVendaConfig : IEntityTypeConfiguration<IntencaoVenda>
{
    public void Configure(EntityTypeBuilder<IntencaoVenda> builder)
    {
        builder.ToTable("IntencaoVenda"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired(); builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.Telefone).HasMaxLength(30).IsRequired(); builder.Property(x => x.Cpf).HasMaxLength(20).IsRequired();
        builder.Property(x => x.PlanoNome).HasMaxLength(150).IsRequired(); builder.Property(x => x.ValorPlano).HasPrecision(18, 2);
        builder.Property(x => x.ValorTotal).HasPrecision(18, 2); builder.Property(x => x.Endereco).HasMaxLength(500);
        builder.Property(x => x.Dependentes).HasMaxLength(4000); builder.Property(x => x.Status).HasConversion<int>();
    }
}
