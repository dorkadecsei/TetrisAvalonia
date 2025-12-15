using Microsoft.Win32;
using System;
using System.Windows;
using Tetris.Model;
using Tetris.Persistence;
using TetrisWPF.ViewModel;

namespace TetrisWPF
{
    public partial class App : Application
    {
        private TetrisGameModel? _model;
        private TetrisViewModel? _viewModel;
        private View.MainWindow? _view;
        private ViewModel.WpfTimer? _timer;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _timer = new ViewModel.WpfTimer();
            _model = new TetrisGameModel(_timer, 10, 20);

            _viewModel = new TetrisViewModel(_model);

            _viewModel.LoadGame += ViewModel_LoadGame;
            _viewModel.SaveGame += ViewModel_SaveGame;
            _viewModel.ExitGame += ViewModel_ExitGame;

            _view = new View.MainWindow();
            _view.DataContext = _viewModel;

            _model.StartGame(10, 20);

            _view.Show();
        }


        private async void ViewModel_LoadGame(object? sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Tetris Save|*.txt" };
                if (openFileDialog.ShowDialog() == true)
                {
                    if (_model!.GameStarted) _model.PauseGame(); // Modell elérése közvetlenül

                    ITetrisDataAccess dataAccess = new FileDataAccess();
                    GameState state = await dataAccess.LoadAsync(openFileDialog.FileName);
                    _model.RestoreGameState(state);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltéskor: " + ex.Message);
            }
        }

        private async void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            try
            {
                if (!_model!.GameStarted) return;

                bool wasPaused = _model.IsPaused;
                if (!wasPaused) _model.PauseGame();

                SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Tetris Save|*.txt" };
                if (saveFileDialog.ShowDialog() == true)
                {
                    ITetrisDataAccess dataAccess = new FileDataAccess();
                    await dataAccess.SaveAsync(saveFileDialog.FileName, _model.SaveGameState());
                }
                else if (!wasPaused)
                {
                    _model.ResumeGame();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mentéskor: " + ex.Message);
            }
        }

        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            _view!.Close();
        }
    }
}