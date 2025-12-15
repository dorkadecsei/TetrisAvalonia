using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using Tetris.Persistence;

namespace Tetris.Model
{
    public class TetrisTable
    {
        #region Fields

        private readonly int[,] _fieldValues;
        private readonly int _width;
        private readonly int _height;
        private TetrisShape _currentShape = null!;
        private Point _currentPosition;
        private readonly Random _random;

        #endregion

        #region Properties

        public int Width => _width;
        public int Height => _height;
        public int[,] FieldValues => _fieldValues;
        public TetrisShape CurrentShape => _currentShape;
        public Point CurrentPosition => _currentPosition;

        #endregion

        #region Constructors
        /// <summary>
        /// Tabla inicializalasa
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public TetrisTable(int width, int height)
        {
            _width = width;
            _height = height;
            _fieldValues = new int[width, height];
            _random = new Random();

            ClearField();
            CreateNewShape();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// field torlese
        /// </summary>
        public void ClearField()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _fieldValues[x, y] = 0;
                }
            }
        }
        /// <summary>
        /// uj shape
        /// </summary>
        public void CreateNewShape()
        {
            _currentShape = TetrisShape.CreateRandomShape();
            _currentPosition = new Point(_width / 2 - 1, 0);
        }

        public void SetCurrentShapeAndPosition(TetrisShape shape, Point position)
        {
            _currentShape = shape;
            _currentPosition = position;
        }
        /// <summary>
        /// shape mozgatasa
        /// </summary>
        /// <returns></returns>
        public bool MoveLeft()
        {
            _currentPosition.X--;
            if (!IsValidPosition())
            {
                _currentPosition.X++;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Jobbra lépés
        /// </summary>
        /// <returns></returns>

        public bool MoveRight()
        {
            _currentPosition.X++;
            if (!IsValidPosition())
            {
                _currentPosition.X--;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Lefele lepes egyet
        /// </summary>
        /// <returns></returns>
        public bool MoveDown()
        {
            _currentPosition.Y++;
            if (!IsValidPosition())
            {
                _currentPosition.Y--;
                LockShape();
                CreateNewShape();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Forgatas
        /// </summary>

        public void Rotate()
        {
            _currentShape.Rotate();
            if (!IsValidPosition())
            {
                for (int i = 0; i < 3; i++)
                {
                    _currentShape.Rotate();
                }
            }
        }
        /// <summary>
        /// Leejtes
        /// </summary>
        public void Drop()
        {
            while (MoveDown()) { }
        }
        /// <summary>
        /// jatek vege check
        /// </summary>
        /// <returns></returns>
        public bool IsGameOver()
        {
            for (int x = 0; x < _width; x++)
            {
                if (_fieldValues[x, 0] != 0 || _fieldValues[x, 1] != 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// betelt sor torlese
        /// </summary>
        /// <returns></returns>
        public int ClearFullLines()
        {
            int cleared = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                if (IsLineFull(y))
                {
                    ClearLine(y);
                    cleared++;
                }
            }
            return cleared;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// shape helyzetenek validalasa
        /// </summary>
        /// <returns></returns>
        private bool IsValidPosition()
        {
            for (int x = 0; x < _currentShape.Size; x++)
            {
                for (int y = 0; y < _currentShape.Size; y++)
                {
                    if (_currentShape[x, y] != 0)
                    {
                        int boardX = _currentPosition.X + x;
                        int boardY = _currentPosition.Y + y;

                        if (boardX < 0 || boardX >= _width || boardY >= _height)
                        {
                            return false;
                        }

                        if (boardY >= 0 && _fieldValues[boardX, boardY] != 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// shape rogzitese
        /// </summary>
        private void LockShape()
        {
            for (int x = 0; x < _currentShape.Size; x++)
            {
                for (int y = 0; y < _currentShape.Size; y++)
                {
                    if (_currentShape[x, y] != 0)
                    {
                        int boardX = _currentPosition.X + x;
                        int boardY = _currentPosition.Y + y;

                        if (boardY >= 0)
                        {
                            _fieldValues[boardX, boardY] = _currentShape.Color;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Betelt-e a sor?
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsLineFull(int y)
        {
            for (int x = 0; x < _width; x++)
            {
                if (_fieldValues[x, y] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sort kitorli
        /// </summary>
        /// <param name="lineY"></param>
        private void ClearLine(int lineY)
        {
            for (int y = lineY; y > 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    _fieldValues[x, y] = _fieldValues[x, y - 1];
                }
            }

            for (int x = 0; x < _width; x++)
            {
                _fieldValues[x, 0] = 0;
            }
        }

        #endregion
    }
}