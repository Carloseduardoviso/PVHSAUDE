using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class GaleriaImagensCredenciado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CredenciadoImagem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredenciadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredenciadoImagem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredenciadoImagem_Credenciado_CredenciadoId",
                        column: x => x.CredenciadoId,
                        principalTable: "Credenciado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CredenciadoImagem_CredenciadoId_CriadoEm_Id",
                table: "CredenciadoImagem",
                columns: new[] { "CredenciadoId", "CriadoEm", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CredenciadoImagem");
        }
    }
}
