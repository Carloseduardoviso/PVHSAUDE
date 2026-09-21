using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPosicaoBannerPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[Banner]', N'Posicao') IS NULL
                BEGIN
                    ALTER TABLE [Banner] ADD [Posicao] int NOT NULL CONSTRAINT [DF_Banner_Posicao] DEFAULT 2;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Posicao",
                table: "Banner");
        }
    }
}
