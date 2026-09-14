using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;

public class ConfiguracaoWhatsAppConfig : IEntityTypeConfiguration<ConfiguracaoWhatsApp>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoWhatsApp> builder)
    {
        builder.ToTable("ConfiguracaoWhatsApp", table => table.HasCheckConstraint("CK_ConfiguracaoWhatsApp_Unico", "[Id] = 1"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Mensagem).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Telefone).HasMaxLength(15).IsRequired();
    }
}
