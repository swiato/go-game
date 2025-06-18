using Domain.Common;
using Domain.Go;

namespace Domain.UnitTests.Go
{
    public class ChainTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Merge_OppositePlayerChain_ThrowsArgumentException()
        {
            Player player = Player.Black;

            Chain firstChain = new(player, [], []);
            Chain secondChain = new(player.Other(), [], []);

            ArgumentException exception = Assert.Throws<ArgumentException>(() => firstChain.Merge(secondChain));
            Assert.That(exception.Message, Is.EqualTo(ErrorMessages.CantMergeOppositePlayerChains));
        }

        /*
            x ^ .   . ^ .    x ^ .
            x ^ .   ^ x ^    x x ^    
            ^ . .   . ^ .    ^ ^ .
        */
        [Test]
        public void Merge_SamePlayerChain_MergesStonesAndLiberties()
        {
            Chain firstChain = new(Player.Black, stones: [new(0, 0), new(1, 0)], liberties: [new(0, 1), new(1, 1), new(2, 0)]);
            Chain secondChain = new(Player.Black, stones: [new(1, 1)], liberties: [new(0, 1), new(1, 2), new(2, 1), new(1, 0)]);

            Point[] expectedStones = [new(0, 0), new(1, 0), new(1, 1)];
            Point[] expectedLiberties = [new(0, 1), new(1, 2), new(2, 1), new(2, 0)];

            Chain mergedChain = firstChain.Merge(secondChain);

            Assert.Multiple(() =>
            {
                Assert.That(mergedChain.Player, Is.EqualTo(Player.Black));
                Assert.That(mergedChain.Stones, Is.EquivalentTo(expectedStones));
                Assert.That(mergedChain.Liberties, Is.EquivalentTo(expectedLiberties));
            });
        }
    }
}