using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class madechatIdaGUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys first
            migrationBuilder.DropForeignKey(
                name: "chats_creator_id_fkey",
                table: "chat");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_template_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "messages_chat_id_fkey",
                table: "message");

            // Drop primary key constraint from chat table
            migrationBuilder.DropPrimaryKey(
                name: "chats_pkey",
                table: "chat");

            // Drop the columns
            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "message");

            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "execution_machine");

            migrationBuilder.DropColumn(
                name: "id",
                table: "chat");

            // Add new UUID columns
            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "chat",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid());

            // Add primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "chats_pkey",
                table: "chat",
                column: "id");

            migrationBuilder.AddColumn<Guid>(
                name: "chat_id",
                table: "message",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid());

            migrationBuilder.AddColumn<Guid>(
                name: "chat_id",
                table: "execution_machine",
                type: "uuid",
                nullable: true);

            // Add foreign keys back
            migrationBuilder.AddForeignKey(
                name: "FK_chat_user_info_creator_id",
                table: "chat",
                column: "creator_id",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_execution_machine_chat_chat_id",
                table: "execution_machine",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_execution_machine_execution_machine_template_execution_mach~",
                table: "execution_machine",
                column: "execution_machine_template_id",
                principalTable: "execution_machine_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_execution_machine_user_info_UserId",
                table: "execution_machine",
                column: "UserId",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_message_chat_chat_id",
                table: "message",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_chat_user_info_creator_id",
                table: "chat");

            migrationBuilder.DropForeignKey(
                name: "FK_execution_machine_chat_chat_id",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "FK_execution_machine_execution_machine_template_execution_mach~",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "FK_execution_machine_user_info_UserId",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "FK_message_chat_chat_id",
                table: "message");

            // Drop primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "chats_pkey",
                table: "chat");

            // Drop the UUID columns
            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "message");

            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "execution_machine");

            migrationBuilder.DropColumn(
                name: "id",
                table: "chat");

            // Add back the bigint columns
            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "chat",
                type: "bigint",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Add primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "chats_pkey",
                table: "chat",
                column: "id");

            migrationBuilder.AddColumn<long>(
                name: "chat_id",
                table: "message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "chat_id",
                table: "execution_machine",
                type: "bigint",
                nullable: true);

            // Add back the original foreign keys
            migrationBuilder.AddForeignKey(
                name: "chats_creator_id_fkey",
                table: "chat",
                column: "creator_id",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "execution_machine_chat_id_fkey",
                table: "execution_machine",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "execution_machine_template_id_fkey",
                table: "execution_machine",
                column: "execution_machine_template_id",
                principalTable: "execution_machine_template",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine",
                column: "UserId",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "messages_chat_id_fkey",
                table: "message",
                column: "chat_id",
                principalTable: "chat",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }


    }
}
