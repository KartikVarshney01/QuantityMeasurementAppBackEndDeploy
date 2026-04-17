using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantityMeasurementRepoLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuantityMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Operation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Operand1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Operand2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasError = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuantityMeasurements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuantityMeasurements_CreatedAt",
                table: "QuantityMeasurements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityMeasurements_HasError",
                table: "QuantityMeasurements",
                column: "HasError");

            migrationBuilder.CreateIndex(
                name: "IX_QuantityMeasurements_Operation",
                table: "QuantityMeasurements",
                column: "Operation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuantityMeasurements");
        }
    }
}
