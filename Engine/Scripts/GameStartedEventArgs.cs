using System;

namespace Engine.Scripts;

public class GameStartedEventArgs(int gridSize, GameType gameType, PlayerType blackPlayer, PlayerType whitePlayer) : EventArgs
{
    public int GridSize { get; } = gridSize;
    public GameType GameType { get; } = gameType;
    public PlayerType BlackPlayer { get; } = blackPlayer;
    public PlayerType WhitePlayer { get; } = whitePlayer;
}