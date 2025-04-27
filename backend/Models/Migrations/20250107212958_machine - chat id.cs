using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class machinechatid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "execution_machine",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_execution_machine_user_id",
                table: "execution_machine",
                newName: "IX_execution_machine_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "app_address",
                table: "execution_machine",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "chat_id",
                table: "execution_machine",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_execution_machine_chat_id",
                table: "execution_machine",
                column: "chat_id");

            migrationBuilder.AddForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropIndex(
                name: "IX_execution_machine_chat_id",
                table: "execution_machine");

            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "execution_machine");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "execution_machine",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_execution_machine_UserId",
                table: "execution_machine",
                newName: "IX_execution_machine_user_id");

            migrationBuilder.AlterColumn<string>(
                name: "app_address",
                table: "execution_machine",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
