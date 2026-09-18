using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class BeneficiarioCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Beneficiario",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.Sql("UPDATE [Beneficiario] SET [Codigo] = 'RO001/' + CONVERT(varchar(4), DATEPART(year, [DataAdesao])) WHERE [Codigo] IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Beneficiario",
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
                table: "Beneficiario");
        }
    }
}
