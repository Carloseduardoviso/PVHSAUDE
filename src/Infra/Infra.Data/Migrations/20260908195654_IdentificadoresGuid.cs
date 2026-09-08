using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations;

public partial class IdentificadoresGuid : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>("NovoId", "Beneficiario", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>("NovoPlanoId", "Beneficiario", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>("NovoId", "Dependente", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<Guid>("NovoBeneficiarioId", "Dependente", type: "uniqueidentifier", nullable: true);

        // Generate one GUID per existing record and one per distinct legacy plan.
        migrationBuilder.Sql("""
            UPDATE [Beneficiario] SET [NovoId] = NEWID();
            SELECT [PlanoId], NEWID() AS [NovoPlanoId]
            INTO #PlanosGuid
            FROM (SELECT DISTINCT [PlanoId] FROM [Beneficiario]) AS planos;
            UPDATE b SET [NovoPlanoId] = p.[NovoPlanoId]
            FROM [Beneficiario] b JOIN #PlanosGuid p ON p.[PlanoId] = b.[PlanoId];
            DROP TABLE #PlanosGuid;
            UPDATE d SET [NovoId] = NEWID(), [NovoBeneficiarioId] = b.[NovoId]
            FROM [Dependente] d JOIN [Beneficiario] b ON b.[Id] = d.[BeneficiarioId];
            """);

        migrationBuilder.DropForeignKey("FK_Dependente_Beneficiario_BeneficiarioId", "Dependente");
        migrationBuilder.DropIndex("IX_Dependente_BeneficiarioId", "Dependente");
        migrationBuilder.DropPrimaryKey("PK_Dependente", "Dependente");
        migrationBuilder.DropPrimaryKey("PK_Beneficiario", "Beneficiario");
        migrationBuilder.DropColumn("BeneficiarioId", "Dependente");
        migrationBuilder.DropColumn("Id", "Dependente");
        migrationBuilder.DropColumn("Id", "Beneficiario");
        migrationBuilder.DropColumn("PlanoId", "Beneficiario");

        migrationBuilder.RenameColumn("NovoId", "Beneficiario", "Id");
        migrationBuilder.RenameColumn("NovoPlanoId", "Beneficiario", "PlanoId");
        migrationBuilder.RenameColumn("NovoId", "Dependente", "Id");
        migrationBuilder.RenameColumn("NovoBeneficiarioId", "Dependente", "BeneficiarioId");

        migrationBuilder.AlterColumn<Guid>("Id", "Beneficiario", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
        migrationBuilder.AlterColumn<Guid>("PlanoId", "Beneficiario", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
        migrationBuilder.AlterColumn<Guid>("Id", "Dependente", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);
        migrationBuilder.AlterColumn<Guid>("BeneficiarioId", "Dependente", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

        migrationBuilder.AddPrimaryKey("PK_Beneficiario", "Beneficiario", "Id");
        migrationBuilder.AddPrimaryKey("PK_Dependente", "Dependente", "Id");
        migrationBuilder.CreateIndex("IX_Dependente_BeneficiarioId", "Dependente", "BeneficiarioId");
        migrationBuilder.AddForeignKey("FK_Dependente_Beneficiario_BeneficiarioId", "Dependente", "BeneficiarioId", "Beneficiario", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("A conversão para GUID não pode recuperar os identificadores inteiros originais. Para reverter, restaure o backup anterior à migration.");
}
