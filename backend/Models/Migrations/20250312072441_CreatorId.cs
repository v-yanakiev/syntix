using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class CreatorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "execution_machine_template",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_execution_machine_template_CreatorId",
                table: "execution_machine_template",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_execution_machine_template_user_info_CreatorId",
                table: "execution_machine_template",
                column: "CreatorId",
                principalTable: "user_info",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_execution_machine_template_user_info_CreatorId",
                table: "execution_machine_template");

            migrationBuilder.DropIndex(
                name: "IX_execution_machine_template_CreatorId",
                table: "execution_machine_template");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "execution_machine_template");
        }
    }
}
