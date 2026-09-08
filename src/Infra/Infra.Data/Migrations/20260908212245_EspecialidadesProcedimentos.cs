using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EspecialidadesProcedimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CredenciadoId",
                table: "Beneficiario",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiario_CredenciadoId",
                table: "Beneficiario",
                column: "CredenciadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiario_Credenciado_CredenciadoId",
                table: "Beneficiario",
                column: "CredenciadoId",
                principalTable: "Credenciado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiario_Credenciado_CredenciadoId",
                table: "Beneficiario");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiario_CredenciadoId",
                table: "Beneficiario");

            migrationBuilder.DropColumn(
                name: "CredenciadoId",
                table: "Beneficiario");
        }
    }
}
