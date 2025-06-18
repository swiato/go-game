using System;
using Domain.Go;

namespace Engine.Scripts;

public class PlayedEventArgs(Move move) : EventArgs
{
    public Move Move { get; } = move;
}