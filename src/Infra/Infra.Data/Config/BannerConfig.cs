using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PVHSAUDE.Domain.Entities;
namespace Infra.Data.Config;
public class BannerConfig : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Posicao).HasDefaultValue(PVHSAUDE.Domain.Enuns.PosicaoBanner.Central);
        b.Property(x => x.ContentType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Imagem).IsRequired();
    }
}
