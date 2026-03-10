namespace GameVault.Models
{
    public class GameGenre
    {
        // FK → jogo
        public int GameId { get; set; }

        // Objeto jogo associado
        public Game? Game { get; set; }

        // FK → género
        public int GenreId { get; set; }

        // Objeto género associado
        public Genre? Genre { get; set; }
    }
}