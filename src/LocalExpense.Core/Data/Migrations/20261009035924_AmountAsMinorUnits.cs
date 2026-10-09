using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalExpense.Data.Migrations
{
    /// <inheritdoc />
    public partial class AmountAsMinorUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transactions");

            migrationBuilder.AddColumn<long>(
                name: "AmountMinor",
                table: "Transactions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountMinor",
                table: "Transactions");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
