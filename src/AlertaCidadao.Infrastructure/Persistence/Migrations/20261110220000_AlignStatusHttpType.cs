using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertaCidadao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignStatusHttpType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No database DDL is required. The Oracle column remains NUMBER(3).
            // This migration synchronizes the EF model metadata from byte? to int?.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No database DDL is required to revert this metadata-only change.
        }
    }
}
