using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class CadastroEmpresasBeneficiadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresaBeneficiada",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RazaoSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NomeFantasia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Cep = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    StatusCredenciamento = table.Column<int>(type: "int", nullable: false),
                    ImagemUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PlanoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaBeneficiada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaBeneficiada_Plano_PlanoId",
                        column: x => x.PlanoId,
                        principalTable: "Plano",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpresaBeneficiadaEspecialidade",
                columns: table => new
                {
                    EmpresaBeneficiadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EspecialidadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaBeneficiadaEspecialidade", x => new { x.EmpresaBeneficiadaId, x.EspecialidadeId });
                    table.ForeignKey(
                        name: "FK_EmpresaBeneficiadaEspecialidade_EmpresaBeneficiada_EmpresaBeneficiadaId",
                        column: x => x.EmpresaBeneficiadaId,
                        principalTable: "EmpresaBeneficiada",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpresaBeneficiadaEspecialidade_Especialidade_EspecialidadeId",
                        column: x => x.EspecialidadeId,
                        principalTable: "Especialidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmpresaBeneficiadaProcedimento",
                columns: table => new
                {
                    EmpresaBeneficiadaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcedimentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaBeneficiadaProcedimento", x => new { x.EmpresaBeneficiadaId, x.ProcedimentoId });
                    table.ForeignKey(
                        name: "FK_EmpresaBeneficiadaProcedimento_EmpresaBeneficiada_EmpresaBeneficiadaId",
                        column: x => x.EmpresaBeneficiadaId,
                        principalTable: "EmpresaBeneficiada",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpresaBeneficiadaProcedimento_Procedimento_ProcedimentoId",
                        column: x => x.ProcedimentoId,
                        principalTable: "Procedimento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaBeneficiada_Cnpj",
                table: "EmpresaBeneficiada",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaBeneficiada_PlanoId",
                table: "EmpresaBeneficiada",
                column: "PlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaBeneficiadaEspecialidade_EspecialidadeId",
                table: "EmpresaBeneficiadaEspecialidade",
                column: "EspecialidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaBeneficiadaProcedimento_ProcedimentoId",
                table: "EmpresaBeneficiadaProcedimento",
                column: "ProcedimentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpresaBeneficiadaEspecialidade");

            migrationBuilder.DropTable(
                name: "EmpresaBeneficiadaProcedimento");

            migrationBuilder.DropTable(
                name: "EmpresaBeneficiada");
        }
    }
}
