namespace Domain.Go;

public class AtariGoStateFactory : IGameStateFactory
{
    public IGameState NewGame(int boardSize)
    {
        Board board = new(boardSize);
        return new AtariGoState(board, Player.Black);
    }
}
