using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class OnDeleteDeleteBehaviorCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_template_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

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
                column: "user_id",
                principalTable: "user_info",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "execution_machine_template_id_fkey",
                table: "execution_machine");

            migrationBuilder.DropForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine");

            migrationBuilder.AddForeignKey(
                name: "execution_machine_template_id_fkey",
                table: "execution_machine",
                column: "execution_machine_template_id",
                principalTable: "execution_machine_template",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "execution_machine_user_id_fkey",
                table: "execution_machine",
                column: "user_id",
                principalTable: "user_info",
                principalColumn: "id");
        }
    }
}
