using Domain.Go;

namespace Domain.UnitTests.Go;

public class GoStateTests
{
    private const int _boardSize = 5;
    private IGameState _gameState;

    [SetUp]
    public void Setup()
    {
        _gameState = new GoState(new Board(_boardSize), Player.Black);
    }

    [Test]
    public void IsOver_NewGame_ReturnsFalse()
    {
        bool isOver = _gameState.IsOver();

        Assert.That(isOver, Is.False);
    }

    [Test]
    public void IsOver_PlayOrPass_ReturnsFalse()
    {
        IGameState nextState = _gameState.ApplyMove(Move.Play(new(1, 1)));
        nextState = nextState.ApplyMove(Move.Pass());
        nextState = nextState.ApplyMove(Move.Play(new(2, 3)));
        nextState = nextState.ApplyMove(Move.Pass());

        bool isOver = nextState.IsOver();

        Assert.That(isOver, Is.False);
    }

    [Test]
    public void IsOver_LastMoveIsResign_ReturnsTrue()
    {
        IGameState nextState = _gameState.ApplyMove(Move.Resign());

        bool isOver = nextState.IsOver();

        Assert.That(isOver, Is.True);
    }

    [Test]
    public void IsOver_TwoConsecutivePasses_ReturnsTrue()
    {
        Move pass = Move.Pass();

        IGameState nextState = _gameState.ApplyMove(pass);
        nextState = nextState.ApplyMove(pass);

        bool isOver = nextState.IsOver();

        Assert.That(isOver, Is.True);
    }

