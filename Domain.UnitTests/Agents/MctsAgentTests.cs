using Domain.Agents;
using Domain.Go;
using NSubstitute;
using static Domain.Agents.MctsAgent;

namespace Domain.UnitTests.Agents
{
    public class MctsAgentTests
    {
        private IGameState _gameState;
        private IAgent _policyAgent;
        private MctsAgent _agent;

        [SetUp]
        public void Setup()
        {
            _gameState = Substitute.For<IGameState>();
            _policyAgent = Substitute.For<IAgent>();
            _agent = new(10, 1.4f, _policyAgent);
        }

        [TearDown]
        public void TearDown()
        {
            _policyAgent.Dispose();
            _agent.Dispose();
        }

        [Test]
        public void SelectNode_GameIsOver_ReturnsNode()
        {
            // arrange
            _gameState.IsOver().Returns(true);

            MctsNode root = new(_gameState);

            // act
            MctsNode selectedNode = _agent.SelectNode(root);

            // assert
            Assert.That(selectedNode, Is.EqualTo(root));
        }

        [Test]
        public void SelectNode_GameIsOnAndNodeHasUnvisitedMoves_CreatesChildNodeForUnvisitedMove()
        {
            // arrange
            Move move = Move.Play(new(0, 0));
            IGameState nextState = Substitute.For<IGameState>();

            _gameState.IsOver().Returns(false);
            _gameState.GetValidMoves().Returns([move]);
            _gameState.ApplyMove(move).Returns(nextState);

            MctsNode root = new(_gameState);

            // act
            MctsNode childNode = _agent.SelectNode(root);

            // assert
            Assert.Multiple(() =>
            {
                Assert.That(childNode.Parent, Is.EqualTo(root));
                Assert.That(childNode.Move, Is.EqualTo(move));
                Assert.That(childNode.GameState, Is.EqualTo(nextState));
            });
        }

