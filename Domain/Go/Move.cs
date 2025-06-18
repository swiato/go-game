using Domain.Common;

namespace Domain.Go;

public class MovePrediction(Point point, float probability, bool isPass = false) : Move(point, isPass)
{
    public float Probability { get; } = probability;

    public new static MovePrediction Pass()
    {
        return new MovePrediction(Point.Empty, 0f, true);
    }
}

// public record MoveProbability(Move Move, float Probability);

public class Move
{
    public Point Point { get; }
    public bool IsPlay { get; }
    public bool IsPass { get; }
    public bool IsResign { get; }

    protected Move(Point? point = null, bool isPass = false, bool isResign = false)
    {
        IsPlay = point != null;
        IsPass = isPass;
        IsResign = isResign;
        Point = point ?? Point.Empty;

        if (!(IsPlay ^ IsPass ^ IsResign))
        {
            throw new ArgumentException(ErrorMessages.InvalidMove);
        }
    }

    public bool IsPassOrResign => !IsPlay;

    public static Move Play(Point point)
    {
        return new Move(point);
    }

    public static Move Pass()
    {
        return new Move(isPass: true);
    }

    public static Move Resign()
    {
        return new Move(isResign: true);
    }

    public static Move FromA1Coordinates(string input, int boardSize)
    {
        input = input.Trim().ToUpper();

        if (input.Equals(GoTerms.Pass, StringComparison.InvariantCultureIgnoreCase))
        {
            return Pass();
        }

        if (input.Equals(GoTerms.Resign, StringComparison.InvariantCultureIgnoreCase))
        {
            return Resign();
        }

        return Play(Point.FromA1Coordinates(input, boardSize));
    }

    public string ToA1Coordinates(int boardSize)
    {
        if (IsPlay)
        {
            return Point.ToA1Coordinates(boardSize);
        }

        if (IsPass)
        {
            return GoTerms.Pass;
        }

        return GoTerms.Resign;
    }
}
