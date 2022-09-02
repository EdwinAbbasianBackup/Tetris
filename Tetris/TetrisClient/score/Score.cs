namespace TetrisClient.score
{
    /// <summary>
    /// Score is the standard implementation of the IScore interface and is used to set the strategy pattern.
    /// </summary>
    public class Score
    {
        private readonly IScoreStrategyInterface _strategy;
        
        public Score(IScoreStrategyInterface strategy)
        {
            _strategy = strategy;
        }
        
        public int CalculateScore(int rows)
        {
            return _strategy.CalculateScore(rows);
        }
    }
}