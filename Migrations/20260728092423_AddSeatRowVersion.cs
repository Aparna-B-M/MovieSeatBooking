using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieSeatBooking.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_IdempotencyKey",
                table: "Bookings",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_IdempotencyKey",
                table: "Bookings");
        }
    }
}
