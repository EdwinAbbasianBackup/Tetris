namespace TetrisClient.score
{
    /// <summary>
    /// this is the class that handles the score of the game with the basics of deleting a row and adding a point
    /// </summary>
    public class ScoreStrategyBasic : IScoreStrategyInterface
    {
        private int _score = 0;
        
        public int CalculateScore(int amountOfRows)
        {
            switch (amountOfRows)
                {
                    case 1:
                        _score = 100;
                        break;
                    case 2:
                        _score = 300;
                        break;
                    case 3:
                        _score = 500;
                        break;
                    case 4:
                        _score = 800;
                        break;
                    case 5:
                        _score = 10000;
                        break;
                }
                return _score;
        }

    }
}