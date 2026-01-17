using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endpoint.Engineering.ALaCarte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddALaCarteRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ALaCarteRequests",
                columns: table => new
                {
                    ALaCarteRequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OutputType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Directory = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SolutionName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: "ALaCarte.sln")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALaCarteRequests", x => x.ALaCarteRequestId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALaCarteRequests");
        }
    }
}
