using System;
using System.Threading.Tasks;
using Tetris;
using Tetris.Model;

namespace Tetris.Persistence
{
    public interface ITetrisDataAccess
    {
        Task<GameState> LoadAsync(string path);

        Task SaveAsync(string path, GameState gameState);
    }
}