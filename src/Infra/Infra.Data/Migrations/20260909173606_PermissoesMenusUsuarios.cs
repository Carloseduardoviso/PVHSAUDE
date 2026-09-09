using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PVHSAUDE.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class PermissoesMenusUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MenusPermitidos",
                table: "Usuario",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
            // Preserve the access non-administrators had before menu permissions existed.
            migrationBuilder.Sql("UPDATE [Usuario] SET [MenusPermitidos] = 'Banner,Beneficiario,Credenciado,Especialidades,Plano,Procedimentos' WHERE [Role] IN (0, 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MenusPermitidos",
                table: "Usuario");
        }
    }
}
