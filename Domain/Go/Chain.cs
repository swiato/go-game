using System.Collections.Immutable;
using Domain.Common;
using Domain.Extensions;

namespace Domain.Go;

public class Chain(Player player, ImmutableHashSet<Point> stones, ImmutableHashSet<Point> liberties) : IEquatable<Chain>
{
    public Chain(Chain chain) : this(chain.Player, chain.Stones, chain.Liberties)
    {

    }

    public Player Player => player;
    public ImmutableHashSet<Point> Stones => stones;
    public ImmutableHashSet<Point> Liberties => liberties;

    public Chain AddLiberty(Point liberty)
    {
        ImmutableHashSet<Point> liberties = Liberties.Add(liberty);
        return new Chain(Player, Stones, liberties);
    }

    public Chain RemoveLiberty(Point liberty)
    {
        ImmutableHashSet<Point> liberties = Liberties.Remove(liberty);
        return new Chain(Player, Stones, liberties);
    }

    public Chain Merge(Chain chain)
    {
        if (Player != chain.Player)
        {
            throw new ArgumentException(ErrorMessages.CantMergeOppositePlayerChains);
        }

        ImmutableHashSet<Point> combinedStones = [.. Stones, .. chain.Stones];
        ImmutableHashSet<Point> combinedLiberties = [.. Liberties, .. chain.Liberties];
        combinedLiberties = combinedLiberties.Except(combinedStones);

        return new Chain(Player, combinedStones, combinedLiberties);
    }

    public bool Equals(Chain? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Player == other.Player && Stones.SetEquals(other.Stones) && Liberties.SetEquals(other.Liberties);
    }

    public override bool Equals(object? obj) => base.Equals(obj as Chain);

    public override int GetHashCode()
    {
        return HashCode.Combine(Player.GetHashCode(), Stones.GetCombinedHashCode(), Liberties.GetCombinedHashCode());
    }

    public static bool operator ==(Chain first, Chain second)
    {
        if (first is null)
        {
            if (second is null)
            {
                return true;
            }

            return false;
        }

        return first.Equals(second);
    }

    public static bool operator !=(Chain first, Chain second) => !(first == second);
}
