using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlanoValidadeOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasValidade",
                table: "Plano");

            migrationBuilder.DropColumn(
                name: "TipoPessoa",
                table: "Plano");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataValidade",
                table: "Plano",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataValidade",
                table: "Plano");

            migrationBuilder.AddColumn<int>(
                name: "DiasValidade",
                table: "Plano",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPessoa",
                table: "Plano",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
