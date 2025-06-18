namespace Domain.Go;

public class GoStateFactory : IGameStateFactory
{
    public IGameState NewGame(int boardSize)
    {
        Board board = new(boardSize);
        return new GoState(board, Player.Black);
    }
}
