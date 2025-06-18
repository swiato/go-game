using Domain.Common;
using Domain.Go;
using NSubstitute;
using static Domain.Agents.MctsAgent;

namespace Domain.UnitTests.Agents
{
    public class MctsNodeTests
    {
        private IGameState _gameState;

        [SetUp]
        public void Setup()
        {
            _gameState = Substitute.For<IGameState>();
        }

        [Test]
        public void Expand_NoValidMoves_ThrowsException()
        {
            _gameState.GetValidMoves().Returns([]);
            MctsNode root = new(_gameState);

            Exception exception = Assert.Throws<IndexOutOfRangeException>(() => root.Expand());
            Assert.That(exception.Message, Is.EqualTo(ErrorMessages.NodeFullyExpanded));
        }

        [Test]
        public void Expand_ValidMoveAvailable_AddNewChild()
        {
            Move move = Move.Play(new(0, 0));
            IGameState nextState = Substitute.For<IGameState>();
            _gameState.GetValidMoves().Returns([move]);
            _gameState.ApplyMove(move).Returns(nextState);
            MctsNode root = new(_gameState);

            MctsNode child = root.Expand();

            Assert.Multiple(() =>
            {
                Assert.That(root.Children, Does.Contain(child));
                Assert.That(child.Parent, Is.EqualTo(root));
                Assert.That(child.Move, Is.EqualTo(move));
                Assert.That(child.GameState, Is.EqualTo(nextState));
            });
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void Expand_ValidMovesAvailable_AddsNewChildEachTime(int movesCount)
        {
            Move[] moves = [.. Enumerable.Range(0, movesCount).Select(index => Move.Play(new(index, index)))];

            IGameState nextState = Substitute.For<IGameState>();
            _gameState.GetValidMoves().Returns(moves);
            MctsNode root = new(_gameState);

            for (int i = 0; i < movesCount; i++)
            {
                root.Expand();
            }

            Assert.That(root.Children, Has.Count.EqualTo(movesCount));
        }

        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void BestChild_ChildrenWithRecordedWins_ReturnsChildWithBestWinRateForParentPlayer(Player player)
        {
            Player opponent = player.Other();

            _gameState.NextPlayer.Returns(player);
            MctsNode root = new(_gameState);
            MctsNode child1 = new(_gameState, root);
            MctsNode child2 = new(_gameState, root);
            MctsNode child3 = new(_gameState, root);

            root.Children.Add(child1);
            root.Children.Add(child2);
            root.Children.Add(child3);

            // win rate for parent player: 0
            child1.RecordWin(opponent);
            child1.RecordWin(opponent);

            // win rate for parent player: 0.5
            child2.RecordWin(player);
            child2.RecordWin(opponent);

            // win rate for parent player: 1
            child3.RecordWin(player);
            child3.RecordWin(player);

            MctsNode bestChild = root.BestChild();

            Assert.That(bestChild, Is.EqualTo(child3));
        }

        [TestCase(Player.Black)]
        [TestCase(Player.Black)]
        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        [TestCase(Player.White)]
        [TestCase(Player.White)]
        public void SelectChild_TemperatureIsZero_ReturnsChildWithBestWinRateForParentPlayer(Player player)
        {
            float temperature = 0f;

            Player opponent = player.Other();
            _gameState.NextPlayer.Returns(player);
            IGameState nextState = Substitute.For<IGameState>();

            MctsNode root = new(_gameState);
            
            MctsNode child1 = new(nextState, root);
            MctsNode child2 = new(nextState, root);
            MctsNode child3 = new(nextState, root);

            root.Children.Add(child1);
            root.Children.Add(child2);
            root.Children.Add(child3);

            // win rate for parent player: 0
            child1.RecordWin(opponent);
            child1.RecordWin(opponent);
            root.RecordWin(opponent);
            root.RecordWin(opponent);

            // win rate for parent player: 0.5
            child2.RecordWin(player);
            child2.RecordWin(opponent);
            root.RecordWin(player);
            root.RecordWin(opponent);

            // win rate for parent player: 1
            child3.RecordWin(player);
            child3.RecordWin(player);
            root.RecordWin(player);
            root.RecordWin(player);

            MctsNode selectedChild = root.SelectChild(temperature);

            Assert.That(selectedChild, Is.EqualTo(child3));
        }

        [TestCase(Player.Black, 0f, 0)]
        [TestCase(Player.Black, 0.5f, 0)]
        [TestCase(Player.Black, 1f, 1)]
        [TestCase(Player.Black, 1.5f, 1)]
        [TestCase(Player.White, 0f, 0)]
        [TestCase(Player.White, 0.5f, 0)]
        [TestCase(Player.White, 1f, 1)]
        [TestCase(Player.White, 1.5f, 1)]
        public void SelectChild_ChildrenWithRecordedWins_ReturnsChildWithBestUTCScoreForParentPlayer(Player player, float temperature, int expectedChildIndex)
        {
            Player opponent = player.Other();
            _gameState.NextPlayer.Returns(player);
            IGameState nextState = Substitute.For<IGameState>();

            MctsNode root = new(_gameState);
            
            MctsNode child1 = new(nextState, root);
            MctsNode child2 = new(nextState, root);

            root.Children.Add(child1);
            root.Children.Add(child2);

            // wins: 4, visits: 5, win rate: 0.8
            child1.RecordWin(player);
            child1.RecordWin(player);
            child1.RecordWin(player);
            child1.RecordWin(player);
            child1.RecordWin(opponent);

            // wins: 3, visits: 4, win rate: 0.75
            child2.RecordWin(player);
            child2.RecordWin(player);
            child2.RecordWin(player);
            child2.RecordWin(opponent);

            // backpropagate wins to parent
            root.RecordWin(player);
            root.RecordWin(player);
            root.RecordWin(player);
            root.RecordWin(player);
            root.RecordWin(opponent);
            root.RecordWin(player);
            root.RecordWin(player);
            root.RecordWin(player);
            root.RecordWin(opponent);

            MctsNode selectedChild = root.SelectChild(temperature);

            Assert.That(selectedChild, Is.EqualTo(root.Children[expectedChildIndex]));
        }
    }
}