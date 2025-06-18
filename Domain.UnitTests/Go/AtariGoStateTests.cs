using Domain.Go;

namespace Domain.UnitTests.Go;

public class AtariGoStateTests
{
    private const int _boardSize = 5;
    private const int _capturesToWin = 1;

    [Test]
    public void IsOver_NewGame_ReturnsFalse()
    {
        GameState gameState = new AtariGoState(new Board(_boardSize), Player.Black);

        bool isOver = gameState.IsOver();

        Assert.That(isOver, Is.False);
    }

    /*
        o . .
        x . .
        . . .
    */
    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void IsOver_NoCaptures_ReturnsFalse(Player player)
    {
        Player opponent = player.Other();

        Board board = new(_boardSize);
        board.PlaceStone(player, new(1, 0));
        board.PlaceStone(opponent, new(0, 0));

        IGameState gameState = new AtariGoState(board, player);
        
        bool isOver = gameState.IsOver();

        Assert.That(isOver, Is.False);
    }

    /*
        o . .   . x .
        x . .   x . .
        . . .   . . .
    */
    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void IsOver_PlayerCaptureStone_ReturnsTrue(Player player)
    {
        Player opponent = player.Other();

        Board board = new(_boardSize);
        board.PlaceStone(player, new(1, 0));
        board.PlaceStone(opponent, new(0, 0));

        IGameState gameState = new AtariGoState(board, player, _capturesToWin);
        
        gameState = gameState.ApplyMove(Move.Play(new(0, 1)));

        bool isOver = gameState.IsOver();

        Assert.Multiple(() =>
        {
            Assert.That(isOver, Is.True);
            Assert.That(gameState.Board.GetCapturedStones(player), Is.EqualTo(_capturesToWin));
        });
    }

    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void IsOver_LastMoveIsResign_ReturnsTrue(Player player)
    {
        IGameState gameState = new AtariGoState(new Board(_boardSize), player);

        gameState = gameState.ApplyMove(Move.Resign());

        bool isOver = gameState.IsOver();

        Assert.That(isOver, Is.True);
    }

    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void GetWinner_PlayerResigns_ReturnsOpponent(Player player)
    {
        IGameState gameState = new AtariGoState(new Board(_boardSize), player);

        gameState = gameState.ApplyMove(Move.Resign());

        Player winner = gameState.GetWinner();

        Assert.That(winner, Is.EqualTo(player.Other()));
    }

    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void GetWinner_PlayerCapturesRequiredAmountOfStones_ReturnsPlayer(Player player)
    {
        Player opponent = player.Other();

        Board board = new(_boardSize);
        board.PlaceStone(player, new(1, 0));
        board.PlaceStone(opponent, new(0, 0));
        board.PlaceStone(player, new(0, 1));

        IGameState gameState = new AtariGoState(board, player, _capturesToWin);

        Player winner = gameState.GetWinner();

        Assert.That(winner, Is.EqualTo(player));
    }

    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void GetWinner_NoCaptures_ReturnsNone(Player player)
    {
        IGameState gameState = new AtariGoState(new Board(_boardSize), player, _capturesToWin);

        Player winner = gameState.GetWinner();

        Assert.That(winner, Is.EqualTo(Player.None));
    }
}