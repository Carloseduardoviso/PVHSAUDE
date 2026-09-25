using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config
{
    public class DependenteConfig : IEntityTypeConfiguration<Dependente>
    {
        public void Configure(EntityTypeBuilder<Dependente> builder)
        {
            builder.ToTable("Dependente");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
            builder.Property(x => x.BeneficiarioId).IsRequired();
            builder.Property(x => x.Nome).IsRequired();
            builder.Property(x => x.Cpf).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(254).IsRequired(false);
            builder.Property(x => x.DataNascimento).IsRequired();
            builder.Property(x => x.GrauParentesco).HasConversion(x => GrauParentescoStorage.ParaTexto(x), x => GrauParentescoStorage.ParaEnum(x)).HasColumnType("nvarchar(max)").IsRequired();

            builder.HasOne(x => x.Beneficiario)
                .WithMany(x => x.Dependentes)
                .HasForeignKey(x => x.BeneficiarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
