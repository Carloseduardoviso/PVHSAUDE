using Infra.Data.Base;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations;

[DbContext(typeof(Context))]
[Migration("20260916100000_PlanoNotificacaoValidade")]
public partial class PlanoNotificacaoValidade : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "NotificacaoValidadeSuspensa",
            table: "Plano",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NotificacaoValidadeSuspensa",
            table: "Plano");
    }
}
