using Domain.Go;
using Domain.Scoring;

namespace Domain.UnitTests.Scoring;

public class TerritoryTests
{
    private const int _boardSize = 5;
    private Board _board;

    [SetUp]
    public void Setup()
    {
        _board = new(_boardSize);
    }

    /*
        . o . o o
        o o o o .
        x x x o o
        . x x x x
        . x . x .
    */
    [TestCase]
    public void ScoringTest1()
    {
        _board.PlaceStone(Player.White, new(1, 0));
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(1, 2));
        _board.PlaceStone(Player.White, new(1, 3));
        _board.PlaceStone(Player.White, new(0, 3));
        _board.PlaceStone(Player.White, new(1, 4));
        _board.PlaceStone(Player.White, new(2, 3));
        _board.PlaceStone(Player.White, new(2, 4));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(2, 1));
        _board.PlaceStone(Player.Black, new(2, 2));
        _board.PlaceStone(Player.Black, new(3, 1));
        _board.PlaceStone(Player.Black, new(3, 2));
        _board.PlaceStone(Player.Black, new(3, 3));
        _board.PlaceStone(Player.Black, new(3, 4));
        _board.PlaceStone(Player.Black, new(4, 1));
        _board.PlaceStone(Player.Black, new(4, 3));

        Territory territory = Territory.EvaluateTerritory(_board);

        Assert.Multiple(() =>
        {
            Assert.That(territory.BlackStones, Is.EqualTo(9));
            Assert.That(territory.BlackTerritory, Is.EqualTo(4));
            Assert.That(territory.WhiteStones, Is.EqualTo(9));
            Assert.That(territory.WhiteTerritory, Is.EqualTo(3));
            Assert.That(territory.Dame, Is.EqualTo(0));
        });
    }

    /*
        . o . x .
        . o . x .
        . o . x .
        . o . x .
        . o . x .
    */
    [Test]
    public void ScoringTest2()
    {
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(2, 1));
        _board.PlaceStone(Player.White, new(3, 1));
        _board.PlaceStone(Player.White, new(4, 1));
        _board.PlaceStone(Player.Black, new(0, 3));
        _board.PlaceStone(Player.Black, new(1, 3));
        _board.PlaceStone(Player.Black, new(2, 3));
        _board.PlaceStone(Player.Black, new(3, 3));
        _board.PlaceStone(Player.Black, new(4, 3));

        Territory territory = Territory.EvaluateTerritory(_board);

        Assert.Multiple(() =>
        {
            Assert.That(territory.BlackStones, Is.EqualTo(5));
            Assert.That(territory.BlackTerritory, Is.EqualTo(5));
            Assert.That(territory.WhiteStones, Is.EqualTo(5));
            Assert.That(territory.WhiteTerritory, Is.EqualTo(5));
            Assert.That(territory.Dame, Is.EqualTo(5));
        });
    }

    /*
        . o . o x
        o o o o x
        . . x x x
        . x x . x
        . x . x x
    */
    [Test]
    public void ScoringTest3()
    {
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.White, new(0, 3));
        _board.PlaceStone(Player.White, new(1, 0));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(1, 2));
        _board.PlaceStone(Player.White, new(1, 3));
        _board.PlaceStone(Player.Black, new(0, 4));
        _board.PlaceStone(Player.Black, new(1, 4));
        _board.PlaceStone(Player.Black, new(2, 2));
        _board.PlaceStone(Player.Black, new(2, 3));
        _board.PlaceStone(Player.Black, new(2, 4));
        _board.PlaceStone(Player.Black, new(3, 1));
        _board.PlaceStone(Player.Black, new(3, 2));
        _board.PlaceStone(Player.Black, new(3, 4));
        _board.PlaceStone(Player.Black, new(4, 1));
        _board.PlaceStone(Player.Black, new(4, 3));
        _board.PlaceStone(Player.Black, new(4, 4));


        Territory territory = Territory.EvaluateTerritory(_board);

        Assert.Multiple(() =>
        {
            Assert.That(territory.BlackStones, Is.EqualTo(11));
            Assert.That(territory.BlackTerritory, Is.EqualTo(2));
            Assert.That(territory.WhiteStones, Is.EqualTo(6));
            Assert.That(territory.WhiteTerritory, Is.EqualTo(2));
            Assert.That(territory.Dame, Is.EqualTo(4));
        });
    }

    /*
        . x x o .
        x . x o .
        x x o . .
        o o . o .
        . . . . .
    */
    [Test]
    public void ScoringTest4()
    {
        _board.PlaceStone(Player.White, new(0, 3));
        _board.PlaceStone(Player.White, new(1, 3));
        _board.PlaceStone(Player.White, new(2, 2));
        _board.PlaceStone(Player.White, new(3, 0));
        _board.PlaceStone(Player.White, new(3, 1));
        _board.PlaceStone(Player.White, new(3, 3));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 0));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(2, 1));

        Territory territory = Territory.EvaluateTerritory(_board);

        Assert.Multiple(() =>
        {
            Assert.That(territory.BlackStones, Is.EqualTo(6));
            Assert.That(territory.BlackTerritory, Is.EqualTo(2));
            Assert.That(territory.WhiteStones, Is.EqualTo(6));
            Assert.That(territory.WhiteTerritory, Is.EqualTo(11));
            Assert.That(territory.Dame, Is.EqualTo(0));
        });
    }
}