        [Test]
        public void SelectNode_GameIsOnButNodeHasNoUnvisitedMoves_CreatesGrandchildForUnvisitedMove()
        {
            // arrange
            IGameState nextState = Substitute.For<IGameState>();
            nextState.IsOver().Returns(false);
            nextState.GetValidMoves().Returns([Move.Play(new(0, 1))]);
            nextState.NextPlayer.Returns(Player.White);

            Move move = Move.Play(new(0, 0));
            _gameState.IsOver().Returns(false);
            _gameState.GetValidMoves().Returns([move]);
            _gameState.ApplyMove(move).Returns(nextState);
            _gameState.NextPlayer.Returns(Player.Black);

            MctsNode root = new(_gameState);
            // expand only child
            _agent.SelectNode(root);

            // act
            MctsNode grandChild = _agent.SelectNode(root);

            // assert
            Assert.Multiple(() =>
            {
                Assert.That(grandChild.Parent?.Parent, Is.EqualTo(root));
                Assert.That(grandChild.Parent?.GameState, Is.EqualTo(nextState));
            });
        }

        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        [TestCase(Player.None)]
        public void SimulateGame_GameIsOver_ReturnsWinner(Player expectedWinner)
        {
            _gameState.IsOver().Returns(true);
            _gameState.GetWinner().Returns(expectedWinner);

            Player winner = _agent.SimulateGame(_gameState);

            Assert.That(winner, Is.EqualTo(expectedWinner));
        }

        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        [TestCase(Player.None)]
        public void SimulateGame_GameIsOn_PlaysGameUntilItsOver(Player expectedWinner)
        {
            // arrange
            IGameState finalState = Substitute.For<IGameState>();
            finalState.IsOver().Returns(true);
            finalState.GetWinner().Returns(expectedWinner);

            Move move = Move.Play(new(0, 0));
            _policyAgent.SelectMove(_gameState).Returns(move);
            _gameState.IsOver().Returns(false);
            _gameState.ApplyMove(move).Returns(finalState);

            // act
            Player winner = _agent.SimulateGame(_gameState);

            // assert
            Assert.That(winner, Is.EqualTo(expectedWinner));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void Backpropagate_GrandChildVisited_PropagatesVisitsToRoot(int visits)
        {
            MctsNode root = new(_gameState);
            MctsNode child = new(_gameState, root);
            MctsNode grandChild = new(_gameState, child);

            for (int i = 0; i < visits; i++)
            {
                _agent.Backpropagate(grandChild, Player.Black);
            }

            Assert.Multiple(() =>
            {
                Assert.That(root.Visits, Is.EqualTo(visits));
                Assert.That(child.Visits, Is.EqualTo(visits));
                Assert.That(grandChild.Visits, Is.EqualTo(visits));
            });
        }

        [TestCase(Player.Black, 1)]
        [TestCase(Player.Black, 2)]
        [TestCase(Player.White, 1)]
        [TestCase(Player.White, 2)]
        public void Backpropagate_GrandChildVisited_PropagatesWinnersToRoot(Player winner, int visits)
        {
            Player loser = winner.Other();

            MctsNode root = new(_gameState);
            MctsNode child = new(_gameState, root);
            MctsNode grandChild = new(_gameState, child);

            for (int i = 0; i < visits; i++)
            {
                _agent.Backpropagate(grandChild, winner);
            }

            Assert.Multiple(() =>
            {
                Assert.That(root.GetWins(winner), Is.EqualTo(visits));
                Assert.That(child.GetWins(winner), Is.EqualTo(visits));
                Assert.That(grandChild.GetWins(winner), Is.EqualTo(visits));

                Assert.That(root.GetWins(loser), Is.Zero);
                Assert.That(child.GetWins(loser), Is.Zero);
                Assert.That(grandChild.GetWins(loser), Is.Zero);
            });
        }

        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void Backpropagate_ChildVisited_DoesNotPropagateDataToGrandChild(Player winner)
        {
            MctsNode root = new(_gameState);
            MctsNode child = new(_gameState, root);
            MctsNode grandChild = new(_gameState, child);

            _agent.Backpropagate(child, winner);

            Assert.Multiple(() =>
            {
                Assert.That(root.Visits, Is.EqualTo(1));
                Assert.That(child.Visits, Is.EqualTo(1));
                Assert.That(grandChild.Visits, Is.EqualTo(0));

                Assert.That(root.GetWins(winner), Is.EqualTo(1));
                Assert.That(child.GetWins(winner), Is.EqualTo(1));
                Assert.That(grandChild.GetWins(winner), Is.EqualTo(0));
            });
        }

        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void Backpropagate_ManyChildrenSameWinner_RootCumulatesWinnersFromEachChild(Player winner)
        {
            MctsNode root = new(_gameState);
            MctsNode child1 = new(_gameState, root);
            MctsNode child2 = new(_gameState, root);
            MctsNode child3 = new(_gameState, root);

            _agent.Backpropagate(child1, winner);
            _agent.Backpropagate(child2, winner);
            _agent.Backpropagate(child3, winner);

            Assert.Multiple(() =>
            {
                Assert.That(child1.Visits, Is.EqualTo(1));
                Assert.That(child2.Visits, Is.EqualTo(1));
                Assert.That(child3.Visits, Is.EqualTo(1));
                Assert.That(child1.GetWins(winner), Is.EqualTo(1));
                Assert.That(child2.GetWins(winner), Is.EqualTo(1));
                Assert.That(child3.GetWins(winner), Is.EqualTo(1));

                Assert.That(root.Visits, Is.EqualTo(3));
                Assert.That(root.GetWins(winner), Is.EqualTo(3));
            });
        }

        [TestCase(Player.Black)]
        [TestCase(Player.White)]
        public void Backpropagate_ManyChildrenDifferentWinners_RootCumulatesWinnersFromEachChild(Player player)
        {
            Player opponent = player.Other();

            MctsNode root = new(_gameState);
            MctsNode child1 = new(_gameState, root);
            MctsNode child2 = new(_gameState, root);

            _agent.Backpropagate(child1, player);
            _agent.Backpropagate(child2, opponent);

            Assert.Multiple(() =>
            {
                Assert.That(child1.Visits, Is.EqualTo(1));
                Assert.That(child2.Visits, Is.EqualTo(1));
                Assert.That(child1.GetWins(player), Is.EqualTo(1));
                Assert.That(child1.GetWins(opponent), Is.EqualTo(0));
                Assert.That(child2.GetWins(player), Is.EqualTo(0));
                Assert.That(child2.GetWins(opponent), Is.EqualTo(1));

                Assert.That(root.Visits, Is.EqualTo(2));
                Assert.That(root.GetWins(player), Is.EqualTo(1));
                Assert.That(root.GetWins(opponent), Is.EqualTo(1));
            });
        }
    }
}