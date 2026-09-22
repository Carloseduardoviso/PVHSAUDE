using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;
public class DescontoConfig : IEntityTypeConfiguration<Desconto>
{
    public void Configure(EntityTypeBuilder<Desconto> builder)
    {
        builder.ToTable("Desconto"); builder.HasKey(x => x.Id); builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
    }
}
