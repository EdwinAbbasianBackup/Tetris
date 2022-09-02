using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public class TetrisHub : Hub
    {
        public async Task DropShape()
        {
            await Clients.Others.SendAsync("DropShape");
        }

        public async Task RotateShape(string direction)
        {
            await Clients.Others.SendAsync("RotateShape", direction);
        }

        public async Task MoveShape(string moveDirection)
        {
            await Clients.Others.SendAsync("MoveShape", moveDirection);
        }
        
        public async Task ReadyUp(int seed)
        {
            await Clients.Others.SendAsync("ReadyUp", seed);
        }
        public async Task StartGame(int seed)
        {
            await Clients.All.SendAsync("StartGame", seed);
        }

        public async Task SendBoard(string board)
        {
            await Clients.Others.SendAsync("tetris_board", board);
        }

        public async Task SendMatrix(string matrix)
        {
            await Clients.Others.SendAsync("Matrix", matrix);
        }

        public async Task SendNextMatrix(string matrix)
        {
            await Clients.Others.SendAsync("nextMatrix", matrix);
        }
    

        public async Task SendScore(string score)
        {
            await Clients.Others.SendAsync("score", score);
        }
    }
}
