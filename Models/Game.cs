using System.ComponentModel.DataAnnotations;

namespace GameVault.Models
{
    public class Game
    {
        // ID único do jogo (PK)
        public int GameId { get; set; }

        // Nome do jogo
        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        // Descrição do jogo (opcional)
        [MaxLength(1000)]
        public string? Description { get; set; }

        // Data de lançamento do jogo (opcional)
        public DateTime? ReleaseDate { get; set; }

        // Nota do jogo (0 a 10)
        [Range(0, 10)]
        public decimal? Rating { get; set; }

        // Estado do jogo (Backlog, Playing, etc.)
        public GameStatus Status { get; set; } = GameStatus.Backlog;

        // URL da capa do jogo (imagem)
        [MaxLength(300)]
        public string? CoverUrl { get; set; }

        // FK → Platform (liga jogo à plataforma)
        [Required]
        public int PlatformId { get; set; }

        // Objeto da plataforma (relação)
        public Platform? Platform { get; set; }

        // ID do utilizador dono do jogo (se tiver login)
        public string? OwnerUserId { get; set; }

        // Data de criação do registo
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Data da última atualização
        public DateTime? UpdatedAt { get; set; }

        // Relação muitos-para-muitos com géneros
        public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    }
}