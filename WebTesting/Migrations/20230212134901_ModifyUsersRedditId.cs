using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTesting.Migrations
{
    /// <inheritdoc />
    public partial class ModifyUsersRedditId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedditId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedditId",
                table: "Users");
        }
    }
}
