using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage; 
using System;
using System.Collections.Generic;
using Tetris.Model;
using Tetris.Persistence;
using TetrisAvalonia.ViewModel;
using TetrisAvalonia.Views;

namespace TetrisAvalonia
{
    public partial class App : Application
    {
        private GameModel? _model;
        private TetrisViewModel? _viewModel;
        private MainWindow? _view;
        private ATimer? _timer;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Timer és Model létrehozása
                _timer = new ATimer();
                _model = new GameModel(_timer, 10, 20);

                // ViewModel létrehozása
                _viewModel = new TetrisViewModel(_model);

                // Események feliratkozása
                _viewModel.LoadGame += ViewModel_LoadGame;
                _viewModel.SaveGame += ViewModel_SaveGame;
                _viewModel.ExitGame += ViewModel_ExitGame;

                // View létrehozása és DataContext beállítása
                _view = new MainWindow
                {
                    DataContext = _viewModel
                };

                desktop.MainWindow = _view;

                // Játék indítása
                _model.StartGame(10, 20);
            }

            base.OnFrameworkInitializationCompleted();
        }

        // --- Új típusú Fájlkezelés (StorageProvider) ---

        private async void ViewModel_LoadGame(object? sender, EventArgs e)
        {
            if (_view == null) return;

            try
            {
                // Ha fut a játék, szüneteltetjük
                if (_model!.GameStarted && !_model.IsPaused)
                    _model.PauseGame();

                // Fájlválasztó ablak
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(_view);
                var files = await topLevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Tetris játék betöltése",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("Tetris Save") { Patterns = new[] { "*.txt" } } }
                });

                if (files.Count > 0)
                {
                    // Betöltés
                    IDataAccess dataAccess = new FileDataAccess();
                    // A TryGetLocalPath() visszaadja a teljes útvonalat stringként
                    // Megjegyzés: Mobilon/Weben streamet kéne használni, de asztali gépen ez jó.
                    string path = files[0].Path.LocalPath;
                    GameState state = await dataAccess.LoadAsync(path);

                    _model.RestoreGameState(state);
                }
            }
            catch (Exception ex)
            {
                // Avaloniában a MessageBox nem beépített, konzolra írunk vagy kell egy nuget csomag (pl. MessageBox.Avalonia)
                Console.WriteLine("Hiba a betöltéskor: " + ex.Message);
            }
        }

        private async void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            if (_view == null || !_model!.GameStarted) return;

            try
            {
                bool wasPaused = _model.IsPaused;
                if (!wasPaused) _model.PauseGame();

                // Fájlmentõ ablak
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(_view);
                var file = await topLevel!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Tetris játék mentése",
                    DefaultExtension = "txt",
                    FileTypeChoices = new[] { new FilePickerFileType("Tetris Save") { Patterns = new[] { "*.txt" } } }
                });

                if (file != null)
                {
                    IDataAccess dataAccess = new FileDataAccess();
                    await dataAccess.SaveAsync(file.Path.LocalPath, _model.SaveGameState());
                }
                else if (!wasPaused)
                {
                    // Ha a felhasználó mégsem mentett ("Mégse"), folytatjuk
                    _model.ResumeGame();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hiba a mentéskor: " + ex.Message);
            }
        }

        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}