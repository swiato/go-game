namespace Domain.Go;

public interface IGameStateFactory
{
    IGameState NewGame(int boardSize);

    public static IGameStateFactory CreateFactory(string game)
    {
        return game.ToLower() switch
        {
            "go" => new GoStateFactory(),
            "atarigo" => new AtariGoStateFactory(),
            _ => throw new NotImplementedException()
        };
    }
}
