using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class SetNullconstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

            migrationBuilder.AddForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine",
                column: "UserId",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

            migrationBuilder.AddForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine",
                column: "UserId",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
