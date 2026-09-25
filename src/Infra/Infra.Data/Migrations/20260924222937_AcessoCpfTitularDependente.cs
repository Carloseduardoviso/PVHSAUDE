using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AcessoCpfTitularDependente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuario_BeneficiarioId",
                table: "Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_BeneficiarioId_CpfSolicitado",
                table: "Usuario",
                columns: new[] { "BeneficiarioId", "CpfSolicitado" },
                unique: true,
                filter: "[BeneficiarioId] IS NOT NULL AND [CpfSolicitado] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuario_BeneficiarioId_CpfSolicitado",
                table: "Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_BeneficiarioId",
                table: "Usuario",
                column: "BeneficiarioId",
                unique: true,
                filter: "[BeneficiarioId] IS NOT NULL");
        }
    }
}
