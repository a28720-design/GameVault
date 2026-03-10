using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GameVault.Models;

namespace GameVault.Data
{
    // DbContext = classe que representa a ligação à base de dados
    public class ApplicationDbContext : IdentityDbContext
    {
        // Construtor que recebe configurações da BD
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tabela Games na BD
        public DbSet<Game> Games => Set<Game>();

        // Tabela Platforms na BD
        public DbSet<Platform> Platforms => Set<Platform>();

        // Tabela Genres na BD
        public DbSet<Genre> Genres => Set<Genre>();

        // Tabela intermédia GameGenres
        public DbSet<GameGenre> GameGenres => Set<GameGenre>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Impede plataformas com nomes repetidos
            builder.Entity<Platform>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Impede géneros com nomes repetidos
            builder.Entity<Genre>()
                .HasIndex(g => g.Name)
                .IsUnique();

            // Define chave primária composta (GameId + GenreId)
            builder.Entity<GameGenre>()
                .HasKey(gg => new { gg.GameId, gg.GenreId });

            // Relação GameGenre → Game
            builder.Entity<GameGenre>()
                .HasOne(gg => gg.Game)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GameId);

            // Relação GameGenre → Genre
            builder.Entity<GameGenre>()
                .HasOne(gg => gg.Genre)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GenreId);

            // Valor automático para data de criação
            builder.Entity<Game>()
                .Property(g => g.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}