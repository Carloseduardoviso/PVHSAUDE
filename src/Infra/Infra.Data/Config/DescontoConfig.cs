using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;
public class DescontoConfig : IEntityTypeConfiguration<Desconto>
{
    public void Configure(EntityTypeBuilder<Desconto> builder)
    {
        builder.ToTable("Desconto"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired(); builder.Property(x => x.Descricao).HasMaxLength(1000);
        builder.Property(x => x.DataValidade).HasColumnType("date").IsRequired(false); builder.Property(x => x.Valor).HasPrecision(18, 2);
        builder.Property(x => x.TipoPessoa).HasConversion<int>(); builder.Property(x => x.Periodicidade).HasConversion<int>();
    }
}
