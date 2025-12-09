using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechnicalTestNorlys.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Offices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offices", x => x.Id);
                    table.UniqueConstraint("AK_Offices_Location", x => x.Location);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Offices_Location",
                        column: x => x.Location,
                        principalTable: "Offices",
                        principalColumn: "Location",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Offices",
                columns: new[] { "Id", "Capacity", "Location" },
                values: new object[,]
                {
                    { 1, 5, "Silkeborg" },
                    { 2, 10, "Esbjerg" },
                    { 3, 50, "Aalborg" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offices_Location",
                table: "Offices",
                column: "Location",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Location",
                table: "Persons",
                column: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Offices");
        }
    }
}
