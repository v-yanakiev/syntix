using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class _2addresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "url",
                table: "execution_machine",
                newName: "machine_address");

            migrationBuilder.AddColumn<string>(
                name: "app_address",
                table: "execution_machine",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "app_address",
                table: "execution_machine");

            migrationBuilder.RenameColumn(
                name: "machine_address",
                table: "execution_machine",
                newName: "url");
        }
    }
}
