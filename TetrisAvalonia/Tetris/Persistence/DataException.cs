using System;

namespace Tetris.Persistence
{
    public class DataException : Exception
    {
        /// <summary>
        /// Adatelérés Exception
        /// </summary>
        public DataException() { }

        /// <param name="message">A kivétel üzenete.</param>
        public DataException(string message) : base(message) { }
    }
}