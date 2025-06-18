namespace Domain.Go;

public class AtariGoState : GameState
{
    private int _capturesToWin;

    public override bool CanPass => false;

    public override bool CanResign => false;

    public AtariGoState(Board board, Player nextPlayer, int capturesToWin = 1) : base(board, nextPlayer)
    {
        _capturesToWin = capturesToWin;
    }

    private AtariGoState(AtariGoState previousState, Move move) : base(previousState, move)
    {
        _capturesToWin = previousState._capturesToWin;
    }

    public override IGameState CreateGameState(IGameState previousState, Move move)
    {
        return new AtariGoState((AtariGoState)previousState, move);
    }

    // public override IEnumerable<Move> GetValidMoves()
    // {
    //     foreach (Point stone in Board.Stones)
    //     {
    //         Point[] neighbors = Board.GetNeighbors(stone);

    //         for (int i = 0; i < neighbors.Length; i++)
    //         {
    //             Point neighbor = neighbors[i];

    //             if (IsValidPoint(neighbor))
    //             {
    //                 yield return Move.Play(neighbor);
    //             }
    //         }

    //         Point[] corners = Board.GetCorners(stone);

    //         for (int i = 0; i < corners.Length; i++)
    //         {
    //             Point corner = corners[i];

    //             if (IsValidPoint(corner))
    //             {
    //                 yield return Move.Play(corner);
    //             }
    //         }
    //     }
    // }

    public override bool IsSuicide(Point point)
    {
        return false;
        // Board nextBoard = new(Board);
        // nextBoard.PlaceStone(NextPlayer, point);
        // return nextBoard.GetChain(point)?.Liberties.Count < 2;
    }

    public override bool DoesViolateKo(Point point)
    {
        return false;
    }

    public override bool IsOver()
    {
        if (LastMove is null)
        {
            return false;
        }

        if (LastMove.IsResign)
        {
            return true;
        }

        return IsWinner(NextPlayer) || IsWinner(NextPlayer.Other());
    }

    public override Player GetWinner()
    {
        if (LastMove != null && LastMove.IsResign)
        {
            return NextPlayer;
        }

        if (IsWinner(NextPlayer))
        {
            return NextPlayer;
        }

        if (IsWinner(NextPlayer.Other()))
        {
            return NextPlayer.Other();
        }

        return Player.None;
    }

    public override string PrintGameResult()
    {
        return $"{Player.Black}: {Board.GetCapturedStones(Player.Black)}, {Player.White}: {Board.GetCapturedStones(Player.White)}";
    }

    private bool IsWinner(Player player)
    {
        return Board.GetCapturedStones(player) >= _capturesToWin;
    }
}
