using Avalonia.Media;
using System;
using System.Media;
using Tetris.Model;
using TetrisAvalonia.ViewModel;

namespace TetrisAvalonia.ViewModel
{
    public class Fields : ViewModelBase
    {
        private int _color;

        /// <summary>
        /// A mező színkódja
        /// A Modelből érkező szám.
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
                    OnPropertyChanged(nameof(DisplayColor));
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

        public Fields(int x, int y)
        {
            X = x;
            Y = y;
            Color = 0;
        }

        // OPCIONÁLIS: Segédtulajdonság a WPF megjelenítéshez
        // Ha nincs Convertered XAML-ben, akkor ez fordítja le a számot valódi színre.
        public IBrush DisplayColor
        {
            get
            {
                switch (_color)
                {
                    case 1: return Brushes.ForestGreen;
                    case 2: return Brushes.Lime;
                    case 3: return Brushes.Green;
                    case 4: return Brushes.LawnGreen;
                    case 5: return Brushes.LightGreen;
                    case 6: return Brushes.DarkGreen;
                    case 7: return Brushes.SpringGreen;
                    default: return Brushes.White;
                }
            }
        }
    }
}