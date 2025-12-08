using System;
using System.Timers;
using Tetris.Model;

namespace Tetris.Persistence
{
    // Átneveztem SudokuTimerAggregation-ról TimerWrapper-re
    public class TimerWrapper : ITimer, IDisposable
    {
        private readonly Timer _timer;

        public bool Enabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public event EventHandler? Elapsed;

        public TimerWrapper()
        {
            _timer = new Timer();
            _timer.Elapsed += (sender, e) =>
            {
                Elapsed?.Invoke(sender, e);
            };
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}