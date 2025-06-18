using Domain.Go;

namespace Domain.UnitTests.Go;

public class GameStateTests
{
    private const int _boardSize = 5;
    private IGameState _gameState;

    [SetUp]
    public void Setup()
    {
        _gameState = new GoState(new Board(_boardSize), Player.Black);
    }

    [TestCase(0, 0)]
    [TestCase(_boardSize - 1, _boardSize - 1)]
    public void ApplyMove_PlayMove_NextStateIncludesPreviousStateInfo(int row, int column)
    {
        Point point = new(row, column);
        Move move = Move.Play(point);

        IGameState nextState = _gameState.ApplyMove(move);

        Assert.Multiple(() =>
        {
            Assert.That(nextState.NextPlayer, Is.EqualTo(Player.White));
            Assert.That(nextState.LastMove, Is.EqualTo(move));
            Assert.That(nextState.PreviousLastMove, Is.EqualTo(_gameState.LastMove));
            Assert.That(nextState.PreviousBoard, Is.EqualTo(_gameState.Board));
            Assert.That(nextState.Board, Is.Not.EqualTo(_gameState.Board));
        });
    }

    [TestCase(0, 0)]
    [TestCase(4, 3)]
    [TestCase(3, 4)]
    [TestCase(_boardSize - 1, _boardSize - 1)]
    public void ApplyMove_PlayMove_NextStateIncludesLastMoveAndUpdatedBoard(int row, int column)
    {
        Point point = new(row, column);
        Move play = Move.Play(point);

        IGameState nextState = _gameState.ApplyMove(play);

        Assert.Multiple(() =>
        {
            Assert.That(nextState.LastMove, Is.EqualTo(play));
            Assert.That(nextState.Board, Is.Not.EqualTo(_gameState.Board));
            Assert.That(nextState.Board.GetPlayer(point), Is.EqualTo(Player.Black));
        });
    }

    [Test]
    public void ApplyMove_Pass_NextStateIncludesSameBoard()
    {
        Move pass = Move.Pass();

        IGameState nextState = _gameState.ApplyMove(pass);

        Assert.Multiple(() =>
        {
            Assert.That(nextState.LastMove, Is.EqualTo(pass));
            Assert.That(nextState.Board, Is.EqualTo(_gameState.Board));
        });
    }

    [Test]
    public void ApplyMove_Resign_NextStateIncludesSameBoard()
    {
        Move resign = Move.Resign();

        IGameState nextState = _gameState.ApplyMove(resign);

        Assert.Multiple(() =>
        {
            Assert.That(nextState.LastMove, Is.EqualTo(resign));
            Assert.That(nextState.Board, Is.EqualTo(_gameState.Board));
        });
    }
}