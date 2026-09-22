using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class DescontoComoCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataValidade",
                table: "Desconto");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Desconto");

            migrationBuilder.DropColumn(
                name: "Periodicidade",
                table: "Desconto");

            migrationBuilder.DropColumn(
                name: "TipoPessoa",
                table: "Desconto");

            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Desconto");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Desconto",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Desconto");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidade",
                table: "Desconto",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Desconto",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Periodicidade",
                table: "Desconto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoPessoa",
                table: "Desconto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Desconto",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
