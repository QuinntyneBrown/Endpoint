using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Endpoint.Engineering.ALaCarte.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepositoryConfigurations",
                columns: table => new
                {
                    RepositoryConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, defaultValue: "main"),
                    LocalDirectory = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryConfigurations", x => x.RepositoryConfigurationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepositoryConfigurations");
        }
    }
}
