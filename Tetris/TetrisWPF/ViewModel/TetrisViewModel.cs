using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using TetrisWPF.ViewModel;
using Tetris.Model;


namespace TetrisWPF.ViewModel
{
    public class TetrisViewModel : ViewModelBase
    {
        #region Fields

        private readonly TetrisGameModel _model;

        #endregion

        #region Properties

        /// <summary>
        /// Új játék parancs.
        /// </summary>
        public DelegateCommand NewGameCommand { get; private set; }

        /// <summary>
        /// Játék betöltése parancs.
        /// </summary>
        public DelegateCommand LoadGameCommand { get; private set; }

        /// <summary>
        /// Játék mentése parancs.
        /// </summary>
        public DelegateCommand SaveGameCommand { get; private set; }

        /// <summary>
        /// Kilépés parancs.
        /// </summary>
        public DelegateCommand ExitCommand { get; private set; }

        /// <summary>
        /// Szünet parancs.
        /// </summary>
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand MoveLeftCommand { get; private set; }
        public DelegateCommand MoveRightCommand { get; private set; }
        public DelegateCommand RotateCommand { get; private set; }
        public DelegateCommand MoveDownCommand { get; private set; }
        public DelegateCommand DropCommand { get; private set; }

        /// <summary>
        /// Játékmező gyűjtemény.
        /// </summary>
        public ObservableCollection<TetrisField> Fields { get; set; }

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
        public event EventHandler? LoadGame;
        public event EventHandler? SaveGame;
        public event EventHandler? ExitGame;
        public event EventHandler? PauseGame;

        #endregion

        #region Constructors

        public TetrisViewModel(TetrisGameModel model)
        {
            _model = model;
            _model.GameUpdated += Model_GameUpdated;
            _model.GameOver += Model_GameOver;
            _model.LinesClearedChanged += Model_LinesClearedChanged;
            _model.GamePaused += Model_GamePaused;
            _model.GameResumed += Model_GameResumed;

            // Parancsok inicializálása
            NewGameCommand = new DelegateCommand(param => OnNewGame(param));
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());
            PauseCommand = new DelegateCommand(param => OnPauseGame());

            // Irányítás
            MoveLeftCommand = new DelegateCommand(param => _model.MoveLeft());
            MoveRightCommand = new DelegateCommand(param => _model.MoveRight());
            RotateCommand = new DelegateCommand(param => _model.Rotate());
            MoveDownCommand = new DelegateCommand(param => _model.MoveDown());
            DropCommand = new DelegateCommand(param => _model.Drop());

            // Tábla inicializálása
            Fields = new ObservableCollection<TetrisField>();
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
                    Fields.Add(new TetrisField
                    {
                        X = x,
                        Y = y,
                        Color = 0
                    });
                }
            }
        }
        /// <summary>
        /// Frissites
        /// </summary>
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
        /// <summary>
        /// törölt sorok
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// jatek betoltes
        /// </summary>

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
            if (Fields.Count != _model.Width * _model.Height)
            {
                InitializeTable();
            }
            RefreshTable();
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
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