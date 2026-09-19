using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;
namespace Infra.Data.Config;
public class CredenciadoConfig : IEntityTypeConfiguration<Credenciado>
{
    public void Configure(EntityTypeBuilder<Credenciado> builder)
    {
        builder.ToTable("Credenciado");
        builder.HasOne(x => x.Plano).WithMany().HasForeignKey(x => x.PlanoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Desconto).WithMany().HasForeignKey(x => x.DescontoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.DataCadastro).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.RazaoSocial).HasMaxLength(150).IsRequired(true);
        builder.Property(x => x.NomeFantasia).HasMaxLength(150).IsRequired(true);
        builder.Property(x => x.Cnpj).HasMaxLength(14).IsRequired(true);
        builder.Property(x => x.Telefone).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.WhatsApp).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired(false);
        builder.Property(x => x.Cep).HasMaxLength(9).IsRequired(false);
        builder.Property(x => x.Endereco).HasMaxLength(250).IsRequired(false);
        builder.Property(x => x.Cidade).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.Uf).HasMaxLength(2).IsRequired(false);
        builder.Property(x => x.Observacoes).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.ImagemUrl).HasMaxLength(500);
        builder.HasIndex(x => x.Cnpj).IsUnique();
        builder.Property(x => x.Tipo).HasConversion<int>();
        builder.Property(x => x.StatusCredenciamento).HasConversion<int>();
    }
}
