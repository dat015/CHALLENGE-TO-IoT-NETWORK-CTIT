using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlockChainForIoT.Migrations
{
    /// <inheritdoc />
    public partial class updatesensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sensorCode",
                table: "Sensors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sensorCode",
                table: "Sensors");
        }
    }
}
