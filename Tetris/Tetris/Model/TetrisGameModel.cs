using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Tetris.Persistence;

namespace Tetris.Model
{
    public class GameState
    {
        public required int[,] Field { get; set; }
        public TetrisShape? CurrentShape { get; set; }
        public Point CurrentPosition { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public int LinesCleared { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime SaveTime { get; set; } = DateTime.Now;

    }

    public class TetrisGameModel
    {
        #region Fields

        private TetrisTable _table;
        private int _linesCleared;
        private int _gameTime;
        private readonly Tetris.Persistence.ITimer _timer;
        private bool _isPaused;
        private bool _gameStarted;
        private DateTime _gameStartTime;
        private TimeSpan _pausedTime;
        private DateTime _pauseStartTime;
        private GameState? _savedState;
        private TimeSpan _finalElapsedTime;

        #endregion

        #region Properties

        public int Width => _table.Width;
        public int Height => _table.Height;
        public int[,] Field => _table.FieldValues;
        public TetrisShape CurrentShape => _table.CurrentShape;
        public Point CurrentPosition => _table.CurrentPosition;
        public bool IsGameOver => _table.IsGameOver();
        public bool IsPaused => _isPaused;
        public bool GameStarted => _gameStarted;
        public TimeSpan ElapsedTime
        {
            get
            {
                if (IsGameOver)
                    return _finalElapsedTime;

                if (!_gameStarted)
                    return TimeSpan.Zero;

                return DateTime.Now - _gameStartTime - _pausedTime;
            }
        }
        public TimeSpan FinalElapsedTime => _finalElapsedTime;

        public int GameTime
        {
            get { return _gameTime; }
            private set { _gameTime = value; }
        }

        public int LinesCleared
        {
            get { return _linesCleared; }
            private set { _linesCleared = value; }
        }

        public Tetris.Persistence.ITimer? Timer1 { get; }
        public object? Timer2 { get; }

        #endregion

        #region Events

        public event EventHandler? GameUpdated;
        public event EventHandler? GameOver;
        public event EventHandler? LinesClearedChanged;
        public event EventHandler? GamePaused;
        public event EventHandler? GameResumed;

        #endregion

        #region Constructor

        public TetrisGameModel(Tetris.Persistence.ITimer timer) : this(timer, 10, 20)
        {
        }

        public TetrisGameModel(Tetris.Persistence.ITimer timer, int width, int height)
        {
            _table = new TetrisTable(width, height);
            _timer = timer;
            _timer.Interval = 700;
            _timer.Elapsed += OnTimerElapsed;
            _linesCleared = 0;
            _gameStarted = false;
            _pausedTime = TimeSpan.Zero;
        }



        #endregion

        #region Public methods
        public void StartGame(int width, int height)
        {
            _table = new TetrisTable(width, height);

            _linesCleared = 0;
            _isPaused = false;
            _gameStarted = true;
            _gameStartTime = DateTime.Now;
            _pausedTime = TimeSpan.Zero;
            _finalElapsedTime = TimeSpan.Zero;

            _timer.Start();

            OnGameUpdated();
            OnLinesClearedChanged();
        }

        public void PauseGame()
        {
            if (!_isPaused && _gameStarted && !IsGameOver)
            {
                _timer.Stop();
                _isPaused = true;
                _pauseStartTime = DateTime.Now;

                _savedState = SaveGameState();

                OnGamePaused();
            }
        }

        public void ResumeGame()
        {
            if (_isPaused && _gameStarted && !IsGameOver)
            {
                if (_savedState != null)
                {
                    RestoreGameState(_savedState);
                }

                _timer.Start();
                _isPaused = false;
                _pausedTime += DateTime.Now - _pauseStartTime;

                OnGameResumed();
                OnGameUpdated();
            }
        }

        public void MoveLeft()
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                _table.MoveLeft();
                OnGameUpdated();
            }
        }

