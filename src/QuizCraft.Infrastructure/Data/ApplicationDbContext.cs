using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizCraft.Core.Entities;

namespace QuizCraft.Infrastructure.Data
{
    /// <summary>
    /// Contexto principal de la base de datos que integra ASP.NET Identity con las entidades del dominio
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las entidades del dominio
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<PreguntaQuiz> PreguntasQuiz { get; set; }
        public DbSet<ResultadoQuiz> ResultadosQuiz { get; set; }
        public DbSet<RespuestaUsuario> RespuestasUsuario { get; set; }
        public DbSet<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
        public DbSet<EstadisticaEstudio> EstadisticasEstudio { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.NombreCompleto)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UltimoAcceso)
                    .HasColumnType("datetime2");

                entity.Property(e => e.PreferenciaIdioma)
                    .HasMaxLength(10)
                    .HasDefaultValue("es");

                entity.Property(e => e.NotificacionesHabilitadas)
                    .HasDefaultValue(true);
            });

            // Configuración de Materia
            builder.Entity<Materia>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500);

                entity.Property(e => e.Color)
                    .HasMaxLength(7);

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.Materias)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Flashcard
            builder.Entity<Flashcard>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Pregunta)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(e => e.Respuesta)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(e => e.Dificultad)
                    .HasConversion<string>()
                    .HasMaxLength(10);

                entity.HasOne(e => e.Materia)
                    .WithMany(m => m.Flashcards)
                    .HasForeignKey(e => e.MateriaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Quiz
            builder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Titulo)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(1000);

                entity.Property(e => e.TiempoPorPregunta)
                    .HasDefaultValue(30);

                entity.HasOne(e => e.Materia)
                    .WithMany(m => m.Quizzes)
                    .HasForeignKey(e => e.MateriaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Creador)
                    .WithMany(u => u.QuizzesCreados)
                    .HasForeignKey(e => e.CreadorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de PreguntaQuiz
            builder.Entity<PreguntaQuiz>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TextoPregunta)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(e => e.OpcionA)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.OpcionB)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.OpcionC)
                    .HasMaxLength(500);

                entity.Property(e => e.OpcionD)
                    .HasMaxLength(500);

                entity.Property(e => e.RespuestaCorrecta)
                    .HasMaxLength(1)
                    .IsRequired();

                entity.Property(e => e.Explicacion)
                    .HasMaxLength(1000);

                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.Preguntas)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de ResultadoQuiz
            builder.Entity<ResultadoQuiz>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Puntuacion)
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.TiempoTotal)
                    .IsRequired();

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.ResultadosQuiz)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.Resultados)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de RespuestaUsuario
            builder.Entity<RespuestaUsuario>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.RespuestaSeleccionada)
                    .HasMaxLength(1)
                    .IsRequired();

                entity.HasOne(e => e.ResultadoQuiz)
                    .WithMany(r => r.RespuestasUsuario)
                    .HasForeignKey(e => e.ResultadoQuizId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Pregunta)
                    .WithMany()
                    .HasForeignKey(e => e.PreguntaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de ArchivoAdjunto
            builder.Entity<ArchivoAdjunto>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NombreArchivo)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.RutaArchivo)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.TipoMime)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.TipoEntidad)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            // Configuración de EstadisticaEstudio
            builder.Entity<EstadisticaEstudio>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TipoActividad)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.TiempoEstudio)
                    .IsRequired();

                entity.HasOne(e => e.Usuario)
                    .WithMany(u => u.EstadisticasEstudio)
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Materia)
                    .WithMany(m => m.EstadisticasEstudio)
                    .HasForeignKey(e => e.MateriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de índices para mejorar rendimiento
            builder.Entity<Flashcard>()
                .HasIndex(f => f.MateriaId)
                .HasDatabaseName("IX_Flashcards_MateriaId");

            builder.Entity<Quiz>()
                .HasIndex(q => q.MateriaId)
                .HasDatabaseName("IX_Quizzes_MateriaId");

            builder.Entity<Quiz>()
                .HasIndex(q => q.CreadorId)
                .HasDatabaseName("IX_Quizzes_CreadorId");

            builder.Entity<ResultadoQuiz>()
                .HasIndex(r => new { r.UsuarioId, r.FechaRealizacion })
                .HasDatabaseName("IX_ResultadosQuiz_Usuario_Fecha");

            builder.Entity<EstadisticaEstudio>()
                .HasIndex(e => new { e.UsuarioId, e.Fecha })
                .HasDatabaseName("IX_EstadisticasEstudio_Usuario_Fecha");
        }
    }
}