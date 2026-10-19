using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BordspelApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPotluckSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPotluck",
                table: "GameNights",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PotluckItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsLactoseFree = table.Column<bool>(type: "bit", nullable: false),
                    IsNutFree = table.Column<bool>(type: "bit", nullable: false),
                    IsVegetarian = table.Column<bool>(type: "bit", nullable: false),
                    IsAlcoholFree = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PotluckItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PotluckItems_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PotluckItems_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PotluckItems_GameNightId",
                table: "PotluckItems",
                column: "GameNightId");

            migrationBuilder.CreateIndex(
                name: "IX_PotluckItems_PersonId",
                table: "PotluckItems",
                column: "PersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PotluckItems");

            migrationBuilder.DropColumn(
                name: "IsPotluck",
                table: "GameNights");
        }
    }
}
