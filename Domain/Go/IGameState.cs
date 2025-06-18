using Domain.Common;

namespace Domain.Go;

public interface IGameState
{
    Board Board { get; }
    Board PreviousBoard { get; }
    Player NextPlayer { get; }
    Move? LastMove { get; }
    Move? PreviousLastMove { get; }
    bool CanPass { get; }
    bool CanResign { get; }

    IGameState ApplyMove(Move move);
    Result<IGameState> Play(Move move);
    Result IsValidPoint(Point point);
    bool IsSuicide(Point point);
    bool DoesViolateKo(Point point);
    IEnumerable<Move> GetValidMoves();
    string PrintGameResult();
    string PrintLastMove();
    bool IsOver();
    Player GetWinner();
    string ToString();
}
