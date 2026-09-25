using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmailDependenteELembretes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Dependente",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailLembrete",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PessoaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "date", nullable: false),
                    DiasAntes = table.Column<int>(type: "int", nullable: false),
                    Destinatario = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Enviado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EnviadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLembrete", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLembrete_PessoaId_DataValidade_DiasAntes",
                table: "EmailLembrete",
                columns: new[] { "PessoaId", "DataValidade", "DiasAntes" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLembrete");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Dependente");
        }
    }
}
