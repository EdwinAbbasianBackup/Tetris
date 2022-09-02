using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TetrisClient.rectangle;
using TetrisClient.score;

namespace TetrisClient
{
    internal class TetrisEngine
    {
        private static readonly Matrix TetrisLShape = new (new[,] { { 0, 0, 1 }, { 1, 1, 1 }, { 0, 0, 0 } });
        private static readonly Matrix TetrisJShape = new (new[,] { { 1, 0, 0 }, { 1, 1, 1 }, { 0, 0, 0 } });
        private static readonly Matrix TetrisZShape = new (new[,] { { 1, 1, 0 }, { 0, 1, 1 }, { 0, 0, 0 } });
        private static readonly Matrix TetrisSShape = new (new[,] { { 0, 1, 1 }, { 1, 1, 0 }, { 0, 0, 0 } });
        private static readonly Matrix TetrisTShape = new (new[,] { { 1, 1, 1 }, { 0, 1, 0 }, { 0, 0, 0 } });
        private static readonly Matrix TetrisOShape = new (new[,] { { 1, 1, 0 }, { 1, 1, 0 }, { 0, 0, 0 } });
        private static readonly Matrix TetrisIShape = new (new[,] { { 1, 1, 1 }, { 0, 0, 0 }, { 0, 0, 0 } });
        private readonly Matrix[] _matrixes = {
            TetrisLShape, TetrisJShape, TetrisZShape, TetrisSShape, TetrisTShape, TetrisOShape, TetrisIShape
        };
        private readonly List<CustomRectangle> _matrixRectangles = new();
        private readonly List<CustomRectangle> _rectanglesToDraw = new();
        private CustomRectangle[,] _tetrisBoard; 
        private SolidColorBrush NextColor { get; set; }
        private SolidColorBrush CurrentColor { get; set; }
        private Random _random;
        private int _offsetX = 3;
        private int _offsetY; 
        public Matrix CurrentMatrix { get; set; }
        public Matrix NextMatrix { get; set; }
        public Score Score { get; private set; }
        public int DeletedRows { private set; get;} 
        public bool Collision { get; set; } 
        public GameState GameState { get; set; }

        private static CustomRectangle[,] GenerateBoard()
        {
            return new CustomRectangle[16, 10];
        }
        
        /// <summary>
        /// this function is used to generate a random matrix
        /// which will be used to create a new tetris piece (Rectangle) and the color of the piece 
        /// </summary>
        private Matrix GetRandomMatrix(int? seed = null)
        {
            if (seed != null) _random = new Random((int) seed);

            Random random = _random ?? new Random();
            var randomIndex = random.Next(1, _matrixes.Length + 1);

            switch (randomIndex)
            {
                case 1:
                    NextColor = Brushes.Orange;
                    return TetrisLShape;
                case 2:
                    NextColor = Brushes.Blue;
                    return TetrisJShape;
                case 3:
                    NextColor = Brushes.Red;
                    return TetrisZShape;
                case 4:
                    NextColor = Brushes.LimeGreen;
                    return TetrisSShape;
                case 5:
                    NextColor = Brushes.Purple;
                    return TetrisTShape;
                case 6:
                    NextColor = Brushes.Yellow;
                    return TetrisOShape;
                case 7:
                    NextColor = Brushes.Aqua;
                    return TetrisIShape;
                default:
                    NextColor = Brushes.Yellow;
                    return TetrisOShape;
            }
        }
        
        /// <summary>
        /// This function sets the current matrix to the next matrix and generates
        /// a new matrix if the next matrix is the same as the current matrix
        /// </summary>
        private Matrix GetAndSetNextMatrix()
        {
            CurrentMatrix = NextMatrix;
            CurrentColor = NextColor;
            do
            {
                NextMatrix = GetRandomMatrix();
            }
            while (NextMatrix.Equals(CurrentMatrix));
            
            return CurrentMatrix;
        }
        
        /// <summary>
        /// This function is used for the initial setup of the game,
        /// it checks the index of the given matrix for the frontend to use in its picture array that has the same index as the matrix
        /// </summary>
        public int GetMatrixIndex(Matrix matrix)
        {
            return _matrixes.ToList().FindIndex(x => x.Equals(matrix));
        }
        
        /// <summary>
        /// Initializes the game and is used to create the board and the first tetris pieces,
        /// it also sets the game score strategy to the given score strategy 
        /// </summary>
        public void InitializeGame(int? seed = null)
        {
            if (seed != null) _random = new Random((int) seed);
            GameState = GameState.Playing;
            CurrentMatrix = GetRandomMatrix();
            CurrentColor = NextColor;
            NextMatrix = GetRandomMatrix();
            Score = new Score(new ScoreStrategyBasic());
            _tetrisBoard = GenerateBoard();
            
        }
        

                
        /// <summary>
        /// this function is used to check if the current matrix can be placed on the board and if it can it places it on the board
        /// it takes a given offsetX and offsetY to place the matrix on the board and is updated every time the matrix is moved
        /// The first forloop is to get the parameters of the given matrix and being checked if it can be placed on the board
        /// if it collides it will be set on the screen in a form of a rectangle with the color of the current matrix and shape of its matrix
        /// </summary>
        public void CheckNextPosition(int offY, int offX, Matrix newMatrixPosition)
        {

            int newOffsetY = _offsetY + offY;
            int newOffsetX = _offsetX + offX;
            Collision = false;
            int blocks = 0;
            List<int> heights = new List<int>();
            List<int> widths = new List<int>();
            bool dontMove = false;

            int[,] values = newMatrixPosition.Value;

            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (values[i, j] != 1) continue;
                    widths.Add(j);
                    heights.Add(i);
                    blocks += 1;

                    if (IsNotInBoard(newOffsetX + j, newOffsetY + i))
                    {
                        newOffsetX = _offsetX;
                        dontMove = true;
                    }
                    
                    else if (IsBoardOccupied(_offsetY + i, newOffsetX + j))
                    {
                        dontMove = true;
                    }
                    else if (newOffsetY + i == 16 || GetTetrisBoard()[newOffsetY + i, _offsetX + j] != null)
                    {
                        Collision = true;
                    }
                }
            }
        
