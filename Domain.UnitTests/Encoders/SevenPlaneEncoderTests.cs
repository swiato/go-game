using Domain.Go;
using Domain.Encoders;

namespace Domain.UnitTests.Encoders
{
    public class SevenPlaneEncoderTests
    {
        private const int _boardSize = 5;
        private readonly IEncoder _encoder = new SevenPlaneEncoder(_boardSize);
        private readonly GoStateFactory _gameStateFactory = new();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Encode_EmptyBoard_ReturnsEmptyTensor()
        {
            IGameState gameState = _gameStateFactory.NewGame(_boardSize);

            int[,,] encodedGameState = _encoder.Encode(gameState);

            Assert.That(encodedGameState, Has.All.Zero);
        }

        /*
            x ^ .
            o . .
            . . .
        */
        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void Encode_PlayerStoneWithOneLiberty_ReturnsTensorWithPlayerStoneEncoded(Player player)
        {
            Board board = new(_boardSize);
            board.PlaceStone(player, new(0, 0));
            board.PlaceStone(player.Other(), new(1, 0));

            GoState gameState = new(board, player);

            int[,,] encodedGameState = _encoder.Encode(gameState);

            Assert.That(encodedGameState[0, 0, 0], Is.EqualTo(1));
        }

        /*
            o ^ .
            x . .
            . . .
        */
        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void Encode_OpponentStoneWithOneLiberty_ReturnsTensorWithOpponentStoneEncoded(Player player)
        {
            Board board = new(_boardSize);
            board.PlaceStone(player.Other(), new(0, 0));
            board.PlaceStone(player, new(1, 0));

            GoState gameState = new(board, player);

            int[,,] encodedGameState = _encoder.Encode(gameState);

            Assert.That(encodedGameState[0, 0, 3], Is.EqualTo(1));
        }

        /*
            ^ x ^
            . ^ .
            . . .
        */
        [TestCase(Player.Black, 0, 0, 1)]
        [TestCase(Player.Black, 0, 2, 2)]
        [TestCase(Player.Black, 2, 1, 2)]
        [TestCase(Player.White, 0, 0, 1)]
        [TestCase(Player.White, 0, 2, 2)]
        [TestCase(Player.White, 2, 1, 2)]
        public void Encode_PlayerStoneWithManyLiberties_ReturnsTensorWithPlayerStoneEncoded(Player player, int row, int column, int plane)
        {
            Board board = new(_boardSize);
            board.PlaceStone(player, new(row, column));
            GoState gameState = new(board, player);

            int[,,] encodedGameState = _encoder.Encode(gameState);

            Assert.That(encodedGameState[row, column, plane], Is.EqualTo(1));
        }

        /*
            ^ o ^
            . ^ .
            . . .
        */
        [TestCase(Player.Black, 0, 0, 4)]
        [TestCase(Player.Black, 0, 2, 5)]
        [TestCase(Player.Black, 2, 1, 5)]
        [TestCase(Player.White, 0, 0, 4)]
        [TestCase(Player.White, 0, 2, 5)]
        [TestCase(Player.White, 2, 1, 5)]
        public void Encode_OpponentStoneWithManyLiberties_ReturnsTensorWithOpponentStoneEncoded(Player player, int row, int column, int plane)
        {
            Board board = new(_boardSize);
            board.PlaceStone(player, new(row, column));
            GoState gameState = new(board, player.Other());

            int[,,] encodedGameState = _encoder.Encode(gameState);

            Assert.That(encodedGameState[row, column, plane], Is.EqualTo(1));
        }

        /*
            x . x   . o x
            o x .   o x .
            . . .   . . .
        */
        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void Encode_MoveViolateKo_ReturnsTensorWithKoMoveEncoded(Player player)
        {
            Player opponent = player.Other();
            Board board = new(_boardSize);
            board.PlaceStone(player, new(0, 0));
            board.PlaceStone(player, new(1, 1));
            board.PlaceStone(player, new(0, 2));
            board.PlaceStone(opponent, new(1, 0));            

            IGameState gameState = new GoState(board, opponent);

            // opponent captures stone at 0-0, so player can't play there next turn
            gameState = gameState.ApplyMove(Move.Play(new(0, 1))); 

            int[,,] encodedGameState = _encoder.Encode(gameState);

            Assert.That(encodedGameState[0, 0, 6], Is.EqualTo(1));
        }

        [TestCase(2, 1, 11)]
        [TestCase(1, 2, 7)]
        [TestCase(2, 2, 12)]
        [TestCase(0, 4, 4)]
        [TestCase(4, 0, 20)]
        [TestCase(4, 4, 24)]
        public void EncodePoint_EncodedPointIndexSameAsExpected(int row, int column, int expectedEncodedPoint)
        {
            Point point = new(row, column);

            int encodedPoint = _encoder.EncodePoint(point);

            Assert.That(encodedPoint, Is.EqualTo(expectedEncodedPoint));
        }

        [TestCase(11, 2, 1)]
        [TestCase(7, 1, 2)]
        [TestCase(12, 2, 2)]
        [TestCase(4, 0, 4)]
        [TestCase(20, 4, 0)]
        [TestCase(24, 4, 4)]
        public void DecodePointIndex_DecodedPointSameAsExpected(int encodedPointIndex, int row, int column)
        {
            Point expectedPoint = new(row, column);

            Point decodedPoint = _encoder.DecodePointIndex(encodedPointIndex);

            Assert.That(decodedPoint, Is.EqualTo(expectedPoint));
        }
    }
}