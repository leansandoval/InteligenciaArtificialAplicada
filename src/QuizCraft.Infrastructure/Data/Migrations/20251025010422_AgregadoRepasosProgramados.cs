using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizCraft.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoRepasosProgramados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepasosProgramados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Completado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoRepaso = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FrecuenciaRepeticion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProximaFecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificacionesHabilitadas = table.Column<bool>(type: "bit", nullable: false),
                    MinutosNotificacionPrevia = table.Column<int>(type: "int", nullable: false),
                    UltimoPuntaje = table.Column<double>(type: "float", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MateriaId = table.Column<int>(type: "int", nullable: true),
                    QuizId = table.Column<int>(type: "int", nullable: true),
                    FlashcardId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepasosProgramados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepasosProgramados_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepasosProgramados_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepasosProgramados_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepasosProgramados_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepasosProgramados_FlashcardId",
                table: "RepasosProgramados",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_RepasosProgramados_MateriaId",
                table: "RepasosProgramados",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_RepasosProgramados_QuizId",
                table: "RepasosProgramados",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_RepasosProgramados_Usuario_Estado",
                table: "RepasosProgramados",
                columns: new[] { "UsuarioId", "Completado", "EstaActivo" });

            migrationBuilder.CreateIndex(
                name: "IX_RepasosProgramados_Usuario_Fecha",
                table: "RepasosProgramados",
                columns: new[] { "UsuarioId", "FechaProgramada" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepasosProgramados");
        }
    }
}
