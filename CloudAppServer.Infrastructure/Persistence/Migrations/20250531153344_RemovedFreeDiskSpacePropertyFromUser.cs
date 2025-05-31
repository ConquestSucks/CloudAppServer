using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudAppServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFreeDiskSpacePropertyFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiskSpaceLeft",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FreeDiskSpace",
                table: "Users",
                newName: "DiskSpace");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiskSpace",
                table: "Users",
                newName: "FreeDiskSpace");

            migrationBuilder.AddColumn<decimal>(
                name: "DiskSpaceLeft",
                table: "Users",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
