namespace Domain.Common;

public static class ErrorMessages
{
    public const string InvalidMove = "Invalid move"; //"Move can be either point on a board, pass or resign but not all of it";
    public const string InvalidCoordinates = "Coordinates must include at least 2 characters";
    public const string InvalidBoardSizeForFixedHandicap = "Invalid board size for fixed handicap placement";
    public const string InvalidHandicapValueForFixedHandicap = "Invalid handicap value for fixed handicap placement";
    public const string BoardNotSquare = "Board Width and Height must be same size";
    public const string PointIsOutsideGrid = "Point is outside of the grid";
    public const string PointIsAlreadyTaken = "This place is already taken by other stone";
    public const string PointIsEmpty = "Point is empty";
    public const string CantMergeOppositePlayerChains = "Can't merge chains of different color";
    public const string PointIsSuicide = "Point is a suicide move";
    public const string ModelEncoderConflict = "Model output doesn't match encoder points count";
    public const string ObjectDisposed = "Object disposed";
    public const string NodeFullyExpanded = "Node is already fully expanded";
    public const string PointViolatesKo = "Point violates KO rule";
    public const string PassIsNotAllowed = "Pass is not allowed";
    public const string ResignIsNotAllowed = "Resign is not allowed";
}
