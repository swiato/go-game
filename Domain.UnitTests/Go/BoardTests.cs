using Domain.Go;
using Domain.Common;

namespace Domain.UnitTests.Go;

public class BoardTests
{
    private const int _boardSize = 5;
    private Board _board;

    [SetUp]
    public void Setup()
    {
        _board = new(_boardSize);
    }

    /*
        . . .
        . . .
        . . .
    */
    [TestCase(-1, -1)]
    [TestCase(-1, _boardSize)]
    [TestCase(_boardSize, -1)]
    [TestCase(_boardSize, _boardSize)]
    public void PlaceStone_StoneOutsideBoard_ThrowsArgumentException(int row, int column)
    {
        var point = new Point(row, column);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => _board.PlaceStone(Player.Black, point));
        Assert.That(exception.Message, Is.EqualTo(ErrorMessages.PointIsOutsideGrid));
    }

    /*
        . . .   . . .
        . x .   . o .
        . . .   . . .
    */
    [Test]
    public void PlaceStone_PlaceIsTaken_ThrowsArgumentException()
    {
        var point = new Point(1, 1);
        _board.PlaceStone(Player.Black, point);

        ArgumentException exception = Assert.Throws<ArgumentException>(() => _board.PlaceStone(Player.White, point));
        Assert.That(exception.Message, Is.EqualTo(ErrorMessages.PointIsAlreadyTaken));
    }

    /*
        . ^ .
        ^ x ^
        . ^ .
    */
    [Test]
    public void PlaceStone_CenterStone_HasFourLiberties()
    {
        var point = new Point(1, 1);

        _board.PlaceStone(Player.Black, point);

        Point[] expectedStones = [point];
        Point[] expectedLiberties = [new Point(0, 1), new Point(2, 1), new Point(1, 0), new Point(1, 2)];

        Chain blackChain = _board.GetChain(point)!;

        Assert.Multiple(() =>
        {
            Assert.That(blackChain.Player, Is.EqualTo(Player.Black));
            Assert.That(expectedStones, Is.EquivalentTo(blackChain.Stones));
            Assert.That(expectedLiberties, Is.EquivalentTo(blackChain.Liberties));
        });
    }

    /*
        ^ x ^
        . ^ .
        . . .
    */
    [Test]
    public void PlaceStone_EdgeStone_HasThreeLiberties()
    {
        Point point = new(0, 1);

        _board.PlaceStone(Player.Black, point);

        Point[] expectedStones = [point];
        Point[] expectedLiberties = [new(0, 0), new(0, 2), new(1, 1)];

        Chain blackChain = _board.GetChain(point)!;

        Assert.Multiple(() =>
        {
            Assert.That(blackChain.Player, Is.EqualTo(Player.Black));
            Assert.That(expectedStones, Is.EquivalentTo(blackChain.Stones));
            Assert.That(expectedLiberties, Is.EquivalentTo(blackChain.Liberties));
        });
    }

    /*
        x ^ .
        ^ . .
        . . .
    */
    [Test]
    public void PlaceStone_CornerStone_HasTwoLiberties()
    {
        Point point = new(0, 0);

        _board.PlaceStone(Player.Black, point);

        Point[] expectedStones = [point];
        Point[] expectedLiberties = [new(1, 0), new(0, 1)];

        Chain blackChain = _board.GetChain(point)!;

        Assert.Multiple(() =>
        {
            Assert.That(blackChain.Player, Is.EqualTo(Player.Black));
            Assert.That(expectedStones, Is.EquivalentTo(blackChain.Stones));
            Assert.That(expectedLiberties, Is.EquivalentTo(blackChain.Liberties));
        });
    }

    /*
        . x .   . x .
        x o x   x . x
        . . .   . x .
    */
    [Test]
    public void PlaceStone_CenterStoneInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(0, 1));

        // act
        _board.PlaceStone(Player.Black, new(2, 1));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 0)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 2)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(2, 1)), Is.EqualTo(Player.Black));
        });
    }

    /*
        . x .   . x .
        x o x   x . x
        . . .   . x .
    */
    [Test]
    public void PlaceStone_StoneCaptured_ReturnsLibertyToOpponent()
    {
        // arrange
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(0, 1));

        // act
        _board.PlaceStone(Player.Black, new(2, 1));

        // assert
        Chain blackChain = _board.GetChain(new(1, 2))!;
        Assert.That(blackChain.Liberties, Does.Contain(new Point(1, 1)));
    }

    /*
        . x .   . x .
        x o x   x . x
        . . .   . x .
    */
    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void PlaceStone_StoneCaptured_StoneAddedToPlayerCapturedStones(Player player)
    {
        // arrange
        Player opponent = player.Other();

        _board.PlaceStone(opponent, new(1, 1));
        _board.PlaceStone(player, new(1, 0));
        _board.PlaceStone(player, new(1, 2));
        _board.PlaceStone(player, new(0, 1));

        // act
        _board.PlaceStone(player, new(2, 1));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetCapturedStones(player), Is.EqualTo(1));
            Assert.That(_board.GetCapturedStones(opponent), Is.Zero);
        });
    }

    /*
        . o .   . o .
        o . o   o x o
        . o .   . o .
    */
    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void PlaceStone_StoneSelfCaptured_StoneAddedToOpponentCapturedStones(Player player)
    {

        // arrange
        Player opponent = player.Other();

        _board.PlaceStone(opponent, new(0, 1));
        _board.PlaceStone(opponent, new(1, 0));
        _board.PlaceStone(opponent, new(1, 2));
        _board.PlaceStone(opponent, new(2, 1));

        // act
        _board.PlaceStone(player, new(1, 1));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetCapturedStones(player), Is.Zero);
            Assert.That(_board.GetCapturedStones(opponent), Is.EqualTo(1));
        });
    }

    /*
        o . .   . x .
        x . .   x . .
        . . .   . . .
    */
    [Test]
    public void PlaceStone_CornerStoneInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(0, 0));
        _board.PlaceStone(Player.Black, new(1, 0));

        // act
        _board.PlaceStone(Player.Black, new(0, 1));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(0, 0)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 0)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.Black));
        });
    }

    /*
        . o x   x . x
        . x .   . x .
        . . .   . . .
    */
    [Test]
    public void PlaceStone_EdgeStoneInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 1));

        // act
        _board.PlaceStone(Player.Black, new(0, 0));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(0, 2)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 0)), Is.EqualTo(Player.Black));
        });
    }

    /*
        . x x .    . x x .
        x o o x    x . . x
        . x . .    . x x .
    */
    [Test]
    public void PlaceStone_CenterChainInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(1, 2));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 0));
        _board.PlaceStone(Player.Black, new(1, 3));
        _board.PlaceStone(Player.Black, new(2, 1));

        // act
        _board.PlaceStone(Player.Black, new(2, 2));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 2)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 2)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 0)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 3)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(2, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(2, 2)), Is.EqualTo(Player.Black));
        });
    }

    /*
        o o .   . . x
        x x .   x x .
        . . .   . . .
    */
    [Test]
    public void PlaceStone_CornerChainInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(0, 0));
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.Black, new(1, 0));
        _board.PlaceStone(Player.Black, new(1, 1));

        // act
        _board.PlaceStone(Player.Black, new(0, 2));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(0, 0)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 0)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 2)), Is.EqualTo(Player.Black));
        });
    }

    /*
        . o o x    x . . x
        . x x .    . x x .
        . . . .    . . . .
    */
    [Test]
    public void PlaceStone_EdgeChainInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.White, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(0, 3));

        // act
        _board.PlaceStone(Player.Black, new(0, 0));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(0, 2)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 2)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 3)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 0)), Is.EqualTo(Player.Black));
        });
    }

    /*
        o o .   . . x
        o o x   . . x
        x x .   x x .
    */
    [Test]
    public void PlaceStone_ChainInAtari_OpponentCaptures()
    {
        // arrange
        _board.PlaceStone(Player.White, new(0, 0));
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.White, new(1, 0));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(2, 1));
        _board.PlaceStone(Player.Black, new(1, 2));

        // act
        _board.PlaceStone(Player.Black, new(0, 2));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(0, 0)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 0)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(2, 0)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(2, 1)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(1, 2)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 2)), Is.EqualTo(Player.Black));
        });
    }

    /*
        o o .   . . x
        o o x   . . x
        x x .   x x .
    */
    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void PlaceStone_ChainCaptured_StonesAddedToPlayerCapturedStones(Player player)
    {
        Player opponent = player.Other();

        // arrange
        _board.PlaceStone(opponent, new(0, 0));
        _board.PlaceStone(opponent, new(0, 1));
        _board.PlaceStone(opponent, new(1, 0));
        _board.PlaceStone(opponent, new(1, 1));
        _board.PlaceStone(player, new(2, 0));
        _board.PlaceStone(player, new(2, 1));
        _board.PlaceStone(player, new(1, 2));

        // act
        _board.PlaceStone(player, new(0, 2));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetCapturedStones(player), Is.EqualTo(4));
            Assert.That(_board.GetCapturedStones(opponent), Is.Zero);
        });
    }

    /*
        . x o   . . o
        x x o   . . o
        o o .   o o .
    */
    [TestCase(Player.Black)]
    [TestCase(Player.White)]
    public void PlaceStone_ChainSelfCaptured_StonesAddedToOpponentCapturedStones(Player player)
    {
        Player opponent = player.Other();

        // arrange
        _board.PlaceStone(player, new(0, 1));
        _board.PlaceStone(player, new(1, 0));
        _board.PlaceStone(player, new(1, 1));
        _board.PlaceStone(opponent, new(0, 2));
        _board.PlaceStone(opponent, new(1, 2));
        _board.PlaceStone(opponent, new(2, 0));
        _board.PlaceStone(opponent, new(2, 1));

        // act
        _board.PlaceStone(player, new(0, 0));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetCapturedStones(player), Is.Zero);
            Assert.That(_board.GetCapturedStones(opponent), Is.EqualTo(4));
        });
    }

    /*
        o . o   . x o
        x o .   x o .
        . . .   . . .
    */
    [Test]
    public void PlaceStone_Capture_IsNotSuicideMove()
    {
        // arrange
        _board.PlaceStone(Player.White, new(0, 0));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 0));

        // act
        _board.PlaceStone(Player.Black, new(0, 1));

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(_board.GetPlayer(new(0, 0)), Is.EqualTo(Player.None));
            Assert.That(_board.GetPlayer(new(1, 1)), Is.EqualTo(Player.White));
            Assert.That(_board.GetPlayer(new(0, 2)), Is.EqualTo(Player.White));
            Assert.That(_board.GetPlayer(new(1, 0)), Is.EqualTo(Player.Black));
            Assert.That(_board.GetPlayer(new(0, 1)), Is.EqualTo(Player.Black));
        });
    }

    /*
        . ^ .   . ^ .
        ^ x ^   ^ x ^
        . ^ o   . o o
    */
    [Test]
    public void PlaceStone_InOpponentLiberty_TakesLiberty()
    {
        // arrange
        _board.PlaceStone(Player.Black, new(1, 1));
        _board.PlaceStone(Player.White, new(2, 2));

        // act
        _board.PlaceStone(Player.White, new(2, 1));

        //assert
        var blackChain = _board.GetChain(new(1, 1))!;

        Assert.That(blackChain.Liberties, Does.Not.Contain(new Point(2, 1)));
    }

    /*
        x x ^   x x ^ 
        ^ x ^   o x ^
        . ^ .   . ^ .
    */
    [Test]
    public void PlaceStone_InEmptyTriangle_TakesOneLiberty()
    {
        // arrange
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(1, 1));

        // act
        _board.PlaceStone(Player.White, new(1, 0));

        // assert
        Chain blackChain = _board.GetChain(new(0, 0))!;

        Point[] expectedLiberties = [new(2, 1), new(1, 2), new(0, 2)];

        Assert.That(blackChain.Liberties, Is.EquivalentTo(expectedLiberties));
    }

    /*
        . ^ .   . ^ ^ .
        ^ x ^   ^ x x ^
        . ^ .   . ^ ^ .
    */
    [Test]
    public void PlaceStone_NearFriendlyStone_MergesStonesIntoChain()
    {
        // arrange
        _board.PlaceStone(Player.Black, new(1, 1));

        // act
        _board.PlaceStone(Player.Black, new(1, 2));

        // assert
        Point[] expectedStones = [new(1, 1), new(1, 2)];
        Point[] expectedLiberties = [new(1, 0), new(0, 1), new(0, 2), new(2, 1), new(2, 2), new(1, 3)];

        Chain blackChain = _board.GetChain(new(1, 1))!;

        Assert.Multiple(() =>
        {
            Assert.That(_board.GetChain(new(1, 2)), Is.EqualTo(blackChain));
            Assert.That(blackChain.Stones, Is.EquivalentTo(expectedStones));
            Assert.That(blackChain.Liberties, Is.EquivalentTo(expectedLiberties));
        });
    }

    /*
        ^ ^ .   ^ ^ ^ .
        x x ^   x x x ^
        ^ ^ .   ^ ^ ^ .
    */
    [Test]
    public void PlaceStone_ExtendFriendlyChain_MergesStoneIntoChain()
    {
        // arrange
        _board.PlaceStone(Player.Black, new(1, 0));
        _board.PlaceStone(Player.Black, new(1, 1));

        // act
        _board.PlaceStone(Player.Black, new(1, 2));

        // assert
        Chain blackChain = _board.GetChain(new(1, 0))!;

        Point[] expectedStones = [new(1, 0), new(1, 1), new(1, 2)];
        Point[] expectedLiberties = [new(0, 0), new(0, 1), new(0, 2), new(2, 0), new(2, 1), new(2, 2), new(1, 3)];

        Assert.Multiple(() =>
        {
            Assert.That(_board.GetChain(new(1, 2)), Is.EqualTo(blackChain));
            Assert.That(blackChain.Stones, Is.EquivalentTo(expectedStones));
            Assert.That(blackChain.Liberties, Is.EquivalentTo(expectedLiberties));
        });
    }

    /*
        x x ^   x x ^
        ^ ^ .   ^ x ^
        . . .   . ^ .
    */
    [Test]
    public void PlaceStone_FormEmptyTriangle_MergesStoneIntoChain()
    {
        // arrange
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));

        // act
        _board.PlaceStone(Player.Black, new(1, 1));

        // assert
        Chain blackChain = _board.GetChain(new(0, 0))!;

        Point[] expectedStones = [new(0, 0), new(0, 1), new(1, 1)];
        Point[] expectedLiberties = [new(1, 0), new(2, 1), new(0, 2), new(1, 2)];

        Assert.Multiple(() =>
        {
            Assert.That(_board.GetChain(new(1, 1)), Is.EqualTo(blackChain));
            Assert.That(blackChain.Stones, Is.EquivalentTo(expectedStones));
            Assert.That(blackChain.Liberties, Is.EquivalentTo(expectedLiberties));
        });
    }

    /*
        x x ^   x x ^
        ^ ^ ^   ^ x ^
        ^ x x   ^ x x
    */
    [Test]
    public void PlaceStone_ConnectTwoChains_MergesAllStonesIntoOneChain()
    {
        // arrange
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(2, 1));
        _board.PlaceStone(Player.Black, new(2, 2));

        // act
        _board.PlaceStone(Player.Black, new(1, 1));

        // assert
        Chain blackChain = _board.GetChain(new(0, 0))!;

        Point[] expectedStones = [new(0, 0), new(0, 1), new(1, 1), new(2, 1), new(2, 2)];
        Point[] expectedLiberties = [new(1, 0), new(2, 0), new(3, 1), new(3, 2), new(2, 3), new(1, 2), new(0, 2)];

        Assert.Multiple(() =>
        {
            Assert.That(_board.GetChain(new(1, 1)), Is.EqualTo(blackChain));
            Assert.That(blackChain.Stones, Is.EquivalentTo(expectedStones));
            Assert.That(blackChain.Liberties, Is.EquivalentTo(expectedLiberties));
        });
    }

    [TestCase(Player.Black, 1, 1)]
    [TestCase(Player.Black, 3, 2)]
    [TestCase(Player.White, 1, 1)]
    [TestCase(Player.White, 3, 2)]
    public void PlaceStone_BoardsWithSameStoneSetup_ReturnsSameZobristHash(Player player, int row, int column)
    {
        _board.PlaceStone(player, new(row, column));

        Board secondBoard = new(_boardSize);
        secondBoard.PlaceStone(player, new(row, column));

        Assert.That(_board.Hash, Is.EqualTo(secondBoard.Hash));
    }

    [Test]
    public void PlaceStone_SameStoneSetupPlacedInDifferentOrder_ReturnsSameZobristHash()
    {
        _board.PlaceStone(Player.White, new(0, 0));
        _board.PlaceStone(Player.White, new(0, 1));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(1, 0));

        Board secondBoard = new(_boardSize);
        secondBoard.PlaceStone(Player.White, new(0, 0));
        secondBoard.PlaceStone(Player.White, new(1, 0));
        secondBoard.PlaceStone(Player.White, new(1, 1));
        secondBoard.PlaceStone(Player.White, new(0, 1));

        Assert.That(_board.Hash, Is.EqualTo(secondBoard.Hash));
    }

    /*
        o . o   . x o   o . o
        x o .   x o .   x o .
        . . .   . . .   . . .
    */
    [Test]
    public void PlaceStone_BackToKoStartingPosition_ReturnsSameZobristHash()
    {
        // create ko
        _board.PlaceStone(Player.White, new(0, 0));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.White, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 0));

        // back to ko starting position
        Board secondBoard = new(_board);
        secondBoard.PlaceStone(Player.Black, new(0, 1));
        secondBoard.PlaceStone(Player.White, new(0, 0));

        Assert.That(_board.Hash, Is.EqualTo(secondBoard.Hash));
    }

    [TestCase(1, -1)]
    [TestCase(-1, 1)]
    [TestCase(-1, -1)]
    [TestCase(1, _boardSize)]
    [TestCase(_boardSize, 1)]
    [TestCase(_boardSize, _boardSize)]
    public void IsOutsideBoard_PointOutsideBoard_ReturnsTrue(int row, int column)
    {
        bool isOutsideBoard = _board.IsOutsideGrid(new(row, column));

        Assert.That(isOutsideBoard, Is.True);
    }

    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(1, 2)]
    [TestCase(2, 1)]
    [TestCase(_boardSize - 1, _boardSize - 1)]
    public void IsOutsideBoard_PointInsideBoard_ReturnsFalse(int row, int column)
    {
        bool isOutsideBoard = _board.IsOutsideGrid(new(row, column));

        Assert.That(isOutsideBoard, Is.False);
    }

    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(1, 2)]
    [TestCase(2, 1)]
    [TestCase(_boardSize - 1, _boardSize - 1)]
    public void IsOnBoard_PointInsideBoard_ReturnsTrue(int row, int column)
    {
        bool isOnBoard = _board.IsOnGrid(new(row, column));

        Assert.That(isOnBoard, Is.True);
    }

    [TestCase(1, -1)]
    [TestCase(-1, 1)]
    [TestCase(-1, -1)]
    [TestCase(1, _boardSize)]
    [TestCase(_boardSize, 1)]
    [TestCase(_boardSize, _boardSize)]
    public void IsOnBoard_PointOutsideBoard_ReturnsFalse(int row, int column)
    {
        bool isOnBoard = _board.IsOnGrid(new(row, column));

        Assert.That(isOnBoard, Is.False);
    }

    [TestCase(0, 0)]
    [TestCase(_boardSize / 2, _boardSize / 2)]
    [TestCase(_boardSize - 1, _boardSize - 1)]
    public void IsPointEmpty_PointEmpty_ReturnsTrue(int row, int column)
    {
        Point point = new(row, column);

        bool isPointEmpty = _board.IsPointEmpty(point);

        Assert.That(isPointEmpty, Is.True);
    }

    [TestCase(Player.Black, 0, 0)]
    [TestCase(Player.White, 0, 0)]
    [TestCase(Player.Black, _boardSize / 2, _boardSize / 2)]
    [TestCase(Player.White, _boardSize / 2, _boardSize / 2)]
    [TestCase(Player.Black, _boardSize - 1, _boardSize - 1)]
    [TestCase(Player.White, _boardSize - 1, _boardSize - 1)]
    public void IsPointEmpty_PointTaken_ReturnsFalse(Player player, int row, int column)
    {
        Point point = new(row, column);

        _board.PlaceStone(player, point);

        bool isPointEmpty = _board.IsPointEmpty(point);

        Assert.That(isPointEmpty, Is.False);
    }

    [TestCase(0, 0)]
    [TestCase(_boardSize / 2, _boardSize / 2)]
    [TestCase(_boardSize - 1, _boardSize - 1)]
    public void IsPointTaken_PointEmpty_ReturnsFalse(int row, int column)
    {
        Point point = new(row, column);

        bool isPointTaken = _board.IsPointTaken(point);

        Assert.That(isPointTaken, Is.False);
    }

    [TestCase(Player.Black, 0, 0)]
    [TestCase(Player.White, 0, 0)]
    [TestCase(Player.Black, _boardSize / 2, _boardSize / 2)]
    [TestCase(Player.White, _boardSize / 2, _boardSize / 2)]
    [TestCase(Player.Black, _boardSize - 1, _boardSize - 1)]
    [TestCase(Player.White, _boardSize - 1, _boardSize - 1)]
    public void IsPointTaken_PointTaken_ReturnsTrue(Player player, int row, int column)
    {
        Point point = new(row, column);

        _board.PlaceStone(player, point);

        bool isPointTaken = _board.IsPointTaken(point);

        Assert.That(isPointTaken, Is.True);
    }

    /*
        e x .
        x . .
        . . .
    */
    [Test]
    public void IsPointAnEye_PointInCornerAllNeighborsAreFriendlyNoCorners_ReturnsFalse()
    {
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(0, 0));

        Assert.That(isPointAnEye, Is.False);
    }

    /*
        e x .
        x o .
        . . .
    */
    [Test]
    public void IsPointAnEye_PointInCornerAllNeighborsAreFriendlyHostileCorner_ReturnsFalse()
    {
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.White, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(0, 0));

        Assert.That(isPointAnEye, Is.False);
    }

    /*
        e x .
        x x .
        . . .
    */
    [Test]
    public void IsPointAnEye_PointInCornerAllNeighborsAndCornersAreFriendly_ReturnsTrue()
    {
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(0, 0));

        Assert.That(isPointAnEye, Is.True);
    }

    /*
        x e x
        . x . 
        . . .
    */
    [Test]
    public void IsPointAnEye_PointOnEdgeAllNeighborsAreFriendlyNoCorners_ReturnsFalse()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(0, 1));

        Assert.That(isPointAnEye, Is.True);
    }

    /*
        x e x
        x x o 
        . . .
    */
    [Test]
    public void IsPointAnEye_PointOnEdgeAllNeighborsAreFriendlyHostileCorner_ReturnsFalse()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.White, new(1, 2));
        _board.PlaceStone(Player.Black, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(0, 1));

        Assert.That(isPointAnEye, Is.False);
    }

    /*
        x e x
        x x x 
        . . .
    */
    [Test]
    public void IsPointAnEye_PointOnEdgeAllNeighborsAndCornersAreFriendly_ReturnsTrue()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(1, 1));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(0, 1));

        Assert.That(isPointAnEye, Is.True);
    }

    /*
        x x x
        x e x
        x . x
    */
    [Test]
    public void IsPointAnEye_PointInCenterAllCornersAreFriendlyMissingNeighbor_ReturnsFalse()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(1, 1));

        Assert.That(isPointAnEye, Is.False);
    }

    /*
        x x x
        x e x
        x o x
    */
    [Test]
    public void IsPointAnEye_PointInCenterAllCornersAreFriendlyHostileNeighbor_ReturnsFalse()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.White, new(2, 1));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(1, 1));

        Assert.That(isPointAnEye, Is.False);
    }

    /*
        x x x
        x e x
        x x .
    */
    [Test]
    public void IsPointAnEye_PointInCenterAllNeighborsAndMostCornersAreFriendly_ReturnsTrue()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(2, 1));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(1, 1));

        Assert.That(isPointAnEye, Is.True);
    }

    /*
        x x x
        x e x
        x x x
    */
    [Test]
    public void IsPointAnEye_PointInCenterAllNeighborsAndCornersAreFriendly_ReturnsTrue()
    {
        _board.PlaceStone(Player.Black, new(0, 0));
        _board.PlaceStone(Player.Black, new(0, 1));
        _board.PlaceStone(Player.Black, new(0, 2));
        _board.PlaceStone(Player.Black, new(1, 2));
        _board.PlaceStone(Player.Black, new(2, 2));
        _board.PlaceStone(Player.Black, new(2, 1));
        _board.PlaceStone(Player.Black, new(2, 0));
        _board.PlaceStone(Player.Black, new(1, 0));

        bool isPointAnEye = _board.IsPointAnEye(Player.Black, new(1, 1));

        Assert.That(isPointAnEye, Is.True);
    }

    [TestCase(2, Player.Black, 0, 0, $" 2 x  . \r\n 1 .  . \r\n   A  B")]
    [TestCase(3, Player.Black, 0, 0, $" 3 x  .  . \r\n 2 .  .  . \r\n 1 .  .  . \r\n   A  B  C")]
    [TestCase(3, Player.Black, 1, 1, $" 3 .  .  . \r\n 2 .  x  . \r\n 1 .  .  . \r\n   A  B  C")]
    [TestCase(3, Player.Black, 2, 1, $" 3 .  .  . \r\n 2 .  .  . \r\n 1 .  x  . \r\n   A  B  C")]
    [TestCase(3, Player.White, 0, 0, $" 3 o  .  . \r\n 2 .  .  . \r\n 1 .  .  . \r\n   A  B  C")]
    [TestCase(3, Player.White, 1, 1, $" 3 .  .  . \r\n 2 .  o  . \r\n 1 .  .  . \r\n   A  B  C")]
    [TestCase(3, Player.White, 2, 1, $" 3 .  .  . \r\n 2 .  .  . \r\n 1 .  o  . \r\n   A  B  C")]
    public void PrintBoard_ReturnsExpectedBoard(int boardSize, Player player, int row, int column, string expectedBoard)
    {
        Board board = new(boardSize);

        board.PlaceStone(player, new(row, column));

        string printedBoard = board.PrintBoard();

        Assert.That(printedBoard, Is.EqualTo(expectedBoard));
    }
}