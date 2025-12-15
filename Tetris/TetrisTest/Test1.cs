using System;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tetris.Model;
using Tetris.Persistence;


namespace TetrisTest
{
    [TestClass]
    public class TetrisGameModelTest
    {
        private TetrisGameModel _model = null!;
        private MockTimer _mockTimer = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockTimer = new MockTimer();

            _model = new TetrisGameModel(_mockTimer, 10, 20);

            _model.GameUpdated += Model_GameUpdated;
            _model.GameOver += Model_GameOver;
        }

        [TestMethod]
        public void TetrisGameModelNewGameTest()
        {
            _model.StartGame(10, 20);

            Assert.IsTrue(_model.GameStarted);
            Assert.IsFalse(_model.IsPaused);
            Assert.IsFalse(_model.IsGameOver);
            Assert.AreEqual(0, _model.LinesCleared);
            Assert.IsNotNull(_model.CurrentShape);

            Assert.AreEqual(10, _model.Width);
            Assert.AreEqual(20, _model.Height);

            Assert.AreEqual(0, _model.Field[0, _model.Height - 1]);
        }

        [TestMethod]
        public void TetrisGameModelNewGameWithCustomSizeTest()
        {
            int width = 4;
            int height = 16;

            _model.StartGame(width, height);

            Assert.IsTrue(_model.GameStarted);

            Assert.AreEqual(width, _model.Width);
            Assert.AreEqual(height, _model.Height);

            Assert.AreEqual(width, _model.Field.GetLength(0));
            Assert.AreEqual(height, _model.Field.GetLength(1));
        }

        [TestMethod]
        public void TetrisGameModelMovementTest()
        {
            _model.StartGame(10, 20);

            Point startPos = _model.CurrentPosition;

            _model.MoveRight();
            Assert.AreEqual(startPos.X + 1, _model.CurrentPosition.X);
            Assert.AreEqual(startPos.Y, _model.CurrentPosition.Y); 

            _model.MoveLeft();
            Assert.AreEqual(startPos.X, _model.CurrentPosition.X);

            _model.MoveDown();
            Assert.AreEqual(startPos.Y + 1, _model.CurrentPosition.Y);
        }

        [TestMethod]
        public void TetrisGameModelRotationTest()
        {
            _model.StartGame(10, 20);
            var shape = new TShape();
            var state = new GameState
            {
                Field = new int[10, 20],
                CurrentShape = shape,
                CurrentPosition = new Point(5, 5),
                Width = 10,
                Height = 20,
                LinesCleared = 0
            };
            _model.RestoreGameState(state);

            bool eventRaised = false;
            _model.GameUpdated += (s, e) => eventRaised = true;

            _model.Rotate();

            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void TetrisGameModelTimerTest()
        {
            _model.StartGame(10, 20);
            var startY = _model.CurrentPosition.Y;

            _mockTimer.RaiseElapsed();

            Assert.AreEqual(startY + 1, _model.CurrentPosition.Y);
        }

        [TestMethod]
        public void TetrisGameModelLineClearTest()
        {
            int width = 10;
            int height = 20;
            int[,] field = new int[width, height];

            for (int x = 1; x < width; x++)
            {
                field[x, height - 1] = 1;
            }

            var state = new GameState
            {
                Field = field,
                CurrentShape = new LShape(),
                CurrentPosition = new Point(0, height - 3),
                Width = width,
                Height = height,
                LinesCleared = 0
            };

            _model.RestoreGameState(state);
            _model.Drop();
            Assert.IsNotNull(_model.Field);
        }

        [TestMethod]
        public void TetrisGameModelGameStateRestoreTest()
        {
            int width = 10;
            int height = 20;
            int[,] mockField = new int[width, height];
            mockField[5, 19] = 2;

            var savedState = new GameState
            {
                Field = mockField,
                CurrentShape = new TShape(),
                CurrentPosition = new Point(3, 4),
                ElapsedTime = TimeSpan.FromMinutes(1),
                LinesCleared = 5,
                Width = width,
                Height = height
            };

            _model.RestoreGameState(savedState);

            Assert.AreEqual(5, _model.LinesCleared);
            Assert.AreEqual(savedState.Width, _model.Width);
            Assert.AreEqual(savedState.Height, _model.Height);
            Assert.AreEqual(2, _model.Field[5, 19]);
            Assert.AreEqual(3, _model.CurrentPosition.X);
            Assert.AreEqual(4, _model.CurrentPosition.Y);

            Assert.IsTrue(_mockTimer.Enabled);
        }

        [TestMethod]
        public void TetrisGameModelGameOverTest()
        {
            int width = 10;
            int height = 20;
            int[,] fullField = new int[width, height];
            fullField[5, 0] = 1;
            fullField[5, 1] = 1;

            var state = new GameState
            {
                Field = fullField,
                CurrentShape = new TShape(),
                CurrentPosition = new Point(5, 0),
                Width = width,
                Height = height,
                LinesCleared = 0
            };

            _model.RestoreGameState(state);

            _model.MoveDown();

            if (!_model.IsGameOver)
            {
                _mockTimer.RaiseElapsed();
            }

            Assert.IsTrue(_model.IsGameOver);
            Assert.IsFalse(_mockTimer.Enabled);
        }


        private void Model_GameUpdated(object? sender, EventArgs e)
        {
        }

        private void Model_GameOver(object? sender, EventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver);
        }
    }
}