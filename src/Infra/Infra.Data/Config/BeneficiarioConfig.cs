using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config
{
    public class BeneficiarioConfig : IEntityTypeConfiguration<Beneficiario>
    {
        public void Configure(EntityTypeBuilder<Beneficiario> builder)
        {
            builder.ToTable("Beneficiario");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Nome).IsRequired();
            builder.Property(x => x.Cpf).IsRequired();
            builder.Property(x => x.DataNascimento).IsRequired();
            builder.Property(x => x.DataAdesao).IsRequired();
            builder.Property(x => x.DataInicio).IsRequired();
            builder.Property(x => x.DataValidade).IsRequired();
            builder.Property(x => x.CredenciadoId).IsRequired(false);
            builder.HasOne(x => x.Credenciado).WithMany().HasForeignKey(x => x.CredenciadoId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.TipoPessoa).HasConversion<int>().IsRequired();
            builder.Property(x => x.EmpresaBeneficiadaId).IsRequired(false);
            builder.HasOne(x => x.EmpresaBeneficiada).WithMany().HasForeignKey(x => x.EmpresaBeneficiadaId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.PlanoId).IsRequired();
            builder.Property(x => x.Telefone).IsRequired(false);
            builder.Property(x => x.Email).IsRequired(false);
            builder.Property(x => x.Endereco).IsRequired(false);
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        }
    }
}
