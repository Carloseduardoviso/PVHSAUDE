using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;
namespace Infra.Data.Config;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Ativo).HasDefaultValue(true);
        builder.Property(x => x.NomeCompleto).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.EmailNormalizado).HasMaxLength(254).IsRequired();
        builder.HasIndex(x => x.EmailNormalizado).IsUnique();
        builder.Property(x => x.SenhaHash).HasMaxLength(512).IsRequired();
    }
}
