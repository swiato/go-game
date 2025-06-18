using Domain.Go;

namespace Engine.Scripts;

public class GameOverEventArgs(Player winner, string result)
{
    public Player Winner { get; } = winner;
    public string Result { get; } = result;
}