using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading;
using Microsoft.VisualBasic;

namespace TetrisGame
{
    public class Grid
    {
        private int[,] grid;
        public int Rows {get;}
        public int Columns {get;}

        public int this [int x, int y]
        {
            get { return grid[x, y]; }
            set { grid[x, y] = value; }
        }

        // Constructor
        public Grid(int rowsCount, int columnsCount)
        {
            Rows = rowsCount;
            Columns = columnsCount;
            grid = new int[rowsCount, columnsCount];
        }

        // Check if the cell of given coordinates is inside the grid
        public bool isInside(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Columns;
        }

        // Check if the cell is empty
        public bool isEmpty(int r, int c)
        {
            return isInside(r, c) && grid[r, c] == 0;
        }

        // Check if the row is full
        public bool isRowFull(int row)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[row, c] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        // Check if the row is empty
        public bool isRowEmpty(int row)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[row, c] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        // Clear the row
        private void clearRow(int row)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[row, c] = 0;
            }
        }

        // Move the row down
        private void moveRowDown(int rowTo, int rowFrom)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[rowTo, c] = grid[rowFrom, c];
            }
        }

        public int eraseFullRows()
        {
            int Count = 0;
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (isRowFull(r))
                {
                    clearRow(r);
                    Count += 1;
                }
            }
            return Count;
        }

        public void pullRows()
        {
            int Count = 0;
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (isRowEmpty(r))
                {
                    Count += 1;
                }
                else if (Count > 0)
                {
                    moveRowDown(r + Count, r);
                    clearRow(r);
                }
            }
        }

        // Clear the full rows
        public int clearFullRows()
        {
            int scoreIncrease = eraseFullRows();
            pullRows();
            return scoreIncrease;
        }
    }

    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Position(int r, int c)
        {
            Row = r;
            Column = c;
        }
    }

    public abstract class Piece
    {
        protected abstract Position[][] Tiles { get; }
        protected abstract Position SpavnOffset { get; }
        public abstract int Type { get; }

        private int rotationState;
        private Position offset;

        public Piece()
        {
            offset = new Position(SpavnOffset.Row, SpavnOffset.Column);
        }

        // Generates an enumerable collection of tile positions based on the current rotation state and offset.
        // returns An IEnumerable of Position objects representing the tile positions.
        public IEnumerable<Position> TilePositions()
        {
            foreach (Position p in Tiles[rotationState])
            {
                yield return new Position(p.Row + offset.Row, p.Column + offset.Column);
            }
        }

        // Rotate the piece clockwise
        public void RotateCW()
        {
            rotationState = (rotationState + 1) % Tiles.Length;
        }

        // Rotate the piece counter-clockwise
        public void RotateCCW()
        {
            rotationState = (rotationState + Tiles.Length - 1) % Tiles.Length;
        }

        public void Move(int row, int column)
        {
            offset.Row += row;
            offset.Column += column;
        }

        public void Reset()
        {
            offset.Row = SpavnOffset.Row;
            offset.Column = SpavnOffset.Column;
            rotationState = 0;
        }
    }

    public class IPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(1, 0), new Position(1, 1), new Position(1, 2), new Position(1, 3) },
            new Position[] { new Position(0, 2), new Position(1, 2), new Position(2, 2), new Position(3, 2) },
            new Position[] { new Position(2, 0), new Position(2, 1), new Position(2, 2), new Position(2, 3) },
            new Position[] { new Position(0, 1), new Position(1, 1), new Position(2, 1), new Position(3, 1) }
        };

        public override int Type => 1;
        protected override Position SpavnOffset => new Position(-1, 3);
        protected override Position[][] Tiles => tiles;
    }


    
    public class JPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(0, 0), new Position(1, 0), new Position(1, 1), new Position(1, 2) },
            new Position[] { new Position(0, 1), new Position(0, 2), new Position(1, 1), new Position(2, 1) },
            new Position[] { new Position(1, 0), new Position(1, 1), new Position(1, 2), new Position(2, 2) },
            new Position[] { new Position(0, 1), new Position(1, 1), new Position(2, 0), new Position(2, 1) }
        };

        public override int Type => 2;
        protected override Position SpavnOffset => new Position(0, 3);
        protected override Position[][] Tiles => tiles;
    }

    public class LPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(1, 0), new Position(1, 1), new Position(1, 2), new Position(0, 2) },
            new Position[] { new Position(0, 1), new Position(1, 1), new Position(2, 1), new Position(2, 2) },
            new Position[] { new Position(2, 0), new Position(1, 0), new Position(1, 1), new Position(1, 2) },
            new Position[] { new Position(0, 0), new Position(0, 1), new Position(1, 1), new Position(2, 1) }
        };

        public override int Type => 3;
        protected override Position SpavnOffset => new Position(0, 3);
        protected override Position[][] Tiles => tiles;
    }

    public class OPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(0, 0), new Position(0, 1), new Position(1, 0), new Position(1, 1) }
        };

        public override int Type => 4;
        protected override Position SpavnOffset => new Position(0, 4);
        protected override Position[][] Tiles => tiles;
    }

    public class SPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(1, 0), new Position(1, 1), new Position(0, 1), new Position(0, 2) },
            new Position[] { new Position(0, 1), new Position(1, 1), new Position(1, 2), new Position(2, 2) },
            new Position[] { new Position(2, 0), new Position(2, 1), new Position(1, 1), new Position(1, 2) },
            new Position[] { new Position(0, 0), new Position(1, 0), new Position(1, 1), new Position(2, 1) }
        };

        public override int Type => 5;
        protected override Position SpavnOffset => new Position(0, 3);
        protected override Position[][] Tiles => tiles;
    }

    public class TPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(0, 1), new Position(1, 0), new Position(1, 1), new Position(1, 2) },
            new Position[] { new Position(0, 1), new Position(1, 1), new Position(1, 2), new Position(2, 1) },
            new Position[] { new Position(1, 0), new Position(1, 1), new Position(1, 2), new Position(2, 1) },
            new Position[] { new Position(0, 1), new Position(1, 0), new Position(1, 1), new Position(2, 1) }
        };

        public override int Type => 6;
        protected override Position SpavnOffset => new Position(0, 3);
        protected override Position[][] Tiles => tiles;
    }

    public class ZPiece : Piece
    {
        protected readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new Position(0, 0), new Position(0, 1), new Position(1, 1), new Position(1, 2) },
            new Position[] { new Position(0, 2), new Position(1, 1), new Position(1, 2), new Position(2, 1) },
            new Position[] { new Position(1, 0), new Position(1, 1), new Position(2, 1), new Position(2, 2) },
            new Position[] { new Position(0, 1), new Position(1, 0), new Position(1, 1), new Position(2, 0) }
        };

        public override int Type => 7;
        protected override Position SpavnOffset => new Position(0, 3);
        protected override Position[][] Tiles => tiles;
    }

    public class PieceQueue
    {
        private readonly Piece[] pieces = new Piece[]
        {
            new IPiece(),
            new JPiece(),
            new LPiece(),
            new OPiece(),
            new SPiece(),
            new TPiece(),
            new ZPiece()
        };

        private readonly Random random = new Random();
        public Piece[] NextPieces =  new Piece[2];

        private void SetUpQueue()
        {
            for (int i = 0; i < NextPieces.Length; i++)
            {
                NextPieces[i] = RandomPiece();
            }
        }
        
        public PieceQueue()
        {
            SetUpQueue();
        }
        private Piece RandomPiece()
        {
            return pieces[random.Next(pieces.Length)];
        }

        private void UpdateQueue()
        {
            for (int i = 0; i < NextPieces.Length - 1; i++)
            {
                NextPieces[i] = NextPieces[i + 1];
            }
            NextPieces[NextPieces.Length - 1] = RandomPiece();
        }

        public Piece GetAndUpdate()
        {
            var piece = NextPieces[0];
            UpdateQueue();
            return piece;
        }
    }

    public class GameState
    {
        private Piece currentPiece;
        public Piece CurrentPiece
        {
            get => currentPiece;
            set
            {
                currentPiece = value;
                currentPiece.Reset();
            }
        }

        public Grid GameGrid { get; }
        public PieceQueue PieceQueue { get; }
        public bool GameOver { get; set; }
        public bool GameStarted { get; set; }

        public int Score { get; set; }
        public int LinesCleared { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public GameState()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            GameGrid = new Grid(22, 10);
            PieceQueue = new PieceQueue();
            CurrentPiece = PieceQueue.GetAndUpdate();
            Score = 0;
            LinesCleared = 0;
        }

        private bool PieceFits()
        {
            foreach (Position p in CurrentPiece.TilePositions())
            {
                if (!GameGrid.isInside(p.Row, p.Column))
                {
                    return false;
                }
                if (!GameGrid.isEmpty(p.Row, p.Column))
                {
                    return false;
                }
            }
            return true;
        }

        public void RotatePieceCW()
        {
            CurrentPiece.RotateCW();
            if (!PieceFits())
            {
                CurrentPiece.RotateCCW();
            }
        }

        public void RotatePieceCCW()
        {
            CurrentPiece.RotateCCW();
            if (!PieceFits())
            {
                CurrentPiece.RotateCW();
            }
        }

        public void MovePieceLeft()
        {
            CurrentPiece.Move(0, -1);
            if (!PieceFits())
            {
                CurrentPiece.Move(0, 1);
            }
        }

        public void MovePieceRight()
        {
            CurrentPiece.Move(0, 1);
            if (!PieceFits())
            {
                CurrentPiece.Move(0, -1);
            }
        }

        private bool isGameOver()
        {
            return !(GameGrid.isRowEmpty(0) && GameGrid.isRowEmpty(1));
        }

        private void PlacePiece()
        {
            foreach (Position p in CurrentPiece.TilePositions())
            {
                GameGrid[p.Row, p.Column] = CurrentPiece.Type;
            }
            int rowClear = GameGrid.clearFullRows();
            if (isGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentPiece = PieceQueue.GetAndUpdate();
            }
            updateScore(rowClear);
        }

        public void MovePieceDown()
        {
            CurrentPiece.Move(1, 0);
            if (!PieceFits())
            {
                CurrentPiece.Move(-1, 0);
                PlacePiece();
            }
        }

        private void updateScore(int rowsCleared)
        {
            LinesCleared += rowsCleared;
            int Level = LinesCleared / 10;
            switch (rowsCleared)
            {
                case 1:
                    Score += (40 * (Level + 1));
                    break;
                case 2:
                    Score += (100 * (Level + 1));
                    break;
                case 3:
                    Score += (300 * (Level + 1));
                    break;
                case 4:
                    Score += (1200 * (Level + 1));
                    break;
            }
        }

        public void restartGame()
        {
            for (int r = 0; r < GameGrid.Rows; r++)
            {
                for (int c = 0; c < GameGrid.Columns; c++)
                {
                    GameGrid[r, c] = 0;
                }
            }
            Score = 0;
            LinesCleared = 0;
        }
    }

    public partial class MainWindow : Window
    {
        private const int CanvasWidth = 600;
        private const int GridWidth = 400;
        private const int CellSize = GridWidth / 10;
        private Bitmap currentBitmap;
        GameState gameState = new GameState();
        private DispatcherTimer gameTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.InvalidateVisual();
            var border = this.FindControl<Border>("VerticalDivider");
            if (border != null)
            {
                border.IsVisible = true;
            }

            currentBitmap = LoadSquareBitmap();
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            gameTimer.Tick += (sender, e) => GameTick();
            StartGame();
        }

        private Bitmap? LoadSquareBitmap()
        {
            var path = "pieces/square.png";
            return File.Exists(path) ? new Bitmap(path) : null;
        }

        private void StartGame()
        {
            gameTimer.Start();
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            this.KeyDown += OnKeyDown;
#pragma warning restore CS8622
        }

        private void GameTick()
        {   

            int Level = gameState.LinesCleared / 10;
            if (Level <= 10) {
                gameTimer.Interval = TimeSpan.FromMilliseconds(500 - (Level * 45));
            }
            if (gameState.GameStarted && !gameState.GameOver) {
                gameState.MovePieceDown();
            }
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            DrawGame(context);
            if (gameState.GameStarted && !gameState.GameOver) {
                Console.WriteLine("Frames rendered");
            }
        }

        private void DrawGame(DrawingContext context)
        {
            var renderer = new GameRenderer(gameState, currentBitmap, CellSize, CanvasWidth, gameState.Score);
            if (!gameState.GameStarted) {
                renderer.DrawStartMenu(context);
            }
            else if (!gameState.GameOver) {
                renderer.DrawGame(context);
            }
            else {
                renderer.DrawGameOver(context);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!gameState.GameStarted || gameState.GameOver) {
                gameState.GameStarted = true;
                gameState.GameOver = false;
                gameState.restartGame();
            }
            switch (e.Key)
            {
                case Key.Left:
                    gameState.MovePieceLeft();
                    break;

                case Key.Right:
                    gameState.MovePieceRight();
                    break;

                case Key.Down:
                    gameState.MovePieceDown();
                    break;

                case Key.Up:
                    gameState.RotatePieceCW();
                    break;
                default:
                    return; // Do nothing if the key is not recognized (Optimization)
            }

            InvalidateVisual();
        }
    }

    internal class GameRenderer
    {
        private GameState gameState;
        private Bitmap currentBitmap;
        private int cellSize;
        private int canvasWidth;
        private int score;

        public GameRenderer(GameState gameState, Bitmap currentBitmap, int cellSize, int canvasWidth, int score)
        {
            this.gameState = gameState;
            this.currentBitmap = currentBitmap;
            this.cellSize = cellSize;
            this.canvasWidth = canvasWidth;
            this.score = score;
        }

        internal void DrawGame(DrawingContext context)
        {
            // Draw the grid lines
            var gridPen = new Pen(Brushes.Gray, 1); // Define the grid line color and thickness
            for (int row = 0; row <= gameState.GameGrid.Rows; row++)
            {
                var y = row * cellSize;
                context.DrawLine(gridPen, new Point(0, y), new Point(gameState.GameGrid.Columns * cellSize, y));
            }
            for (int col = 0; col <= gameState.GameGrid.Columns; col++)
            {
                var x = col * cellSize;
                context.DrawLine(gridPen, new Point(x, 0), new Point(x, gameState.GameGrid.Rows * cellSize));
            }
        
            // Draw the game grid
            for (int row = 0; row < gameState.GameGrid.Rows; row++)
            {
                for (int col = 0; col < gameState.GameGrid.Columns; col++)
                {
                    DrawCell(context, row, col, gameState.GameGrid[row, col]);
                }
            }
        
            // Draw the current piece
            foreach (var position in gameState.CurrentPiece.TilePositions())
            {
                DrawCell(context, position.Row, position.Column, gameState.CurrentPiece.Type);
            }
        
            // Draw score
            var scoreText = new FormattedText("Score: " + score, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.White);
            double textWidth = scoreText.Width;
            double xPosition = canvasWidth - textWidth - 10; // 10 pixels padding from the right edge
            context.DrawText(scoreText, new Point(xPosition, 10)); // 10 pixels padding from the top edge
        }

        internal void DrawStartMenu(DrawingContext context)
        {
            var startText = new FormattedText("Press any key to start", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.White);
            double textWidth = startText.Width;
            double textHeight = startText.Height;
            double xPosition = (canvasWidth - textWidth) / 2;
            double yPosition = (canvasWidth - textHeight) / 2;
            context.DrawText(startText, new Point(xPosition, yPosition));
        }

        internal void DrawGameOver(DrawingContext context)
        {
            var gameOverText = new FormattedText("Game Over", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 48, Brushes.White);
            var restartText = new FormattedText("Press any key to start", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 20, Brushes.White);
            
            double gameOverTextWidth = gameOverText.Width;
            double gameOverTextHeight = gameOverText.Height;
            double gameOverTextXPosition = (canvasWidth - gameOverTextWidth) / 2;
            double gameOverTextYPosition = (canvasWidth - gameOverTextHeight) / 2;

            double restartTextWidth = restartText.Width;
            double restartTextHeight = restartText.Height;
            double restartTextXPosition = (canvasWidth - restartTextWidth) / 2;
            double restartTextYPosition = (canvasWidth - restartTextHeight) / 2 + 50;

            context.DrawText(gameOverText, new Point(gameOverTextXPosition, gameOverTextYPosition));
            context.DrawText(restartText, new Point(restartTextXPosition, restartTextYPosition));
        }
        
        private void DrawCell(DrawingContext context, int row, int col, int cellValue)
        {
            if (cellValue > 0)
            {
                var rect = new Rect(col * cellSize, (row - 2) * cellSize, cellSize, cellSize);
                var brush = new SolidColorBrush(GetColorFromCellType(cellValue));
                var borderBrush = Brushes.Black; // Define the border color
                var borderThickness = 1; // Define the border thickness
        
                // Fill the cell with the piece color
                context.FillRectangle(brush, rect);
        
                // Draw the border around the cell
                context.DrawRectangle(new Pen(borderBrush, borderThickness), rect);
            }
        }

        private Color GetColorFromCellType(int cellType)
        {
            return cellType switch
            {
                1 => Colors.Cyan,
                2 => Colors.Blue,
                3 => Colors.Orange,
                4 => Colors.Yellow,
                5 => Colors.Green,
                6 => Colors.Purple,
                7 => Colors.Red,
                _ => Colors.Black,
            };
        }
    }

}

