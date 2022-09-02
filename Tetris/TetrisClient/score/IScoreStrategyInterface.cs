namespace TetrisClient.score
{
    
    public interface IScoreStrategyInterface
    {
        int CalculateScore(int amountOfRows);
    }
}