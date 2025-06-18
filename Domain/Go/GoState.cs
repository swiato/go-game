using Domain.Scoring;

namespace Domain.Go;

public class GoState : GameState
{
    public override bool CanPass => true;

    public override bool CanResign => true;

    public GoState(Board board, Player nextPlayer) : base(board, nextPlayer)
    {
    }

    private GoState(IGameState previousState, Move move) : base(previousState, move)
    {
    }

    public override IGameState CreateGameState(IGameState previousState, Move move)
    {
        return new GoState(previousState, move);
    }

    public override bool IsSuicide(Point point)
    {
        Board nextBoard = new(Board);
        nextBoard.PlaceStone(NextPlayer, point);
        return nextBoard.GetChain(point) is null;
    }

    public override bool DoesViolateKo(Point point)
    {
        if (LastMove is null || LastMove.IsPassOrResign)
        {
            return false;
        }

        Board nextBoard = new(Board);
        nextBoard.PlaceStone(NextPlayer, point);

        return PreviousBoard == nextBoard;
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

        if (PreviousLastMove is null)
        {
            return false;
        }

        return LastMove.IsPass && PreviousLastMove.IsPass;
    }

    public override Player GetWinner()
    {
        if (LastMove is not null && LastMove.IsResign)
        {
            return NextPlayer;
        }

        if (IsOver())
        {
            GameResult gameResult = Territory.ComputeGameResult(this);
            return gameResult.Winner;
        }

        return Player.None;
    }

    public override string PrintGameResult()
    {
        GameResult gameResult = Territory.ComputeGameResult(this);
        return gameResult.ToString();
        
        // return $"Winner: {gameResult.Winner} ({gameResult}) => {nameof(gameResult.Black)}: {gameResult.Black}, {nameof(gameResult.White)}: {gameResult.White} + {gameResult.Komi} ({nameof(gameResult.Komi)}) = {gameResult.White + gameResult.Komi}";
    }
}
