using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudAppServer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangedCloudFolderGuidToNullableInCloudFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CloudFiles_CloudFolders_CloudFolderId",
                table: "CloudFiles");

            migrationBuilder.AlterColumn<Guid>(
                name: "CloudFolderId",
                table: "CloudFiles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_CloudFiles_CloudFolders_CloudFolderId",
                table: "CloudFiles",
                column: "CloudFolderId",
                principalTable: "CloudFolders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CloudFiles_CloudFolders_CloudFolderId",
                table: "CloudFiles");

            migrationBuilder.AlterColumn<Guid>(
                name: "CloudFolderId",
                table: "CloudFiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CloudFiles_CloudFolders_CloudFolderId",
                table: "CloudFiles",
                column: "CloudFolderId",
                principalTable: "CloudFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
