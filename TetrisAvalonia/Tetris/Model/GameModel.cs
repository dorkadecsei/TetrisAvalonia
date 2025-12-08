using System;
using System.Threading;
using System.Threading.Tasks;
using Tetris.Persistence;

namespace Tetris.Model
{
    public class GameState
    {
        public required int[,] Field { get; set; }
        public Shape? CurrentShape { get; set; }
        public Position CurrentPosition { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public int LinesCleared { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime SaveTime { get; set; } = DateTime.Now;
    }

    public class GameModel
    {
        #region Fields

        private Table _table;
        private int _linesCleared;
        private readonly ITimer _timer;
        private bool _isPaused;
        private bool _gameStarted;
        private DateTime _gameStartTime;
        private TimeSpan _pausedTime;
        private DateTime _pauseStartTime;
        private GameState? _savedState;
        private TimeSpan _finalElapsedTime;
        private readonly IDataAccess _dataAccess;

        #endregion

        #region Properties

        public int Width => _table.Width;
        public int Height => _table.Height;
        public int[,] Field => _table.FieldValues;
        public Shape CurrentShape => _table.CurrentShape;
        public Position CurrentPosition => _table.CurrentPosition;
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

        public int LinesCleared => _linesCleared;

        #endregion

        #region Events

        public event EventHandler? GameUpdated;
        public event EventHandler? GameOver;
        public event EventHandler? LinesClearedChanged;
        public event EventHandler? GamePaused;
        public event EventHandler? GameResumed;

        #endregion

        #region Constructor

        public GameModel(ITimer timer, IDataAccess dataAccess, int width, int height)
        {
            _table = new Table(width, height);
            _timer = timer;
            _dataAccess = dataAccess;
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
            _table = new Table(width, height);

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
                if (_table.MoveLeft())
                    OnGameUpdated();
            }
        }

        public void MoveRight()
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                if (_table.MoveRight())
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
                        EndGame();
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
                    EndGame();
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

            _table = new Table(state.Width, state.Height);

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
                EndGame();
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
        public async Task SaveGameAsync(string path)
        {
            if (IsGameOver || !_gameStarted || _isPaused)
            {
                throw new InvalidOperationException("A játék mentése csak futó, vagy szüneteltetett állapotban lehetséges.");
            }

            var gameState = SaveGameState();
            await _dataAccess.SaveAsync(path, gameState);
        }

        public async Task LoadGameAsync(string path)
        {
            if (_gameStarted && !_isPaused && !IsGameOver)
            {
                PauseGame();
            }

            var gameState = await _dataAccess.LoadAsync(path);
            RestoreGameState(gameState);

            _table.ClearField();
        }

        #endregion

        #region Private methods

        private void EndGame()
        {
            _gameStarted = false;
            _timer.Stop();
            _finalElapsedTime = DateTime.Now - _gameStartTime - _pausedTime;
            OnGameOver();
        }

        private static Shape? CloneShape(Shape original)
        {
            if (original == null) return null;

            var cloned = original.Clone();
            return cloned;
        }

        private void OnTimerElapsed(object? sender, EventArgs e)
        {
            MoveDown();
        }

        private void OnGameUpdated() => GameUpdated?.Invoke(this, EventArgs.Empty);
        private void OnGameOver() => GameOver?.Invoke(this, EventArgs.Empty);
        private void OnLinesClearedChanged() => LinesClearedChanged?.Invoke(this, EventArgs.Empty);
        private void OnGamePaused() => GamePaused?.Invoke(this, EventArgs.Empty);
        private void OnGameResumed() => GameResumed?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}