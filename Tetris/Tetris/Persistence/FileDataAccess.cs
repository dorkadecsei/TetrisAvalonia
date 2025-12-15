using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Tetris.Model;

namespace Tetris.Persistence
{
    public class FileDataAccess : ITetrisDataAccess
    {
        /// <summary>
        /// Fájl betöltése
        /// </summary>
        public async Task<GameState> LoadAsync(string path)
        {
            try
            {
                using StreamReader reader = new StreamReader(path);
                string line = await reader.ReadLineAsync() ?? string.Empty;
                string[] header = line.Split(' ');
                int width = int.Parse(header[0]);
                int height = int.Parse(header[1]);
                TimeSpan elapsedTime = TimeSpan.Parse(header[2]);
                int linesCleared = int.Parse(header[3]);

                line = await reader.ReadLineAsync() ?? string.Empty;
                string[] shapeInfo = line.Split(' ');
                ShapeType currentShapeType = Enum.Parse<ShapeType>(shapeInfo[0]);
                int shapeColor = int.Parse(shapeInfo[1]);

                line = await reader.ReadLineAsync() ?? string.Empty;
                string[] position = line.Split(' ');
                Point currentPosition = new Point(int.Parse(position[0]), int.Parse(position[1]));

                line = await reader.ReadLineAsync() ?? string.Empty;
                int shapeSize = int.Parse(line);
                int[,] shapeMatrix = new int[shapeSize, shapeSize];

                for (int i = 0; i < shapeSize; i++)
                {
                    line = await reader.ReadLineAsync() ?? string.Empty;
                    string[] shapeRow = line.Split(' ');
                    for (int j = 0; j < shapeSize; j++)
                    {
                        shapeMatrix[i, j] = int.Parse(shapeRow[j]);
                    }
                }

                int[,] field = new int[width, height];
                for (int y = 0; y < height; y++)
                {
                    line = await reader.ReadLineAsync() ?? string.Empty;
                    string[] fieldRow = line.Split(' ');
                    for (int x = 0; x < width; x++)
                    {
                        field[x, y] = int.Parse(fieldRow[x]);
                    }
                }

                TetrisShape currentShape = TetrisShape.ShapeFactory(currentShapeType);
                SetShapeMatrix(currentShape, shapeMatrix);
                currentShape.SetColor(shapeColor);

                return new GameState
                {
                    Field = field,
                    CurrentShape = currentShape,
                    CurrentPosition = currentPosition,
                    ElapsedTime = elapsedTime,
                    LinesCleared = linesCleared,
                    Width = width,
                    Height = height
                };
            }
            catch (Exception ex)
            {
                throw new DataException("Hiba a fájl betöltése során: " + ex.Message);
            }
        }

        /// <summary>
        /// Fájl mentése
        /// </summary>
        public async Task SaveAsync(string path, GameState gameState)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    await writer.WriteLineAsync(
                        $"{gameState.Width} {gameState.Height} {gameState.ElapsedTime} {gameState.LinesCleared}");

                    var shapeType = GetShapeType(gameState.CurrentShape!);
                    await writer.WriteLineAsync($"{shapeType} {gameState.CurrentShape!.Color}");

                    await writer.WriteLineAsync($"{gameState.CurrentPosition.X} {gameState.CurrentPosition.Y}");

                    int shapeSize = gameState.CurrentShape.Size;
                    await writer.WriteLineAsync(shapeSize.ToString());

                    for (int i = 0; i < shapeSize; i++)
                    {
                        for (int j = 0; j < shapeSize; j++)
                        {
                            await writer.WriteAsync(gameState.CurrentShape[i, j] + " ");
                        }
                        await writer.WriteLineAsync();
                    }

                    for (int y = 0; y < gameState.Height; y++)
                    {
                        for (int x = 0; x < gameState.Width; x++)
                        {
                            await writer.WriteAsync(gameState.Field![x, y] + " ");
                        }
                        await writer.WriteLineAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Hiba a fájl mentése során: " + ex.Message);
            }
        }

        private static ShapeType GetShapeType(TetrisShape shape)
        {
            return shape switch
            {
                KShape => ShapeType.K,
                EShape => ShapeType.E,
                LShape => ShapeType.L,
                TShape => ShapeType.T,
                RShape => ShapeType.R,
                _ => ShapeType.K
            };
        }

        private static void SetShapeMatrix(TetrisShape shape, int[,] matrix)
        {
            var field = shape.GetType().GetField("_shape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(shape, matrix);
            }
        }
    }
}