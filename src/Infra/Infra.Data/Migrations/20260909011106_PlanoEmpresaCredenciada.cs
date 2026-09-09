using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlanoEmpresaCredenciada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CredenciadoId",
                table: "Plano",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plano_CredenciadoId",
                table: "Plano",
                column: "CredenciadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plano_Credenciado_CredenciadoId",
                table: "Plano",
                column: "CredenciadoId",
                principalTable: "Credenciado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plano_Credenciado_CredenciadoId",
                table: "Plano");

            migrationBuilder.DropIndex(
                name: "IX_Plano_CredenciadoId",
                table: "Plano");

            migrationBuilder.DropColumn(
                name: "CredenciadoId",
                table: "Plano");
        }
    }
}
