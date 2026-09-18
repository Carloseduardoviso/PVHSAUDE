using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class DescontosECredenciadoDesconto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DescontoId",
                table: "Credenciado",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Desconto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Periodicidade = table.Column<int>(type: "int", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "date", nullable: true),
                    TipoPessoa = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Desconto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credenciado_DescontoId",
                table: "Credenciado",
                column: "DescontoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Credenciado_Desconto_DescontoId",
                table: "Credenciado",
                column: "DescontoId",
                principalTable: "Desconto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Credenciado_Desconto_DescontoId",
                table: "Credenciado");

            migrationBuilder.DropTable(
                name: "Desconto");

            migrationBuilder.DropIndex(
                name: "IX_Credenciado_DescontoId",
                table: "Credenciado");

            migrationBuilder.DropColumn(
                name: "DescontoId",
                table: "Credenciado");
        }
    }
}
