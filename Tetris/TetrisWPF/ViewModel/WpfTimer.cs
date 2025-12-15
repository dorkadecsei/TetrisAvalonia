using System;
using System.Windows.Threading;
using Tetris.Persistence;

namespace TetrisWPF.ViewModel
{
    public class WpfTimer : Tetris.Persistence.ITimer
    {
        private readonly DispatcherTimer _timer;

        public WpfTimer()
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