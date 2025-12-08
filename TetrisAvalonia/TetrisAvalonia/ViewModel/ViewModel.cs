using System;
using System.Collections.ObjectModel;
using System.Threading;
using Tetris.Model;
using TetrisAvalonia.ViewModel;
using System.Threading.Tasks;
using Tetris.Persistence;
using System.IO;


namespace TetrisAvalonia.ViewModel
{
    // A class nevét "TetrisViewModel"-re módosítottam a projektkonvenciókhoz való illeszkedés érdekében.
    public class ViewModel : ViewModelBase
    {
        #region Fields

        private readonly GameModel _model;

        #endregion

        #region Properties

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand MoveLeftCommand { get; private set; }
        public DelegateCommand MoveRightCommand { get; private set; }
        public DelegateCommand RotateCommand { get; private set; }
        public DelegateCommand MoveDownCommand { get; private set; }
        public DelegateCommand DropCommand { get; private set; }

        public ObservableCollection<Fields> Fields { get; set; }

        /// <summary>
        /// Eltüntetett sorok száma.
        /// </summary>
        public int LinesCleared
        {
            get { return _model.LinesCleared; }
        }

        /// <summary>
        /// Játékidő formázott kijelzése.
        /// </summary>
        public String GameTime
        {
            get { return _model.ElapsedTime.ToString("mm\\:ss"); }
        }

        public bool IsGamePaused
        {
            get { return _model.IsPaused; }
        }
        public bool IsGameOver
        {
            get { return _model.IsGameOver; }
        }

        public int TableWidth => _model.Width;
        public int TableHeight => _model.Height;

        #endregion

        #region Events

        public event EventHandler? NewGame;
        public event EventHandler<string?>? LoadGame;
        public event EventHandler<string?>? SaveGame;
        public event EventHandler? ExitGame;
        public event EventHandler? PauseGame;

        #endregion

        #region Constructors

        public ViewModel(GameModel model)
        {
            _model = model;
            _model.GameUpdated += Model_GameUpdated;
            _model.GameOver += Model_GameOver;
            _model.LinesClearedChanged += Model_LinesClearedChanged;
            _model.GamePaused += Model_GamePaused;
            _model.GameResumed += Model_GameResumed;

            // Parancsok
            NewGameCommand = new DelegateCommand(param => OnNewGame(param));
            // A LoadGameCommand és SaveGameCommand eseményeket küld a nézetnek a fájl kiválasztásának kérésére
            LoadGameCommand = new DelegateCommand(_ => OnLoadGame());
            SaveGameCommand = new DelegateCommand(_ => OnSaveGame());
            ExitCommand = new DelegateCommand(_ => OnExitGame());
            PauseCommand = new DelegateCommand(_ => OnPauseGame());

            // Irányítás
            MoveLeftCommand = new DelegateCommand(_ => _model.MoveLeft());
            MoveRightCommand = new DelegateCommand(_ => _model.MoveRight());
            RotateCommand = new DelegateCommand(_ => _model.Rotate());
            MoveDownCommand = new DelegateCommand(_ => _model.MoveDown());
            DropCommand = new DelegateCommand(_ => _model.Drop());

            Fields = new ObservableCollection<Fields>();
            InitializeTable();
        }

        #endregion

        #region Private Methods

        private void InitializeTable()
        {
            Fields.Clear();
            for (int y = 0; y < _model.Height; y++)
            {
                for (int x = 0; x < _model.Width; x++)
                {
                    Fields.Add(new Fields(x, y));
                }
            }
        }

        private void RefreshTable()
        {
            if (Fields.Count != _model.Width * _model.Height)
            {
                InitializeTable();
            }

            for (int y = 0; y < _model.Height; y++)
            {
                for (int x = 0; x < _model.Width; x++)
                {
                    int index = y * _model.Width + x;
                    if (index < Fields.Count)
                    {
                        Fields[index].Color = _model.Field[x, y];
                    }
                }
            }

            var shape = _model.CurrentShape;
            var position = _model.CurrentPosition;

            if (shape != null)
            {
                for (int sizeY = 0; sizeY < shape.Size; sizeY++)
                {
                    for (int sizeX = 0; sizeX < shape.Size; sizeX++)
                    {
                        if (shape[sizeX, sizeY] != 0)
                        {
                            int boardX = position.X + sizeX;
                            int boardY = position.Y + sizeY;

                            if (boardX >= 0 && boardX < _model.Width &&
                                boardY >= 0 && boardY < _model.Height)
                            {
                                int index = boardY * _model.Width + boardX;
                                if (index >= 0 && index < Fields.Count)
                                {
                                    Fields[index].Color = shape.Color;
                                }
                            }
                        }
                    }
                }
            }

            OnPropertyChanged(nameof(GameTime));
            OnPropertyChanged(nameof(LinesCleared));
        }
        private void OnNewGame(object? param)
        {
            int width = 10;
            int height = 20;

            if (param is string sizeStr)
            {
                string[] parts = sizeStr.Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                {
                    width = w;
                    height = h;
                }
                else
                {
                    width = _model.Width;
                    height = _model.Height;
                }
            }
            else
            {
                width = _model.Width;
                height = _model.Height;
            }

            _model.GameUpdated -= Model_GameUpdated;

            _model.StartGame(width, height);

            OnPropertyChanged(nameof(TableWidth));
            OnPropertyChanged(nameof(TableHeight));

            InitializeTable();

            _model.GameUpdated += Model_GameUpdated;

            RefreshTable();
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(GameTime));
            NewGame?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Game Event Handlers

        private void Model_GameUpdated(object? sender, EventArgs e)
        {
            RefreshTable();
        }

        private void Model_LinesClearedChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(LinesCleared));
        }

        private void Model_GameOver(object? sender, EventArgs e)
        {
            RefreshTable();
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(GameTime));
        }

        private void Model_GamePaused(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsGamePaused));
        }

        private void Model_GameResumed(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsGamePaused));
        }

        #endregion

        #region Event Methods

        private void OnLoadGame()
        {
            // Esemény kiváltása: a nézet feladata a fájl kiválasztása.
            LoadGame?.Invoke(this, null); // <-- HIBA JAVÍTVA: A LoadGame eseményt váltja ki
        }

        public async Task LoadGameView(string path)
        {
            try
            {
                await _model.LoadGameAsync(path);

                OnPropertyChanged(nameof(TableWidth));
                OnPropertyChanged(nameof(TableHeight));

                if (Fields.Count != _model.Width * _model.Height)
                {
                    InitializeTable();
                }
                RefreshTable();

                OnPropertyChanged(nameof(IsGamePaused));
                OnPropertyChanged(nameof(IsGameOver));
                OnPropertyChanged(nameof(LinesCleared));
                OnPropertyChanged(nameof(GameTime));
            }
            catch (Exception ex) when (ex is DataException || ex is IOException || ex is InvalidOperationException)
            {
                throw;
            }
        }

        private void OnSaveGame()
        {
            // Esemény kiváltása: a nézet feladata a mentési útvonal kiválasztása.
            SaveGame?.Invoke(this, null); // <-- HIBA JAVÍTVA: A SaveGame eseményt váltja ki
        }

        public async Task SaveGameView(string path)
        {
            try
            {
                await _model.SaveGameAsync(path);
            }
            catch (Exception ex) when (ex is DataException || ex is IOException || ex is InvalidOperationException)
            {
                throw;
            }
        }

        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnPauseGame()
        {
            if (_model.IsPaused)
                _model.ResumeGame();
            else
                _model.PauseGame();

            PauseGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}