        public void MoveRight()
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                _table.MoveRight();
                OnGameUpdated();
            }
        }

        public void MoveDown()
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                if (!_table.MoveDown())
                {
                    int cleared = _table.ClearFullLines();
                    if (cleared > 0)
                    {
                        _linesCleared += cleared;
                        OnLinesClearedChanged();
                    }

                    if (_table.IsGameOver())
                    {
                        _gameStarted = false;
                        _timer.Stop();
                        _finalElapsedTime = DateTime.Now - _gameStartTime - _pausedTime;
                        OnGameOver();
                        return;
                    }
                }
                OnGameUpdated();
            }
        }

        public void Rotate()
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                _table.Rotate();
                OnGameUpdated();
            }
        }

        public void Drop()
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                _table.Drop();
                int cleared = _table.ClearFullLines();
                if (cleared > 0)
                {
                    _linesCleared += cleared;
                    OnLinesClearedChanged();
                }

                if (_table.IsGameOver())
                {
                    _gameStarted = false;
                    _timer.Stop();
                    _finalElapsedTime = DateTime.Now - _gameStartTime - _pausedTime;
                    OnGameOver();
                    return;
                }

                OnGameUpdated();
            }
        }

        public GameState SaveGameState()
        {
            return new GameState
            {
                Field = (int[,])_table.FieldValues.Clone(),
                CurrentShape = CloneShape(_table.CurrentShape),
                CurrentPosition = _table.CurrentPosition,
                ElapsedTime = ElapsedTime,
                LinesCleared = _linesCleared,
                Width = _table.Width,
                Height = _table.Height
            };
        }

        public void RestoreGameState(GameState state)
        {
            if (state == null) return;

            _table = new TetrisTable(state.Width, state.Height);

            if (state.Field != null)
            {
                Array.Copy(state.Field, _table.FieldValues, state.Field.Length);
            }

            if (state.CurrentShape != null)
            {
                _table.SetCurrentShapeAndPosition(state.CurrentShape, state.CurrentPosition);
            }

            _linesCleared = state.LinesCleared;

            if (_table.IsGameOver())
            {
                _gameStarted = false;
                _isPaused = false;
                _timer.Stop();
                if (state.ElapsedTime.HasValue) _finalElapsedTime = state.ElapsedTime.Value;
            }
            else
            {
                _gameStarted = true;
                _isPaused = false;

                if (state.ElapsedTime.HasValue)
                {
                    _gameStartTime = DateTime.Now - state.ElapsedTime.Value;
                    _pausedTime = TimeSpan.Zero;
                }

                _timer.Start();
            }

            OnLinesClearedChanged();

            OnGameUpdated();
        }
        #endregion

        #region Private methods

        private static TetrisShape? CloneShape(TetrisShape original)
        {
            if (original == null) return original;

            var shapeType = GetShapeType(original);
            var cloned = TetrisShape.ShapeFactory(shapeType);
            cloned.SetColor(original.Color);

            return cloned;
        }

        private static ShapeType GetShapeType(TetrisShape shape)
        {
            return shape switch
            {
                KShape => ShapeType.K,
                EShape => ShapeType.E,
                LShape => ShapeType.L,
                TShape => ShapeType.T,
                RShape => ShapeType.R,
                _ => ShapeType.K
            };
        }

        private static int[,] GetShapeMatrix(TetrisShape shape)
        {
            int size = shape.Size;
            int[,] matrix = new int[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    matrix[x, y] = shape[x, y];
                }
            }

            return matrix;
        }

        private void OnTimerElapsed(object? sender, EventArgs e)
        {
            MoveDown();
        }

        private void OnGameUpdated()
        {
            GameUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameOver()
        {
            _timer.Stop();
            _gameStarted = false;
            _finalElapsedTime = DateTime.Now - _gameStartTime - _pausedTime;
            GameOver?.Invoke(this, EventArgs.Empty);
        }

        private void OnLinesClearedChanged()
        {
            LinesClearedChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGamePaused()
        {
            GamePaused?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameResumed()
        {
            GameResumed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}