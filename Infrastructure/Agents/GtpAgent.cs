using System.Diagnostics;
using Domain.Agents;
using Domain.Go;

namespace Infrastructure.Agents;

public partial class GtpAgent : IAgent
{
    private readonly Process _gtpProcess;
    private readonly int _boardSize;
    private bool _disposed = false;

    public GtpAgent(string name, int boardSize)
    {
        _boardSize = boardSize;
        _gtpProcess = StartGtpProcess(name);

        InitializeBoard();
    }

    public Move SelectMove(IGameState gameState)
    {
        HandleOpponentLastMove(gameState);
        return GenerateMove(gameState.NextPlayer);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            QuitGtpProcess();
            _gtpProcess.WaitForExit();
            _gtpProcess.Dispose();
        }

        _disposed = true;
    }

    private static Process StartGtpProcess(string name)
    {
        ProcessStartInfo startInfo = GetStartInfo(name);
        return Process.Start(startInfo)!;
    }

    private static ProcessStartInfo GetStartInfo(string name)
    {
        return name switch
        {
            // --quiet - don't log anything
            // --level 10 - strenght (max 10)
            // --monte-carlo --mc-games-per-level 500 - 500 MCTS rollouts per move
            "gnugo" => new()
            {
                FileName = name,
                Arguments = "--mode gtp --quiet --level 10",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            // -d 0 - don't log anything
            // -t 2 - two seconds per move
            // -t =500 - 500 MCTS rollouts per move (min 500)
            "pachi" => new()
            {
                FileName = name,
                Arguments = "-d 0 -t =1000",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = "C:/Program Files/Pachi-12.84/"
            },
            _ => throw new NotImplementedException(),
        };
    }

    private void HandleOpponentLastMove(IGameState gameState)
    {
        if (gameState.LastMove is null)
        {
            ClearBoard();
            return;
        }

        if (gameState.PreviousLastMove is null)
        {
            ClearBoard();
        }

        PlayMove(gameState.LastMove, gameState.NextPlayer.Other());
    }

    private void InitializeBoard()
    {
        SendCommand($"boardsize {_boardSize}");
    }

    private void ClearBoard()
    {
        SendCommand($"clear_board");
    }

    private void PlayMove(Move move, Player player)
    {
        SendCommand($"play {player} {move.ToA1Coordinates(_boardSize)}");
    }

    private Move GenerateMove(Player player)
    {
        string move = SendCommand($"genmove {player}");
        return Move.FromA1Coordinates(move, _boardSize);
    }

    private void QuitGtpProcess()
    {
        SendCommand("quit");
    }

    private string SendCommand(string command)
    {
        _gtpProcess.StandardInput.WriteLine(command);

        StreamReader reader = _gtpProcess.StandardOutput;

        string? output;

        while ((output = reader.ReadLine()) != null)
        {
            if (output.StartsWith('='))
            {
                return output.Replace("=", string.Empty).Trim();
            }
        }

        throw new InvalidOperationException(output);
    }
}
