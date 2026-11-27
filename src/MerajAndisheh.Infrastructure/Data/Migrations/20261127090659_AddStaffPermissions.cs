using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MerajAndisheh.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffPermissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPermissions", x => new { x.UserId, x.PermissionKey });
                    table.ForeignKey(
                        name: "FK_StaffPermissions_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffPermissions");
        }
    }
}
