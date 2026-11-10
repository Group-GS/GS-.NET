using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertaCidadao.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PONTO_MONITORAMENTO",
                columns: table => new
                {
                    ID_PONTO = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODIGO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    LOGRADOURO = table.Column<string>(type: "NVARCHAR2(180)", maxLength: 180, nullable: true),
                    BAIRRO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    CIDADE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    UF = table.Column<string>(type: "CHAR(2)", nullable: true),
                    LATITUDE = table.Column<decimal>(type: "NUMBER(9,6)", nullable: true),
                    LONGITUDE = table.Column<decimal>(type: "NUMBER(9,6)", nullable: true),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    DATA_CADASTRO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PONTO_MONITORAMENTO", x => x.ID_PONTO);
                });

            migrationBuilder.CreateTable(
                name: "REQUISICAO_IDEMPOTENCIA",
                columns: table => new
                {
                    CHAVE_IDEMPOTENCIA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    METODO_HTTP = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    ROTA = table.Column<string>(type: "NVARCHAR2(250)", maxLength: 250, nullable: false),
                    STATUS_HTTP = table.Column<byte>(type: "NUMBER(3)", nullable: true),
                    RESPOSTA_JSON = table.Column<string>(type: "CLOB", nullable: true),
                    DATA_CRIACAO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    DATA_EXPIRACAO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REQUISICAO_IDEMPOTENCIA", x => x.CHAVE_IDEMPOTENCIA);
                });

            migrationBuilder.CreateTable(
                name: "USUARIO",
                columns: table => new
                {
                    ID_USUARIO = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(180)", maxLength: 180, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    PERFIL = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "CIDADAO"),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    DATA_CADASTRO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIO", x => x.ID_USUARIO);
                });

            migrationBuilder.CreateTable(
                name: "LEITURA_ENCHENTE",
                columns: table => new
                {
                    ID_LEITURA = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_PONTO = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    NIVEL_AGUA_CM = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false),
                    CHUVA_MM = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false, defaultValue: 0m),
                    ELEVACAO_CM_H = table.Column<decimal>(type: "NUMBER(10,2)", nullable: false, defaultValue: 0m),
                    DATA_HORA_LEITURA = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false),
                    ORIGEM = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "SIMULACAO"),
                    CHAVE_EVENTO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DATA_RECEBIMENTO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LEITURA_ENCHENTE", x => x.ID_LEITURA);
                    table.ForeignKey(
                        name: "FK_LEITURA_PONTO",
                        column: x => x.ID_PONTO,
                        principalTable: "PONTO_MONITORAMENTO",
                        principalColumn: "ID_PONTO",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALERTA",
                columns: table => new
                {
                    ID_ALERTA = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_LEITURA = table.Column<decimal>(type: "NUMBER(20,0)", nullable: false),
                    CLASSIFICACAO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    PROBABILIDADE = table.Column<decimal>(type: "NUMBER(5,4)", nullable: true),
                    MENSAGEM_EXPLICATIVA = table.Column<string>(type: "CLOB", nullable: true),
                    TIPO_MENSAGEM = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    MODELO_ML = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "ATIVO"),
                    DATA_GERACAO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false, defaultValueSql: "SYSTIMESTAMP"),
                    DATA_RESOLUCAO = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALERTA", x => x.ID_ALERTA);
                    table.ForeignKey(
                        name: "FK_ALERTA_LEITURA",
                        column: x => x.ID_LEITURA,
                        principalTable: "LEITURA_ENCHENTE",
                        principalColumn: "ID_LEITURA",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ALERTA_LEITURA",
                table: "ALERTA",
                column: "ID_LEITURA",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_LEITURA_EVENTO",
                table: "LEITURA_ENCHENTE",
                column: "CHAVE_EVENTO",
                unique: true,
                filter: "\"CHAVE_EVENTO\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_LEITURA_PONTO_DATA",
                table: "LEITURA_ENCHENTE",
                columns: new[] { "ID_PONTO", "DATA_HORA_LEITURA" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PONTO_CODIGO",
                table: "PONTO_MONITORAMENTO",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IDEMPOTENCIA_EXPIRA",
                table: "REQUISICAO_IDEMPOTENCIA",
                column: "DATA_EXPIRACAO");

            migrationBuilder.CreateIndex(
                name: "UQ_USUARIO_EMAIL",
                table: "USUARIO",
                column: "EMAIL",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ALERTA");

            migrationBuilder.DropTable(
                name: "REQUISICAO_IDEMPOTENCIA");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "LEITURA_ENCHENTE");

            migrationBuilder.DropTable(
                name: "PONTO_MONITORAMENTO");
        }
    }
}
