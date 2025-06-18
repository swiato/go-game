namespace Domain.Go;

public static class PlayerExtensions
{
    private static readonly Dictionary<Player, char> _playerSymbols = new()
    {
        { Player.None,  '.' },
        { Player.Black, 'x' },
        { Player.White, 'o' }
    };

    public static Player Other(this Player player)
    {
        return player == Player.Black ? Player.White : Player.Black;
    }

    public static char Symbol(this Player player)
    {
        return _playerSymbols[player];
    }
}