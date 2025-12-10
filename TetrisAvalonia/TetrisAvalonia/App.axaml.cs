using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TetrisAvaloniaDesktop;
using Tetris.Model;
using TetrisAvalonia.ViewModel;
using Tetris.Persistence;
using System;

namespace TetrisAvalonia
{
    public partial class App : Application
    {
        private GameModel _model = null!;
        private TetrisAvalonia.ViewModel.ViewModel _viewModel = null!;
        private IDataAccess _dataAccess = null!; // <-- ÚJ MEZÕ HOZZÁADÁSA

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 1. Perzisztencia réteg létrehozása
            _dataAccess = new FileDataAccess(); // <-- INITIALIZÁLÁS

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 2. Modell létrehozása (a GameModel konstruktora kell, hogy fogadja a IDataAccess-t)
                _model = new GameModel(new ATimer(), _dataAccess, 10, 20); // <-- _dataAccess átadása

                // 3. Nézetmodell létrehozása és a modell csatlakoztatása
                _viewModel = new TetrisAvalonia.ViewModel.ViewModel(_model);

                // 4. Események feliratkoztatása a fõablakra
                var mainWindow = new MainWindow
                {
                    DataContext = _viewModel
                };

                // Mivel az események a nézetmodellben vannak, a fõablakot (MainWindow.axaml.cs) kell kibõvítenünk.

                desktop.MainWindow = mainWindow;

                // Az elsõ játék elindítása a gombok és a játékidõzítõ mûködéséhez
                _model.StartGame(10, 20);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}