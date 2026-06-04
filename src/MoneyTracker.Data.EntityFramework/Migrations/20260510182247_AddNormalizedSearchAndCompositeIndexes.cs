using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedSearchAndCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionNormalized",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "PaymentCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionNormalized",
                table: "Incomes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Date_PaymentCategoryId",
                table: "Payments",
                columns: new[] { "Date", "PaymentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DescriptionNormalized",
                table: "Payments",
                column: "DescriptionNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategories_NameNormalized",
                table: "PaymentCategories",
                column: "NameNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_Incomes_DescriptionNormalized",
                table: "Incomes",
                column: "DescriptionNormalized");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastOccurrences_ForecastOccurrenceStatusId_ExpectedDate_IsIncome",
                table: "ForecastOccurrences",
                columns: new[] { "ForecastOccurrenceStatusId", "ExpectedDate", "IsIncome" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_Date_PaymentCategoryId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_DescriptionNormalized",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentCategories_NameNormalized",
                table: "PaymentCategories");

            migrationBuilder.DropIndex(
                name: "IX_Incomes_DescriptionNormalized",
                table: "Incomes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastOccurrences_ForecastOccurrenceStatusId_ExpectedDate_IsIncome",
                table: "ForecastOccurrences");

            migrationBuilder.DropColumn(
                name: "DescriptionNormalized",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "PaymentCategories");

            migrationBuilder.DropColumn(
                name: "DescriptionNormalized",
                table: "Incomes");
        }
    }
}
