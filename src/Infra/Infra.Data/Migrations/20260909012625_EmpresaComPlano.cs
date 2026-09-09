using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmpresaComPlano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT CredenciadoId FROM Plano WHERE CredenciadoId IS NOT NULL GROUP BY CredenciadoId HAVING COUNT(*) > 1)
                    THROW 50001, 'Existe empresa vinculada a mais de um plano. Defina um único plano por empresa antes de aplicar esta migration.', 1;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_Plano_Credenciado_CredenciadoId",
                table: "Plano");

            migrationBuilder.DropIndex(
                name: "IX_Plano_CredenciadoId",
                table: "Plano");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanoId",
                table: "Credenciado",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                EXEC(N'UPDATE empresa SET PlanoId = plano.Id
                FROM Credenciado empresa INNER JOIN Plano plano ON plano.CredenciadoId = empresa.Id;');
                """);

            migrationBuilder.DropColumn(
                name: "CredenciadoId",
                table: "Plano");

            migrationBuilder.CreateIndex(
                name: "IX_Credenciado_PlanoId",
                table: "Credenciado",
                column: "PlanoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Credenciado_Plano_PlanoId",
                table: "Credenciado",
                column: "PlanoId",
                principalTable: "Plano",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT PlanoId FROM Credenciado WHERE PlanoId IS NOT NULL GROUP BY PlanoId HAVING COUNT(*) > 1)
                    THROW 50002, 'Não é possível reverter: existem empresas compartilhando o mesmo plano.', 1;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_Credenciado_Plano_PlanoId",
                table: "Credenciado");

            migrationBuilder.DropIndex(
                name: "IX_Credenciado_PlanoId",
                table: "Credenciado");

            migrationBuilder.AddColumn<Guid>(
                name: "CredenciadoId",
                table: "Plano",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                EXEC(N'UPDATE plano SET CredenciadoId = empresa.Id
                FROM Plano plano INNER JOIN Credenciado empresa ON empresa.PlanoId = plano.Id;');
                """);

            migrationBuilder.DropColumn(
                name: "PlanoId",
                table: "Credenciado");

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
    }
}
