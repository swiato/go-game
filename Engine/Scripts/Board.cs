using Godot;
using System.Collections.Generic;
using Domain.Agents;
using Domain.Common;
using Domain.Go;
using Infrastructure.Agents;

namespace Engine.Scripts;

public partial class Board : Control
{
    [Export(PropertyHint.Range, "2,19,1")]
    private int _gridSize { get; set; } = 19;

    [Export]
    private GameType _gameType;

    [Export]
    private PlayerType _blackPlayer;

    [Export]
    private PlayerType _whitePlayer;

    [Export]
    private Grid _grid;

    [Export]
    private Control _stonesContainer;

    [Export]
    private PackedScene _stoneScene = GD.Load<PackedScene>("res://Scenes/Stone.tscn");

    private IGameState _gameState;
    private readonly Dictionary<Point, Stone> _stones = [];

    private readonly Dictionary<Player, PlayerType> _players = [];

    private IAgent _agent;

    public override void _Ready()
    {
        GameEvents.Played += OnPlayed;
        GameEvents.GameStarted += OnGameStarted;
    }

    public override void _ExitTree()
    {
        GameEvents.Played -= OnPlayed;
        GameEvents.GameStarted -= OnGameStarted;
    }

    private void OnGameStarted(object sender, GameStartedEventArgs e)
    {
        StartNewGame(e.GridSize, e.GameType, e.BlackPlayer, e.WhitePlayer);
    }

    private void StartNewGame(int gridSize, GameType gameType, PlayerType blackPlayer, PlayerType whitePlayer)
    {
        _grid.GridSize = gridSize;
        IGameStateFactory factory = IGameStateFactory.CreateFactory(gameType.ToString());
        _gameState = factory.NewGame(gridSize);

        if (blackPlayer == PlayerType.Computer || whitePlayer == PlayerType.Computer)
        {
            _agent = AgentFactory.CreateTerminationAgent("current", gridSize);
        }

        _players[Player.Black] = blackPlayer;
        _players[Player.White] = whitePlayer;

        ChangePlayer();
    }

    private void OnPlayed(object sender, PlayedEventArgs e)
    {
        Result<IGameState> result = _gameState.Play(e.Move);

        if (result.IsFailure)
        {
            GameEvents.OnUserNotificationRequired(this, new(result.Error, NotificationType.Error));
            return;
        }

        _gameState = result.Value;

        AddLastMove();
        // PrintLastMove();
        CheckIfGameIsOver();
        ChangePlayer();
    }

    private void ChangePlayer()
    {
        if (_players[_gameState.NextPlayer] == PlayerType.Computer)
        {
            _grid.MouseFilter = MouseFilterEnum.Ignore;

            Move move = _agent.SelectMove(_gameState);
            GameEvents.OnPlayed(this, new(move));
        }
        else
        {
            _grid.MouseFilter = MouseFilterEnum.Stop;
        }
    }

    private void PrintLastMove()
    {
        GameEvents.OnUserNotificationRequired(this, new(_gameState.PrintLastMove(), NotificationType.Info));
    }

    private void AddLastMove()
    {
        if (!_gameState.LastMove.IsPlay)
        {
            return;
        }

        AddStone();
        RemoveStones();
    }

    private void AddStone()
    {
        Point point = _gameState.LastMove.Point;

        Stone stone = _stoneScene.Instantiate<Stone>();
        stone.Position = _grid.PointToPosition(point);
        stone.StoneSize = _grid.CellSize.X;
        stone.Color = _gameState.NextPlayer.Other() == Player.White ? Colors.White : Colors.Black;

        _stones[point] = stone;
        _stonesContainer.AddChild(stone);
    }

    private void RemoveStones()
    {
        foreach (Point point in _stones.Keys)
        {
            if (_gameState.Board.IsPointEmpty(point))
            {
                // flyweight return to pool
                _stones[point].QueueFree();
                _stones.Remove(point);
            }
        }
    }

    private void CheckIfGameIsOver()
    {
        if (_gameState.IsOver())
        {
            GameEvents.OnGameOver(this, new(_gameState.GetWinner(), _gameState.PrintGameResult()));
        }
    }
}