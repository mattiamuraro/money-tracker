using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTracker.Data.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuditColumnsToIdAndAddUserNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PaymentCategories");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PaymentCategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ForecastExpenses");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ForecastExpenses");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "PaymentCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "PaymentCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ForecastRecurrenceRuleTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "ForecastRecurrenceRuleTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ForecastIncomes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "ForecastIncomes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "ForecastExpenses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedById",
                table: "ForecastExpenses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedById",
                table: "Users",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ModifiedById",
                table: "Users",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedById",
                table: "Payments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ModifiedById",
                table: "Payments",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategories_CreatedById",
                table: "PaymentCategories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategories_ModifiedById",
                table: "PaymentCategories",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRecurrenceRuleTypes_CreatedById",
                table: "ForecastRecurrenceRuleTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRecurrenceRuleTypes_ModifiedById",
                table: "ForecastRecurrenceRuleTypes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastIncomes_CreatedById",
                table: "ForecastIncomes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastIncomes_ModifiedById",
                table: "ForecastIncomes",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastExpenses_CreatedById",
                table: "ForecastExpenses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastExpenses_ModifiedById",
                table: "ForecastExpenses",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastExpenses_Users_CreatedById",
                table: "ForecastExpenses",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastExpenses_Users_ModifiedById",
                table: "ForecastExpenses",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastIncomes_Users_CreatedById",
                table: "ForecastIncomes",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastIncomes_Users_ModifiedById",
                table: "ForecastIncomes",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastRecurrenceRuleTypes_Users_CreatedById",
                table: "ForecastRecurrenceRuleTypes",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForecastRecurrenceRuleTypes_Users_ModifiedById",
                table: "ForecastRecurrenceRuleTypes",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentCategories_Users_CreatedById",
                table: "PaymentCategories",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentCategories_Users_ModifiedById",
                table: "PaymentCategories",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_CreatedById",
                table: "Payments",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_ModifiedById",
                table: "Payments",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_CreatedById",
                table: "Users",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ModifiedById",
                table: "Users",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_Users_CreatedById",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastExpenses_Users_ModifiedById",
                table: "ForecastExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastIncomes_Users_CreatedById",
                table: "ForecastIncomes");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastIncomes_Users_ModifiedById",
                table: "ForecastIncomes");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastRecurrenceRuleTypes_Users_CreatedById",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ForecastRecurrenceRuleTypes_Users_ModifiedById",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentCategories_Users_CreatedById",
                table: "PaymentCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentCategories_Users_ModifiedById",
                table: "PaymentCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_CreatedById",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_ModifiedById",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_CreatedById",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ModifiedById",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedById",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ModifiedById",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedById",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ModifiedById",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentCategories_CreatedById",
                table: "PaymentCategories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentCategories_ModifiedById",
                table: "PaymentCategories");

            migrationBuilder.DropIndex(
                name: "IX_ForecastRecurrenceRuleTypes_CreatedById",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastRecurrenceRuleTypes_ModifiedById",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastIncomes_CreatedById",
                table: "ForecastIncomes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastIncomes_ModifiedById",
                table: "ForecastIncomes");

            migrationBuilder.DropIndex(
                name: "IX_ForecastExpenses_CreatedById",
                table: "ForecastExpenses");

            migrationBuilder.DropIndex(
                name: "IX_ForecastExpenses_ModifiedById",
                table: "ForecastExpenses");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "PaymentCategories");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "PaymentCategories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "ForecastRecurrenceRuleTypes");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "ForecastIncomes");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "ForecastExpenses");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "ForecastExpenses");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PaymentCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "PaymentCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ForecastRecurrenceRuleTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "ForecastRecurrenceRuleTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ForecastIncomes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "ForecastIncomes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ForecastExpenses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "ForecastExpenses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