    /*
        o s o
        . o .
        . . .
    */
    [Test]
    public void IsSuicide_PointIsSurroundedByOpponentStones_ReturnsTrue()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.White, new Point(0, 0));
        board.PlaceStone(Player.White, new Point(1, 1));
        board.PlaceStone(Player.White, new Point(0, 2));

        Point suicidePoint = new(0, 1);

        bool isSucide = _gameState.IsSuicide(suicidePoint);

        Assert.That(isSucide, Is.True);
    }

    /*
        x s x
        . x .
        . . .
    */
    [Test]
    public void IsSuicide_PointIsSurroundedByPlayerStones_ReturnsFalse()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.Black, new Point(0, 0));
        board.PlaceStone(Player.Black, new Point(1, 1));
        board.PlaceStone(Player.Black, new Point(0, 2));

        Point point = new(0, 1);

        bool isSucide = _gameState.IsSuicide(point);

        Assert.That(isSucide, Is.False);
    }

    /*
        o s o
        x o .
        . . .
    */
    [Test]
    public void IsSuicide_CapturingPoint_ReturnsFalse()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.White, new Point(0, 0));
        board.PlaceStone(Player.White, new Point(1, 1));
        board.PlaceStone(Player.White, new Point(0, 2));
        board.PlaceStone(Player.Black, new Point(1, 0));

        Point capturingPoint = new(0, 1);

        bool isSucide = _gameState.IsSuicide(capturingPoint);

        Assert.That(isSucide, Is.False);
    }

    /*
        o . o
        . . .
        . . .
    */
    [Test]
    public void IsSuicide_SelfAtariPoint_ReturnsFalse()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.White, new Point(0, 0));
        board.PlaceStone(Player.White, new Point(0, 2));

        Point selfAtariPoint = new(0, 1);

        bool isSucide = _gameState.IsSuicide(selfAtariPoint);

        Assert.That(isSucide, Is.False);
    }

    /*
        o . o   . x o   o . o
        x o .   x o .   x o .
        . . .   . . .   . . .
    */
    [Test]
    public void DoesViolateKo_PlayerTriesToImediatelyRecaptureStone_ReturnsTrue()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.White, new Point(0, 0));
        board.PlaceStone(Player.White, new Point(1, 1));
        board.PlaceStone(Player.White, new Point(0, 2));
        board.PlaceStone(Player.Black, new Point(1, 0));

        _gameState = _gameState.ApplyMove(Move.Play(new(0, 1)));

        Point koViolationPoint = new(0, 0);

        bool doesViolateKo = _gameState.DoesViolateKo(koViolationPoint);

        Assert.That(doesViolateKo, Is.True);
    }

    [Test]
    public void DoesViolateKo_PlayerRecapturesStoneAfterTenuki_ReturnsFalse()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.White, new Point(0, 0));
        board.PlaceStone(Player.White, new Point(1, 1));
        board.PlaceStone(Player.White, new Point(0, 2));
        board.PlaceStone(Player.Black, new Point(1, 0));

        // black captures
        IGameState nextState = _gameState.ApplyMove(Move.Play(new(0, 1)));

        // white connects
        nextState = nextState.ApplyMove(Move.Play(new(1, 2)));

        // black tenuki
        nextState = nextState.ApplyMove(Move.Play(new(_boardSize - 1, _boardSize - 1)));

        Point koPoint = new(0, 0);

        bool doesViolateKo = nextState.DoesViolateKo(koPoint);

        Assert.That(doesViolateKo, Is.False);
    }

    [Test]
    public void DoesViolateKo_PlayerRecapturesStoneAfterPass_ReturnsFalse()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.White, new Point(0, 0));
        board.PlaceStone(Player.White, new Point(1, 1));
        board.PlaceStone(Player.White, new Point(0, 2));
        board.PlaceStone(Player.Black, new Point(1, 0));

        // black captures
        IGameState nextState = _gameState.ApplyMove(Move.Play(new(0, 1)));

        // white pass
        nextState = nextState.ApplyMove(Move.Pass());

        // black tenuki
        nextState = nextState.ApplyMove(Move.Play(new(_boardSize - 1, _boardSize - 1)));

        Point koPoint = new(0, 0);

        bool doesViolateKo = nextState.DoesViolateKo(koPoint);

        Assert.That(doesViolateKo, Is.False);
    }

    [Test]
    public void GetWinner_GameNotOver_ReturnsNone()
    {
        Player winner = _gameState.GetWinner();

        Assert.That(winner, Is.EqualTo(Player.None));
    }

    [Test]
    public void GetWinner_BlackResigns_ReturnsPlayerWhite()
    {
        IGameState nextState = _gameState.ApplyMove(Move.Resign());

        Player winner = nextState.GetWinner();

        Assert.That(winner, Is.EqualTo(Player.White));
    }

    [Test]
    public void GetWinner_WhiteResigns_ReturnsPlayerBlack()
    {
        // black passes
        IGameState nextState = _gameState.ApplyMove(Move.Pass());
        // white resigns
        nextState = nextState.ApplyMove(Move.Resign());

        Player winner = nextState.GetWinner();

        Assert.That(winner, Is.EqualTo(Player.Black));
    }

    [Test]
    public void GetWinner_EmptyBoardWinByKomi_ReturnsPlayerWhite()
    {
        IGameState nextState = _gameState.ApplyMove(Move.Resign());

        Player winner = nextState.GetWinner();

        Assert.That(winner, Is.EqualTo(Player.White));
    }

    [Test]
    public void GetWinner_BlackHasBiggerTerritory_ReturnsPlayerBlack()
    {
        Board board = _gameState.Board;
        board.PlaceStone(Player.Black, new Point(0, 2));
        board.PlaceStone(Player.Black, new Point(1, 2));
        board.PlaceStone(Player.Black, new Point(2, 2));
        board.PlaceStone(Player.Black, new Point(3, 2));
        board.PlaceStone(Player.Black, new Point(4, 2));

        IGameState nextState = _gameState.ApplyMove(Move.Pass());
        nextState = nextState.ApplyMove(Move.Pass());

        Player winner = nextState.GetWinner();

        Assert.That(winner, Is.EqualTo(Player.Black));
    }

    [TestCase(Player.Black, "Black to move")]
    [TestCase(Player.White, "White to move")]
    public void PrintLastMove_NewGame_PrintedMoveAsExpected(Player player, string expectedLastMove)
    {
        IGameState gameState = new GoState(new Board(_boardSize), player);

        string lastMove = gameState.PrintLastMove();

        Assert.That(lastMove, Is.EqualTo(expectedLastMove));
    }

    [TestCase(Player.Black, "Black PASS")]
    [TestCase(Player.White, "White PASS")]
    public void PrintLastMove_LastMoveIsPass_PrintedMoveAsExpected(Player player, string expectedLastMove)
    {
        IGameState gameState = new GoState(new Board(_boardSize), player);
        gameState = gameState.ApplyMove(Move.Pass());

        string lastMove = gameState.PrintLastMove();

        Assert.That(lastMove, Is.EqualTo(expectedLastMove));
    }

    [TestCase(Player.Black, "Black RESIGN")]
    [TestCase(Player.White, "White RESIGN")]
    public void PrintLastMove_LastMoveIsResign_PrintedMoveAsExpected(Player player, string expectedLastMove)
    {
        IGameState gameState = new GoState(new Board(_boardSize), player);
        gameState = gameState.ApplyMove(Move.Resign());

        string lastMove = gameState.PrintLastMove();

        Assert.That(lastMove, Is.EqualTo(expectedLastMove));
    }

    /*
        . . .
        . . .
        . . .
    */
    [TestCase(Player.Black, 0, 0, "Black A5")]
    [TestCase(Player.White, 0, 0, "White A5")]
    [TestCase(Player.Black, _boardSize - 1, _boardSize - 1, "Black E1")]
    [TestCase(Player.White, _boardSize - 1, _boardSize - 1, "White E1")]
    public void PrintLastMove_LastMoveIsPlay_ReturnsExpectedLastMove(Player player, int row, int column, string expectedLastMove)
    {
        IGameState gameState = new GoState(new Board(_boardSize), player);
        gameState = gameState.ApplyMove(Move.Play(new(row, column)));

        string lastMove = gameState.PrintLastMove();

        Assert.That(lastMove, Is.EqualTo(expectedLastMove));
    }

    /*
        o s o
        . o .
        . . .
    */
    // [Test]
    // public void ApplyMove_IsSuicide_ThrowsException()
    // {
    //     Board board = _gameState.Board;
    //     board.PlaceStone(Player.White, new Point(0, 0));
    //     board.PlaceStone(Player.White, new Point(1, 1));
    //     board.PlaceStone(Player.White, new Point(0, 1));

    //     Move suicideMove = Move.Play(new(1, 2));

    //     _gameState.ApplyMove(suicideMove);

    //     InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => _gameState.ApplyMove(suicideMove));
    //     Assert.That(exception.Message, Is.EqualTo(ErrorMessages.IsSuicideMove));
    // }
}