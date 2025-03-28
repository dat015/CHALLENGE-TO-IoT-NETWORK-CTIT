using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web3IoT.Migrations
{
    /// <inheritdoc />
    public partial class AddQRCodeToCrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "Crops",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "Crops");
        }
    }
}
