using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_for_all_groups",
                table: "submission_slots",
                newName: "allow_all_groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "allow_all_groups",
                table: "submission_slots",
                newName: "is_for_all_groups");
        }
    }
}
