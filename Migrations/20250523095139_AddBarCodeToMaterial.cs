using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequisitionSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBarCodeToMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarCode",
                table: "Materials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionRemarks_AuthorId",
                table: "RequisitionRemarks",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequisitionRemarks_Users_AuthorId",
                table: "RequisitionRemarks",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequisitionRemarks_Users_AuthorId",
                table: "RequisitionRemarks");

            migrationBuilder.DropIndex(
                name: "IX_RequisitionRemarks_AuthorId",
                table: "RequisitionRemarks");

            migrationBuilder.DropColumn(
                name: "BarCode",
                table: "Materials");
        }
    }
}
