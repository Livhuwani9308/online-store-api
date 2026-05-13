using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace online_store_api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsWhereApplicable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Users");
        }
    }
}
