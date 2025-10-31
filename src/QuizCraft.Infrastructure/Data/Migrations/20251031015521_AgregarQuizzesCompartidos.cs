using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizCraft.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarQuizzesCompartidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizzesCompartidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_QuizzesCompartidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizzesCompartidos_AspNetUsers_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizzesCompartidos_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizzesImportados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizCompartidoId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizzesImportados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizzesImportados_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizzesImportados_QuizzesCompartidos_QuizCompartidoId",
                        column: x => x.QuizCompartidoId,
                        principalTable: "QuizzesCompartidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizzesImportados_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizzesCompartidos_Codigo",
                table: "QuizzesCompartidos",
                column: "CodigoCompartido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuizzesCompartidos_PropietarioId",
                table: "QuizzesCompartidos",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizzesCompartidos_QuizId",
                table: "QuizzesCompartidos",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizzesImportados_Compartido_Usuario",
                table: "QuizzesImportados",
                columns: new[] { "QuizCompartidoId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuizzesImportados_QuizId",
                table: "QuizzesImportados",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizzesImportados_UsuarioId",
                table: "QuizzesImportados",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizzesImportados");

            migrationBuilder.DropTable(
                name: "QuizzesCompartidos");
        }
    }
}
