using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;

namespace Infra.Data.Config;

public class CatalogoConfig : IEntityTypeConfiguration<Especialidade>, IEntityTypeConfiguration<Procedimento>, IEntityTypeConfiguration<CredenciadoEspecialidade>, IEntityTypeConfiguration<CredenciadoProcedimento>
{
    public void Configure(EntityTypeBuilder<Especialidade> b) { b.ToTable("Especialidade"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).ValueGeneratedNever(); b.Property(x=>x.Nome).HasMaxLength(150).IsRequired(); b.HasIndex(x=>x.Nome).IsUnique(); }
    public void Configure(EntityTypeBuilder<Procedimento> b) { b.ToTable("Procedimento"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).ValueGeneratedNever(); b.Property(x=>x.Nome).HasMaxLength(150).IsRequired(); b.HasIndex(x=>x.Nome).IsUnique(); }
    public void Configure(EntityTypeBuilder<CredenciadoEspecialidade> b) { b.ToTable("CredenciadoEspecialidade"); b.HasKey(x=>new{x.CredenciadoId,x.EspecialidadeId}); b.HasOne(x=>x.Credenciado).WithMany(x=>x.Especialidades).HasForeignKey(x=>x.CredenciadoId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Especialidade).WithMany(x=>x.Credenciados).HasForeignKey(x=>x.EspecialidadeId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<CredenciadoProcedimento> b) { b.ToTable("CredenciadoProcedimento"); b.HasKey(x=>new{x.CredenciadoId,x.ProcedimentoId}); b.HasOne(x=>x.Credenciado).WithMany(x=>x.Procedimentos).HasForeignKey(x=>x.CredenciadoId).OnDelete(DeleteBehavior.Cascade); b.HasOne(x=>x.Procedimento).WithMany(x=>x.Credenciados).HasForeignKey(x=>x.ProcedimentoId).OnDelete(DeleteBehavior.Restrict); }
}
