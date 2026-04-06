using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTruyen.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentChapterSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChapterName",
                table: "Comments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChapterName",
                table: "Comments");
        }
    }
}
