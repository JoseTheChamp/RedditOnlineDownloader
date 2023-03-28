using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebTesting.Migrations
{
    /// <inheritdoc />
    public partial class AddingTamplatesForReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowDownloaded = table.Column<bool>(type: "bit", nullable: false),
                    GroupBySubreddit = table.Column<bool>(type: "bit", nullable: false),
                    Nsfw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainsForm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Numbering = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubredditName = table.Column<bool>(type: "bit", nullable: false),
                    DomainName = table.Column<bool>(type: "bit", nullable: false),
                    PriorityName = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<int>(type: "int", nullable: false),
                    SubredditFolder = table.Column<bool>(type: "bit", nullable: false),
                    DomainFolder = table.Column<bool>(type: "bit", nullable: false),
                    PriorityFolder = table.Column<bool>(type: "bit", nullable: false),
                    Empty = table.Column<bool>(type: "bit", nullable: false),
                    Split = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Templates");
        }
    }
}
