using Avalonia.Threading;
using System;
using System.Threading;
using Tetris.Model;

namespace TetrisAvalonia.ViewModel
{
    public class ATimer : Tetris.Model.ITimer
    {
        private readonly DispatcherTimer _timer;

        public ATimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += (s, e) => Elapsed?.Invoke(this, EventArgs.Empty);
        }

        public bool Enabled
        {
            get => _timer.IsEnabled;
            set => _timer.IsEnabled = value;
        }

        public double Interval
        {
            get => _timer.Interval.TotalMilliseconds;
            set => _timer.Interval = TimeSpan.FromMilliseconds(value);
        }

        public event EventHandler? Elapsed;

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
    }
}