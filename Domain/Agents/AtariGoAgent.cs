using Domain.Go;

namespace Domain.Agents;

public class AtariGoAgent : IAgent
{
    public Move SelectMove(IGameState gameState)
    {
        Board board = gameState.Board;

        Chain? opponentChain = null;
        Chain? playerChain = null;

        // if you can win with one move - just do it!
        foreach (Chain chain in board.Chains)
        {
            if (chain.Player != gameState.NextPlayer)
            {
                if (chain.Liberties.Count == 1)
                {
                    Point liberty = chain.Liberties.Single();

                    IGameState nextState = gameState.ApplyMove(Move.Play(liberty));

                    if (nextState.GetWinner() == gameState.NextPlayer)
                    {
                        return Move.Play(liberty);
                    }
                }

                if (opponentChain is null || opponentChain.Liberties.Count > chain.Liberties.Count)
                {
                    opponentChain = chain;
                }
            }
        }

        // if you can lose with one move - prevent it!
        foreach (Chain chain in board.Chains)
        {
            if (chain.Player == gameState.NextPlayer)
            {
                if (chain.Liberties.Count == 1)
                {
                    Board nextBoard = new(board);
                    Point liberty = chain.Liberties.First();

                    nextBoard.PlaceStone(gameState.NextPlayer, liberty);

                    if (nextBoard.GetChain(liberty) is not null)
                    {
                        return Move.Play(liberty);
                    }
                }

                if (playerChain is null || playerChain.Liberties.Count > chain.Liberties.Count)
                {
                    playerChain = chain;
                }
            }
        }

        if (opponentChain is not null)
        {
            foreach (Point liberty in opponentChain.Liberties)
            {
                return Move.Play(liberty);
            }
        }

        if (playerChain is not null)
        {
            foreach (Point liberty in playerChain.Liberties)
            {
                return Move.Play(liberty);
            }
        }

        return Move.Resign();
    }

    public void Dispose()
    {

    }

    private bool IsCapture(IGameState gameState, Point point)
    {
        Point[] neighbors = gameState.Board.GetNeighbors(point);

        for (int i = 0; i < neighbors.Length; i++)
        {
            Point neighbor = neighbors[i];

            Chain? chain = gameState.Board.GetChain(neighbor);

            if (chain is null || chain.Player == gameState.NextPlayer)
            {
                continue;
            }

            if (chain.Liberties.Count == 1)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAtari(IGameState gameState, Point point)
    {
        Point[] neighbors = gameState.Board.GetNeighbors(point);

        for (int i = 0; i < neighbors.Length; i++)
        {
            Point neighbor = neighbors[i];

            Chain? chain = gameState.Board.GetChain(neighbor);

            if (chain is null || chain.Player == gameState.NextPlayer)
            {
                continue;
            }

            if (chain.Liberties.Count == 2 && chain.Liberties.Contains(point))
            {
                return true;
            }
        }

        return false;
    }

    /*
    private bool IsStretch()
    {
        return false;
    }

    private bool IsHane(IGameState gameState, Point point)
    {
        Point[] neighbors = gameState.Board.GetNeighbors(point);

        for (int i = 0; i < neighbors.Length; i++)
        {
            Point neighbor = neighbors[i];

            gameState.
        }
    }

    private bool IsLadder(IGameState gameState, Chain chain, out Point ladderPoint)
    {
        if (chain.Liberties.Count == 2)
        {
            foreach (Point liberty in chain.Liberties)
            {
                Chain? libertyChain = gameState.Board.GetChain(liberty);

                if (libertyChain is null)
                {
                    continue;
                }


            }
        }
    }
    */

    // atari, double atari, tripple atari (exclusive) - played stone edge opponents chain has only one liberty - count each for double and triple atari (but previously had more?)
    // self atari - if played chain has one liberty and consists of only one stone or other stones from previous board had more liberties
    // suicide - after play the group doesn't exist anymore
    // geta (net) - corner (or edge) of played stone is opponent chain with two or three liberties & extending at neither liberty will increase chain liberties nor decrease them
    // ladder - (if is atari) if edge is opponent chain with one liberty and after playing this liberty has only two liberties (not three) - use turn no stretch
    // ladder continues - previously was ladder and now is also ladder
    // ladder breaker - previously was ladder but not anymore 
    // Oiotoshi (consecutive ataris) - opponent in atari when opponent plays his last liberty his group is still under atari (has one liberty) - but not captured
    // seki - if group has two liberties and playing either of them will end up in one liberty (self atari) and if this liberty played by the opponent would also put him into self atari
    // push to edge - similar to ladder but by stretch not turn
    // push to owns stones
    // Crane's Nest Tesuji - needs sucrifice

}
