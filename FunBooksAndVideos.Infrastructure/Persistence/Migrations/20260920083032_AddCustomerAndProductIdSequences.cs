using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunBooksAndVideos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAndProductIdSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "CustomerIds");

            migrationBuilder.CreateSequence(
                name: "ProductIds",
                startValue: 100L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "CustomerIds");

            migrationBuilder.DropSequence(
                name: "ProductIds");
        }
    }
}
