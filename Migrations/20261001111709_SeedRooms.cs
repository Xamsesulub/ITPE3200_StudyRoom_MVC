using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MVC.Migrations
{
    /// <inheritdoc />
    public partial class SeedRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "RoomBuilding", "RoomCapacity", "RoomFloor", "RoomScreen", "RoomWhiteboard" },
                values: new object[,]
                {
                    { 101, "Pilestredet 35", 10, 1, true, false },
                    { 102, "Pilestredet 35", 6, 3, true, true },
                    { 103, "Pilestredet 35", 4, 4, false, true },
                    { 104, "Pilestredet 32", 4, 6, false, true },
                    { 105, "Pilestredet 48", 8, 2, true, true },
                    { 106, "Pilestredet 48", 2, 5, false, false },
                    { 107, "Holbergs terrasse", 12, 1, true, false },
                    { 108, "Holbergs terrasse", 6, 3, true, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 108);
        }
    }
}
