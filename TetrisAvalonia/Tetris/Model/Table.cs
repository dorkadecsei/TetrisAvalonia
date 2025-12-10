using System;
using Tetris.Persistence;

namespace Tetris.Model
{
    public class Table
    {
        #region Fields

        private readonly int[,] _fieldValues;
        private readonly int _width;
        private readonly int _height;
        private Shape _currentShape = null!;
        private Position _currentPosition;
        private readonly Random _random;

        #endregion

        #region Properties

        public int Width => _width;
        public int Height => _height;
        public int[,] FieldValues => _fieldValues;
        public Shape CurrentShape => _currentShape;
        public Position CurrentPosition => _currentPosition; // Point helyett Position

        #endregion

        #region Constructors

        public Table(int width, int height)
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

        public void CreateNewShape()
        {
            _currentShape = Shape.CreateRandomShape();
            _currentPosition = new Position(_width / 2 - 1, 0);
        }

        public void SetCurrentShapeAndPosition(Shape shape, Position position)
        {
            _currentShape = shape;
            _currentPosition = position;
        }

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

        public void Drop()
        {
            while (MoveDown()) { }
        }

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

        public int ClearFullLines()
        {
            int cleared = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                if (IsLineFull(y))
                {
                    ClearLine(y);
                    cleared++;
                    y++;
                }
            }
            return cleared;
        }

        #endregion

        #region Private Methods

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

                        if (boardY >= 0 && boardY < _height && boardX >= 0 && boardX < _width)
                        {
                            _fieldValues[boardX, boardY] = _currentShape.Color;
                        }
                    }
                }
            }
        }

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