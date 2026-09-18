using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class DependenteCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Dependente",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql("WITH Maximo AS (SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING([Codigo], 3, CHARINDEX('/', [Codigo]) - 3))), 0) AS Numero FROM [Beneficiario]), Numerados AS (SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Id]) AS Numero FROM [Dependente]) UPDATE d SET [Codigo] = 'RO' + RIGHT('000' + CONVERT(varchar(10), m.Numero + n.Numero), 3) + '/' + CONVERT(varchar(4), YEAR(GETUTCDATE())) FROM [Dependente] d CROSS JOIN Maximo m INNER JOIN Numerados n ON n.[Id] = d.[Id]");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Dependente",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Dependente");
        }
    }
}
