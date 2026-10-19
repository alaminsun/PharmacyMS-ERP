using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PharmacyMS.Infrastructure.Data;

#nullable disable

namespace PharmacyMS.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261018020000_AddMedicineMinimumStockLevel")]
public sealed class AddMedicineMinimumStockLevel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "MinimumStockLevel", table: "Medicines", type: "int",
            nullable: false, defaultValue: 10);
        migrationBuilder.AddCheckConstraint(
            name: "CK_Medicines_MinimumStockLevel", table: "Medicines",
            sql: "[MinimumStockLevel] >= 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Medicines_MinimumStockLevel", table: "Medicines");
        migrationBuilder.DropColumn(name: "MinimumStockLevel", table: "Medicines");
    }
}
