using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;

public class CredenciadoImagemConfig : IEntityTypeConfiguration<CredenciadoImagem>
{
    public void Configure(EntityTypeBuilder<CredenciadoImagem> builder)
    {
        builder.ToTable("CredenciadoImagem");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CriadoEm).IsRequired();
        builder.HasIndex(x => new { x.CredenciadoId, x.CriadoEm, x.Id });
        builder.HasOne(x => x.Credenciado)
            .WithMany(x => x.Imagens)
            .HasForeignKey(x => x.CredenciadoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
