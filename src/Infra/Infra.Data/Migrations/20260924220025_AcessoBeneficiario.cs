using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AcessoBeneficiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BeneficiarioId",
                table: "Usuario",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpfSolicitado",
                table: "Usuario",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_BeneficiarioId",
                table: "Usuario",
                column: "BeneficiarioId",
                unique: true,
                filter: "[BeneficiarioId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuario_Beneficiario_BeneficiarioId",
                table: "Usuario",
                column: "BeneficiarioId",
                principalTable: "Beneficiario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuario_Beneficiario_BeneficiarioId",
                table: "Usuario");

            migrationBuilder.DropIndex(
                name: "IX_Usuario_BeneficiarioId",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "BeneficiarioId",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "CpfSolicitado",
                table: "Usuario");
        }
    }
}
