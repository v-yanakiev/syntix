using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Models.Migrations
{
    /// <inheritdoc />
    public partial class stoptemplatetypefrombeingunique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_execution_machine_template_type",
                table: "execution_machine_template");

            migrationBuilder.CreateIndex(
                name: "IX_execution_machine_template_type",
                table: "execution_machine_template",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_execution_machine_template_type",
                table: "execution_machine_template");

            migrationBuilder.CreateIndex(
                name: "IX_execution_machine_template_type",
                table: "execution_machine_template",
                column: "type",
                unique: true);
        }
    }
}
