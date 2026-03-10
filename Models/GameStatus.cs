namespace GameVault.Models
{
    // Enum = lista fixa de valores possíveis para o estado do jogo
    public enum GameStatus
    {
        Backlog = 0,   // jogo ainda não jogado
        Playing = 1,   // jogo que o utilizador está a jogar
        Completed = 2, // jogo terminado
        Dropped = 3    // jogo não jogado
    }
}