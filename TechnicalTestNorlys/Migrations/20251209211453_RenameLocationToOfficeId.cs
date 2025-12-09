using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechnicalTestNorlys.Migrations
{
    /// <inheritdoc />
    public partial class RenameLocationToOfficeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Offices_Location",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_Location",
                table: "Persons");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Offices_Location",
                table: "Offices");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Persons");

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_OfficeId",
                table: "Persons",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Offices_OfficeId",
                table: "Persons",
                column: "OfficeId",
                principalTable: "Offices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Offices_OfficeId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_OfficeId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Persons");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Persons",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Offices_Location",
                table: "Offices",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Location",
                table: "Persons",
                column: "Location");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Offices_Location",
                table: "Persons",
                column: "Location",
                principalTable: "Offices",
                principalColumn: "Location",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
