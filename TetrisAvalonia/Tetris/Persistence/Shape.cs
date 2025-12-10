using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris.Persistence
{
    public abstract class Shape
    {
        protected int[,]? _shape;
        protected int _color;
        protected static Random _random = new();

        public int Size => _shape!.GetLength(0);
        public int Color => _color;

        public Shape()
        {
            _color = _random.Next(1, 8);
        }

        public int this[int x, int y] => _shape![x, y];

        public virtual void Rotate()
        {
            int size = Size;
            int[,] rotated = new int[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    rotated[x, y] = _shape![size - 1 - y, x];
                }
            }

            _shape = rotated;
        }

        public static Shape ShapeFactory(ShapeType type)
        {
            return type switch
            {
                ShapeType.K => new KShape(),
                ShapeType.R => new RShape(),
                ShapeType.L => new LShape(),
                ShapeType.E => new EShape(),
                ShapeType.T => new TShape(),
                _ => throw new ArgumentException("rossz tipus")
            };
        }

        public static Shape CreateRandomShape()
        {
            ShapeType[] values = Enum.GetValues<ShapeType>();
            ShapeType randomType = values[_random.Next(values.Length)];
            return ShapeFactory(randomType);
        }

        public void SetColor(int color)
        {
            _color = color;
        }

        protected void SetShapeMatrix(int[,] matrix)
        {
            if (matrix.GetLength(0) != Size || matrix.GetLength(1) != Size)
                throw new ArgumentException("rossz meretek");

            _shape = matrix;
        }
        public Shape Clone()
        {
            ShapeType type = this switch
            {
                KShape => ShapeType.K,
                EShape => ShapeType.E,
                LShape => ShapeType.L,
                TShape => ShapeType.T,
                RShape => ShapeType.R,
                _ => ShapeType.K
            };

            Shape clonedShape = ShapeFactory(type);

            clonedShape._color = this._color;

            if (this._shape != null)
            {
                clonedShape._shape = (int[,])this._shape.Clone();
            }

            return clonedShape;
        }
    }

    public enum ShapeType
    {
        K, E, L, T, R
    }

    public class KShape : Shape
    {
        public KShape()
        {
            _shape = new int[2, 2] {
                {1, 1},
                {1, 1}
            };
        }
    }

    public class EShape : Shape
    {
        public EShape()
        {
            _shape = new int[4, 4] {
                {0, 0, 0, 0},
                {1, 1, 1, 1},
                {0, 0, 0, 0},
                {0, 0, 0, 0}
            };
        }
    }

    public class LShape : Shape
    {
        public LShape()
        {
            _shape = new int[3, 3] {
                {0, 0, 1},
                {1, 1, 1},
                {0, 0, 0}
            };
        }
    }

    public class TShape : Shape
    {
        public TShape()
        {
            _shape = new int[3, 3] {
                {0, 1, 0},
                {1, 1, 1},
                {0, 0, 0}
            };
        }
    }

    public class RShape : Shape
    {
        public RShape()
        {
            _shape = new int[3, 3] {
                {0, 0, 1},
                {0, 1, 1},
                {0, 1, 0}
            };
        }
    }
}