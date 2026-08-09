using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieSeatBooking.Migrations
{
    /// <inheritdoc />
    public partial class FixPostgresConcurrency : Migration
    {
        /// <inheritdoc />
       protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "RowVersion",
        table: "Seats");
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<byte[]>(
        name: "RowVersion",
        table: "Seats",
        type: "bytea",
        rowVersion: true,
        nullable: true);
}
    }
}
