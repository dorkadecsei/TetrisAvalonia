using System;
using Tetris.Model;

namespace TetrisWPF.ViewModel
{
    public class TetrisField : ViewModelBase
    {
        private int _color;

        /// <summary>
        /// Színkód lekérdezése
        /// </summary>
        public int Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Vízszintes koordináta
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Függőleges koordináta
        /// </summary>
        public int Y { get; set; }
    }
}