using Domain.Go;

namespace Domain.Scoring;

public record GameResult(int Black, int White, float Komi)
{
    public Player Winner
    {
        get
        {
            if (Black == (White + Komi))
            {
                return Player.None;
            }

            return Black > White + Komi ? Player.Black : Player.White;
        }
    }

    public float WinningMargin => MathF.Abs(Black - (White + Komi));

    public override string ToString() => (Winner == Player.Black ? "B+" : "W+") + WinningMargin;
}
