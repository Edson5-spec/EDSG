using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDSG.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioFieldsToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PortfolioUrl",
                table: "AspNetUsers",
                newName: "PortfolioFileName");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioFile",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortfolioFile",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PortfolioFileName",
                table: "AspNetUsers",
                newName: "PortfolioUrl");
        }
    }
}
