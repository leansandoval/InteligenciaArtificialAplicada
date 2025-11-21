using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizCraft.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardCompartidaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlashcardsCompartidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlashcardId = table.Column<int>(type: "int", nullable: false),
                    PropietarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CodigoCompartido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VecesUsado = table.Column<int>(type: "int", nullable: false),
                    MaximoUsos = table.Column<int>(type: "int", nullable: true),
                    PermiteModificaciones = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardsCompartidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardsCompartidas_AspNetUsers_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlashcardsCompartidas_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashcardsImportadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlashcardCompartidaId = table.Column<int>(type: "int", nullable: false),
                    FlashcardId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaImportacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardsImportadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashcardsImportadas_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlashcardsImportadas_FlashcardsCompartidas_FlashcardCompartidaId",
                        column: x => x.FlashcardCompartidaId,
                        principalTable: "FlashcardsCompartidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlashcardsImportadas_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsCompartidas_Codigo",
                table: "FlashcardsCompartidas",
                column: "CodigoCompartido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsCompartidas_FlashcardId",
                table: "FlashcardsCompartidas",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsCompartidas_PropietarioId",
                table: "FlashcardsCompartidas",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsImportadas_Compartida_Usuario",
                table: "FlashcardsImportadas",
                columns: new[] { "FlashcardCompartidaId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsImportadas_FlashcardId",
                table: "FlashcardsImportadas",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardsImportadas_UsuarioId",
                table: "FlashcardsImportadas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashcardsImportadas");

            migrationBuilder.DropTable(
                name: "FlashcardsCompartidas");
        }
    }
}
