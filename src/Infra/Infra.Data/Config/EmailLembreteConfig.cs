using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;

public class EmailLembreteConfig : IEntityTypeConfiguration<EmailLembrete>
{
    public void Configure(EntityTypeBuilder<EmailLembrete> builder)
    {
        builder.ToTable("EmailLembrete");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Destinatario).HasMaxLength(254).IsRequired();
        builder.Property(x => x.DataValidade).HasColumnType("date").IsRequired();
        builder.Property(x => x.Enviado).HasDefaultValue(false);
        builder.HasIndex(x => new { x.PessoaId, x.DataValidade, x.DiasAntes }).IsUnique();
    }
}
