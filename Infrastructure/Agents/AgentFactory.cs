using Domain.Agents;
using Domain.Encoders;

namespace Infrastructure.Agents;

public static class AgentFactory
{
    public static TerminationAgent CreateTerminationAgent(string name, int boardSize, ITerminationStrategy? terminationStrategy = null)
    {
        IAgent agent = CreateAgent(name, boardSize);
        return new TerminationAgent(agent, terminationStrategy ?? new PassWhenOpponentPasses());
    }

    public static IAgent CreateAgent(string name, int boardSize)
    {
        name = name.ToLower();

        switch (name)
        {
            case "random":
                return new RandomAgent();
            case "atari":
                return new AtariGoAgent();
            case "mcts":
                return new MctsAgent(5000, 4f, rolloutAgent: new AtariGoAgent());
            case "heavymcts":
                return new MctsAgent(1000, 5, rolloutAgent: new DeepLearningAgent(new SevenPlaneEncoder(boardSize)));
            case "randomdeepmcts":
                return new DeepMctsAgent(new DeepLearningAgent(new SevenPlaneEncoder(boardSize)), null, new AtariGoAgent(), 1f);
            case "deepmcts":
                SevenPlaneEncoder encoder = new(boardSize);
                DeepLearningAgent policyNetwork = new(encoder);
                ValueAgent valueNetwork = new(encoder);
                return new DeepMctsAgent(policyNetwork, valueNetwork, null, 0f);
            case "current":
                return new DeepLearningAgent(new SevenPlaneEncoder(boardSize));
            default:
                return new DeepLearningAgent(new SevenPlaneEncoder(boardSize), name);
        }
    }
}
