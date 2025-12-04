using Avalonia.Threading;
using System;
using System.Threading;
using TetrisAvalonia.Model;

namespace TetrisAvalonia.ViewModel
{
    public class ATimer : TetrisAvalonia.Model.ITimer
    {
        private readonly DispatcherTimer _timer;

        public ATimer()
        {
            _timer = new DispatcherTimer();
            // Az Avalonia DispatcherTimer Tick eseménye szintén EventHandler
            _timer.Tick += (s, e) => Elapsed?.Invoke(this, EventArgs.Empty);
        }

        public bool Enabled
        {
            get => _timer.IsEnabled;
            set => _timer.IsEnabled = value;
        }

        // Az Avalonia Timer Intervalja TimeSpan típusú, de az ITimer double-t (ms) vár
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