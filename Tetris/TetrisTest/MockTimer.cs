using System;

namespace TetrisTest
{
    /// <summary>
    /// Időzítő mockolása
    /// </summary>
    public class MockTimer : Tetris.Persistence.ITimer
    {
        public bool Enabled { get; set; }
        public double Interval { get; set; }
        public event EventHandler? Elapsed;

        public void Start()
        {
            Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
        }

        /// <summary>
        /// Időzítő eseményének kiváltása.
        /// </summary>
        public void RaiseElapsed()
        {
            Elapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}