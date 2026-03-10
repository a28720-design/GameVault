using System.ComponentModel.DataAnnotations;

namespace GameVault.Models
{
    public class Genre
    {
        // ID único do género
        public int GenreId { get; set; }

        // Nome do género (RPG, Action, etc.)
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Relação muitos-para-muitos com Game
        // Um género pode estar em muitos jogos
        public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    }
}