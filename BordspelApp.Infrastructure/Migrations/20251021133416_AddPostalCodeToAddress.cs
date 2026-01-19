using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostalCodeToAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_PostalCode",
                table: "GameNights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GameNights",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "GameNights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Address_PostalCode",
                table: "GameNights");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "GameNights");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "GameNights");
        }
    }
}
