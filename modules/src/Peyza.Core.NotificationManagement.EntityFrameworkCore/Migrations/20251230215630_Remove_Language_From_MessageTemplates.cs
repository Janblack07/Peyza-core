using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peyza.Core.NotificationManagement.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Language_From_MessageTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                schema: "notification",
                table: "MessageTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "notification",
                table: "MessageTemplates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
