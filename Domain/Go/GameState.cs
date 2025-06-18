using System.Text;
using Domain.Common;
using Domain.Extensions;

namespace Domain.Go;

public abstract class GameState : IGameState
{
    public Board Board { get; }
    public Board PreviousBoard { get; }
    public Player NextPlayer { get; }
    public Move? LastMove { get; }
    public Move? PreviousLastMove { get; }
    public abstract bool CanPass { get; }
    public abstract bool CanResign { get; }

    protected GameState(Board board, Player nextPlayer)
    {
        Board = board;
        PreviousBoard = board;
        NextPlayer = nextPlayer;
    }

    protected GameState(IGameState previousState, Move move)
    {
        Board = CreateUpdatedBoard(previousState, move);
        PreviousBoard = previousState.Board;
        NextPlayer = previousState.NextPlayer.Other();
        PreviousLastMove = previousState.LastMove;
        LastMove = move;
    }

    public abstract IGameState CreateGameState(IGameState previousState, Move move);
    public abstract bool IsSuicide(Point point);
    public abstract bool DoesViolateKo(Point point);
    public abstract Player GetWinner();
    public abstract bool IsOver();
    public abstract string PrintGameResult();

    public IGameState ApplyMove(Move move)
    {
        return CreateGameState(this, move);
    }

    public Result<IGameState> Play(Move move)
    {
        return IsValidMove(move)
            .OnSuccess(() => ApplyMove(move));
    }

    public Result IsValidMove(Move move)
    {
        if (move.IsPass && !CanPass)
        {
            return Result.Failure(ErrorMessages.PassIsNotAllowed);
        }

        if (move.IsResign && !CanResign)
        {
            return Result.Failure(ErrorMessages.ResignIsNotAllowed);
        }

        if (move.IsPlay)
        {
            if (Board.IsOutsideGrid(move.Point))
            {
                return Result.Failure(ErrorMessages.PointIsOutsideGrid);
            }

            if (Board.IsPointTaken(move.Point))
            {
                return Result.Failure(ErrorMessages.PointIsAlreadyTaken);
            }

            // if (Board.IsPointAnEye(NextPlayer, point))
            // {
            //     return Result.Failure("");
            // }

            if (IsSuicide(move.Point))
            {
                return Result.Failure(ErrorMessages.PointIsSuicide);
            }

            if (DoesViolateKo(move.Point))
            {
                return Result.Failure(ErrorMessages.PointViolatesKo);
            }
        }

        return Result.Success();
    }

    public Result IsValidPoint(Point point)
    {
        if (Board.IsOutsideGrid(point))
        {
            return Result.Failure(ErrorMessages.PointIsOutsideGrid);
        }

        if (Board.IsPointTaken(point))
        {
            return Result.Failure(ErrorMessages.PointIsAlreadyTaken);
        }

        // if (Board.IsPointAnEye(NextPlayer, point))
        // {
        //     return Result.Failure("");
        // }

        if (IsSuicide(point))
        {
            return Result.Failure(ErrorMessages.PointIsSuicide);
        }

        if (DoesViolateKo(point))
        {
            return Result.Failure(ErrorMessages.PointViolatesKo);
        }

        return Result.Success();
    }

    public virtual IEnumerable<Move> GetValidMoves()
    {
        if (IsOver())
        {
            return [];
        }

        return GetValidMoves(Board.GetIntersections());
    }

    public string PrintLastMove()
    {
        Player lastPlayer;
        string moveDescription;

        if (LastMove == null)
        {
            lastPlayer = NextPlayer;
            moveDescription = "to move";
        }
        else
        {
            lastPlayer = NextPlayer.Other();
            moveDescription = LastMove.ToA1Coordinates(Board.Size);
        }

        return $"{lastPlayer} {moveDescription}";
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();

        stringBuilder.Append(PrintLastMove());

        stringBuilder.AppendLine();

        stringBuilder.Append(Board.PrintBoard());

        return stringBuilder.ToString();
    }

    private static Board CreateUpdatedBoard(IGameState previousState, Move move)
    {
        Board board = previousState.Board;
        Player player = previousState.NextPlayer;

        if (move.IsPlay)
        {
            board = new(board);
            board.PlaceStone(player, move.Point);
        }

        return board;
    }

    private IEnumerable<Move> GetValidMoves(Point[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Point point = points[i];

            if (IsValidPoint(point).IsSuccess)
            {
                yield return Move.Play(point);
            }
        }
    }

    private bool IsNotAnEye(Point point)
    {
        return !Board.IsPointAnEye(NextPlayer, point);
    }
}
