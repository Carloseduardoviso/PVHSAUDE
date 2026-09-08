using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogosRelacionamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especialidade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Procedimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procedimento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CredenciadoEspecialidade",
                columns: table => new
                {
                    CredenciadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EspecialidadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredenciadoEspecialidade", x => new { x.CredenciadoId, x.EspecialidadeId });
                    table.ForeignKey(
                        name: "FK_CredenciadoEspecialidade_Credenciado_CredenciadoId",
                        column: x => x.CredenciadoId,
                        principalTable: "Credenciado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CredenciadoEspecialidade_Especialidade_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "Especialidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CredenciadoProcedimento",
                columns: table => new
                {
                    CredenciadoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcedimentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredenciadoProcedimento", x => new { x.CredenciadoId, x.ProcedimentoId });
                    table.ForeignKey(
                        name: "FK_CredenciadoProcedimento_Credenciado_CredenciadoId",
                        column: x => x.CredenciadoId,
                        principalTable: "Credenciado",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CredenciadoProcedimento_Procedimento_ProcedimentoId",
                        column: x => x.ProcedimentoId,
                        principalTable: "Procedimento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CredenciadoEspecialidade_EspecialidadeId",
                table: "CredenciadoEspecialidade",
                column: "EspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_CredenciadoProcedimento_ProcedimentoId",
                table: "CredenciadoProcedimento",
                column: "ProcedimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Especialidade_Nome",
                table: "Especialidade",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Procedimento_Nome",
                table: "Procedimento",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CredenciadoEspecialidade");

            migrationBuilder.DropTable(
                name: "CredenciadoProcedimento");

            migrationBuilder.DropTable(
                name: "Especialidade");

            migrationBuilder.DropTable(
                name: "Procedimento");
        }
    }
}
