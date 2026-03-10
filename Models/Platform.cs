using System.ComponentModel.DataAnnotations;

namespace GameVault.Models
{
    public class Platform
    {
        // ID único da plataforma (chave primária na BD)
        public int PlatformId { get; set; }

        // Nome da plataforma (PC, PS5, Switch, etc.)
        // Required = obrigatório
        // MaxLength = limite de caracteres na BD
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Relação: uma plataforma tem muitos jogos
        // ICollection = lista de jogos associados
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}