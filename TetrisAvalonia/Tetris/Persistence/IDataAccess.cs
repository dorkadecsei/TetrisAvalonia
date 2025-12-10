using System;
using System.Threading.Tasks;
using Tetris.Model;

namespace Tetris.Persistence
{
    public interface IDataAccess
    {
        Task<GameState> LoadAsync(string path);

        Task SaveAsync(string path, GameState gameState);
    }
}