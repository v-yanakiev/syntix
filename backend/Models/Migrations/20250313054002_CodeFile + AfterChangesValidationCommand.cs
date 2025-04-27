using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class CodeFileAfterChangesValidationCommand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "setup_command",
                table: "execution_machine_template",
                newName: "CodeFile");

            migrationBuilder.AddColumn<string>(
                name: "AfterChangesValidationCommand",
                table: "execution_machine_template",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterChangesValidationCommand",
                table: "execution_machine_template");

            migrationBuilder.RenameColumn(
                name: "CodeFile",
                table: "execution_machine_template",
                newName: "setup_command");
        }
    }
}
