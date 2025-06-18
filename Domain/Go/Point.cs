using Domain.Common;

namespace Domain.Go;

public record Point(int Row, int Column)
{
    public static readonly Point Empty = new(-1, -1);

    public static Point FromA1Coordinates(string coordinates, int boardSize)
    {
        ValidateCoordinates(coordinates);

        var column = BoardHelper.ColumnNames.IndexOf(char.ToUpper(coordinates[0]));
        var row = boardSize - int.Parse(coordinates[1..]);

        return new Point(row, column);
    }

    public static Point FromSgfCoordinates(string coordinates)
    {
        ValidateCoordinates(coordinates);

        int column = SgfAlphaToNumeric(coordinates[0]);
        int row = SgfAlphaToNumeric(coordinates[1]);

        return new Point(row, column);
    }

    public string ToA1Coordinates(int boardSize)
    {
        return $"{BoardHelper.ColumnNames[Column]}{boardSize - Row}";
    }

    public override string ToString()
    {
        return $"Row: {Row}, Column: {Column}";
    }

    private static void ValidateCoordinates(string coordinates)
    {
        if (coordinates.Length < 2)
        {
            throw new ArgumentException(ErrorMessages.InvalidCoordinates);
        }
    }

    private static int SgfAlphaToNumeric(char coordinate)
    {
        return char.ToLower(coordinate) - 'a';
    }
}