using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDSG.Migrations
{
    /// <inheritdoc />
    public partial class AddDataRegistroToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataRegistro",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfil",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaAtualizacao",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServicosProfissionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Preco = table.Column<decimal>(type: "TEXT", nullable: false),
                    TempoEstimado = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ProfissionalId = table.Column<string>(type: "TEXT", nullable: false),
                    IsAtivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicosProfissionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicosProfissionais_AspNetUsers_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ImagemUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LinkProjeto = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataProjeto = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAtivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProfissionalId = table.Column<string>(type: "TEXT", nullable: false),
                    ServicoProfissionalId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioItems_AspNetUsers_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PortfolioItems_ServicosProfissionais_ServicoProfissionalId",
                        column: x => x.ServicoProfissionalId,
                        principalTable: "ServicosProfissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_ProfissionalId",
                table: "PortfolioItems",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_ServicoProfissionalId",
                table: "PortfolioItems",
                column: "ServicoProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosProfissionais_ProfissionalId",
                table: "ServicosProfissionais",
                column: "ProfissionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioItems");

            migrationBuilder.DropTable(
                name: "ServicosProfissionais");

            migrationBuilder.DropColumn(
                name: "DataRegistro",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UltimaAtualizacao",
                table: "AspNetUsers");
        }
    }
}
