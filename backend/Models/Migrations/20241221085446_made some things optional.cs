using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class madesomethingsoptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "execution_machine",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "machine_address",
                table: "execution_machine",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "app_address",
                table: "execution_machine",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine",
                column: "user_id",
                principalTable: "user_info",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "execution_machine",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "machine_address",
                table: "execution_machine",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "app_address",
                table: "execution_machine",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine",
                column: "user_id",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
