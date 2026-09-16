using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class BeneficiarioTipoPessoaEmpresaBeneficiada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmpresaBeneficiadaId",
                table: "Beneficiario",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPessoa",
                table: "Beneficiario",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Beneficiario_EmpresaBeneficiadaId",
                table: "Beneficiario",
                column: "EmpresaBeneficiadaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Beneficiario_EmpresaBeneficiada_EmpresaBeneficiadaId",
                table: "Beneficiario",
                column: "EmpresaBeneficiadaId",
                principalTable: "EmpresaBeneficiada",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Beneficiario_EmpresaBeneficiada_EmpresaBeneficiadaId",
                table: "Beneficiario");

            migrationBuilder.DropIndex(
                name: "IX_Beneficiario_EmpresaBeneficiadaId",
                table: "Beneficiario");

            migrationBuilder.DropColumn(
                name: "EmpresaBeneficiadaId",
                table: "Beneficiario");

            migrationBuilder.DropColumn(
                name: "TipoPessoa",
                table: "Beneficiario");
        }
    }
}
