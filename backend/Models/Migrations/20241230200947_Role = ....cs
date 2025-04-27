using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "messages_sender_id_fkey",
                table: "message");

            migrationBuilder.RenameColumn(
                name: "sender_id",
                table: "message",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_message_sender_id",
                table: "message",
                newName: "IX_message_SenderId");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "message",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_message_user_info_SenderId",
                table: "message",
                column: "SenderId",
                principalTable: "user_info",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_message_user_info_SenderId",
                table: "message");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "message");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "message",
                newName: "sender_id");

            migrationBuilder.RenameIndex(
                name: "IX_message_SenderId",
                table: "message",
                newName: "IX_message_sender_id");

            migrationBuilder.AddForeignKey(
                name: "messages_sender_id_fkey",
                table: "message",
                column: "sender_id",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
