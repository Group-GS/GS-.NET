using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertaCidadao.Infrastructure.Persistence.Migrations;

public partial class AddIdempotencyFingerprint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FINGERPRINT_HASH",
            table: "REQUISICAO_IDEMPOTENCIA",
            type: "NVARCHAR2(64)",
            maxLength: 64,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FINGERPRINT_HASH",
            table: "REQUISICAO_IDEMPOTENCIA");
    }
}
