using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyIncidentsApp.Migrations
{
    /// <inheritdoc />
    public partial class isIsResolvedProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "Incidents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "Incidents");
        }
    }
}