            if (dontMove)
            {
                return;
            }
            
            _rectanglesToDraw.Clear();
            _matrixRectangles.Clear();
            
            
            for (int i = 0; i < blocks; i++)
            {
                _matrixRectangles.Add(CreateRectangle(_offsetY + heights[i], _offsetX + widths[i]));
            }
            
            if (Collision)
            {

                Collision = true;
                for (int i = 0; i < blocks; i++)
                {
                    AddToTetrisBoard(_offsetY + heights[i], _offsetX + widths[i], CurrentColor);
                    _rectanglesToDraw.Add(CreateRectangle(_offsetY + heights[i], _offsetX + widths[i]));
                }
                
                CurrentMatrix = GetAndSetNextMatrix();
                
                _offsetY = 0;
                _offsetX = 3;
            }
            else
            {
                CurrentMatrix = newMatrixPosition;

                _offsetY += offY;
                _offsetX += offX;
                for (int i = 0; i < blocks; i++)
                {
                    _rectanglesToDraw.Add(CreateRectangle(_offsetY + heights[i], _offsetX + widths[i]));
                }
            }
        }
        
        
        /// <summary>
        /// this function removes the given row from the board
        /// and moves all the rows above it down one row based on the given row
        /// </summary>
        private void RemoveRows(int fromYPosition)
        {
            List<CustomRectangle> toAdd = new List<CustomRectangle>();
            foreach (CustomRectangle rectangle in GetTetrisBoard())
            {
                if (rectangle != null)
                {
                    toAdd.Add(rectangle);
                }
            }
            
            Array.Clear(GetTetrisBoard(), 0, GetTetrisBoard().Length);
            foreach (CustomRectangle rect in toAdd)
            {
                if (rect.Y < fromYPosition)
                {
                    rect.Y += 1;
                }

                AddToTetrisBoard(rect.Y, rect.X, rect.Color);
                _rectanglesToDraw.Add(CreateRectangle(rect.Y, rect.X, rect.Color));
            }

        }
        
        /// <summary>
        /// this function checks if the row is full based on the length of the dimensions in the tetris board
        /// Deleted rows are being counted and used to calculate the score of the game
        /// The rows are also deleted in the backend with RemoveFromTetrisBoard()
        /// </summary>
        public bool RowFull()
        {
            DeletedRows = 0;
            List<int> fullRows = new List<int>();
            bool fullRow = false;
            for (int row = 0; row < GetTetrisBoard().GetLength(0); row++)
            {
                int count = 0;
                for (int column = 0; column < GetTetrisBoard().GetLength(1); column++)
                {
                    if (GetTetrisBoard()[row, column] != null)
                    {
                        count += 1;
                    }
                    if (count == 10)
                    {
                        DeletedRows++;
                        foreach (int k in Enumerable.Range(0, 10))
                        {
                            RemoveFromTetrisBoard(row, k);
                        }
                        
                        fullRows.Add(row);
                        fullRow = true;
                    }
                }
            }
            fullRows.ForEach(x => RemoveRows(x));
            
            
            return fullRow;
        }

        /// <summary>
        /// Checks the collision of the x and y position
        /// </summary>
        private bool IsNotInBoard(int x, int y)
        {
            if(y < 0) {
                GameState = GameState.GameOver;
            }
            return x is < 0 or >= 10 || y > 16;
        }
        // checks if the board is occupied with the a shape
        private bool IsBoardOccupied(int y, int x)
        {
            return _tetrisBoard[y, x] != null;
        }

        /// <summary>
        /// Creates the rectangles for the tetris board with a given color
        /// </summary>
        private CustomRectangle CreateRectangle(int y, int x, SolidColorBrush color = null)
        {
            if (color == null)
            {
                color = CurrentColor;
            }
            CustomRectangle rectangle = new CustomRectangle(color, y, x);
 
            return rectangle;
        }
        
        public List<CustomRectangle> GetRectanglesToDraw()
        {
            return _rectanglesToDraw;
        }

        public CustomRectangle[,] GetTetrisBoard()
        {
            return _tetrisBoard;
        }

        private void AddToTetrisBoard(int i, int j, SolidColorBrush color)
        {
            _tetrisBoard[i, j] = new CustomRectangle(color, i, j);
        }

        /// <summary>
        /// this function removes i, j from the tetris board
        /// I is the row and j is the column
        /// </summary>
        private void RemoveFromTetrisBoard(int i, int j)
        {
            _tetrisBoard[i, j] = null;
        }

    }
}