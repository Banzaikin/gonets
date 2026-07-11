using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonBackend.Migrations
{
    /// <inheritdoc />
    public partial class Migrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncomingMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SystemId = table.Column<string>(type: "text", nullable: false),
                    From = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TypeMessage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutgoingMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    To = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TypeMessage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutgoingMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SentMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    To = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TypeMessage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncomingMessages_SystemId",
                table: "IncomingMessages",
                column: "SystemId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncomingMessages");

            migrationBuilder.DropTable(
                name: "OutgoingMessages");

            migrationBuilder.DropTable(
                name: "SentMessages");
        }
    }
}
