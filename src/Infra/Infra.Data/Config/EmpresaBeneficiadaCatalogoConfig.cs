using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;
namespace Infra.Data.Config;
public class EmpresaBeneficiadaCatalogoConfig : IEntityTypeConfiguration<EmpresaBeneficiadaEspecialidade>, IEntityTypeConfiguration<EmpresaBeneficiadaProcedimento>
{
    public void Configure(EntityTypeBuilder<EmpresaBeneficiadaEspecialidade> b) { b.ToTable("EmpresaBeneficiadaEspecialidade"); b.HasKey(x=>new{x.EmpresaBeneficiadaId,x.EspecialidadeId}); b.HasOne(x=>x.EmpresaBeneficiada).WithMany(x=>x.Especialidades).HasForeignKey(x=>x.EmpresaBeneficiadaId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Especialidade).WithMany().HasForeignKey(x=>x.EspecialidadeId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<EmpresaBeneficiadaProcedimento> b) { b.ToTable("EmpresaBeneficiadaProcedimento"); b.HasKey(x=>new{x.EmpresaBeneficiadaId,x.ProcedimentoId}); b.HasOne(x=>x.EmpresaBeneficiada).WithMany(x=>x.Procedimentos).HasForeignKey(x=>x.EmpresaBeneficiadaId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Procedimento).WithMany().HasForeignKey(x=>x.ProcedimentoId).OnDelete(DeleteBehavior.Restrict); }
}
