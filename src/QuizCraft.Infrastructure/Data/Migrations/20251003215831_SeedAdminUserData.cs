using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizCraft.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear roles
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Administrador')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'Administrador', 'ADMINISTRADOR', NEWID())
                END
                
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Profesor')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'Profesor', 'PROFESOR', NEWID())
                END
                
                IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Estudiante')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'Estudiante', 'ESTUDIANTE', NEWID())
                END
            ");

            // Crear usuario admin
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM')
                BEGIN
                    DECLARE @AdminUserId NVARCHAR(450) = NEWID()
                    
                    INSERT INTO AspNetUsers (
                        Id, 
                        Nombre, 
                        Apellido, 
                        NombreCompleto, 
                        FechaRegistro, 
                        EstaActivo, 
                        NotificacionesEmail, 
                        NotificacionesWeb, 
                        NotificacionesHabilitadas, 
                        PreferenciaIdioma,
                        UserName, 
                        NormalizedUserName, 
                        Email, 
                        NormalizedEmail, 
                        EmailConfirmed, 
                        PasswordHash, 
                        SecurityStamp, 
                        ConcurrencyStamp,
                        PhoneNumberConfirmed,
                        TwoFactorEnabled,
                        LockoutEnabled,
                        AccessFailedCount
                    )
                    VALUES (
                        @AdminUserId,
                        'Admin',
                        'QuizCraft',
                        'Admin QuizCraft',
                        GETDATE(),
                        1,
                        1,
                        1,
                        1,
                        'es',
                        'admin@quizcraft.com',
                        'ADMIN@QUIZCRAFT.COM',
                        'admin@quizcraft.com',
                        'ADMIN@QUIZCRAFT.COM',
                        1,
                        NULL,
                        NEWID(),
                        NEWID(),
                        0,
                        0,
                        1,
                        0
                    )
                    
                    -- Asignar rol de Administrador
                    INSERT INTO AspNetUserRoles (UserId, RoleId)
                    SELECT @AdminUserId, Id 
                    FROM AspNetRoles 
                    WHERE NormalizedName = 'ADMINISTRADOR'
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar asignación de roles del usuario admin
            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserRoles 
                WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM')
            ");

            // Eliminar usuario admin
            migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@QUIZCRAFT.COM'");

            // Eliminar roles
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE NormalizedName IN ('ADMINISTRADOR', 'PROFESOR', 'ESTUDIANTE')");
        }
    }
}
