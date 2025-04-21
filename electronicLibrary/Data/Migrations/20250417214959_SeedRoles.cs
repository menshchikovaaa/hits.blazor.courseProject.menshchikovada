using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace electronicLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
        table: "AspNetRoles",
        columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
        values: new object[,]
        {
            { Guid.NewGuid().ToString(), "Admin", "ADMIN", Guid.NewGuid().ToString() },
            { Guid.NewGuid().ToString(), "Librarian", "LIBRARIAN", Guid.NewGuid().ToString() },
            { Guid.NewGuid().ToString(), "User", "USER", Guid.NewGuid().ToString() }
        });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
