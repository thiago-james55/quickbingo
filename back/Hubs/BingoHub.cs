using Microsoft.AspNetCore.SignalR;
using QuickBingo.Services;

namespace QuickBingo.Hubs
{

    public class BingoHub : Hub
    {
        private readonly BingoService _bingoService;

        public BingoHub(BingoService bingoService)
        {
           _bingoService = bingoService;
        }

        public async Task<List<int>> RegisterPlayer(string playerName)
        {
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                string connectionId = Context.ConnectionId;

                List<int> bingoCard = _bingoService.RegisterPlayer(playerName, connectionId);
                if (bingoCard == null) return null;
                await Clients.All.SendAsync("PlayerRegistered", playerName);
                await Clients.All.SendAsync("PlayerListUpdated", _bingoService.GetAllPlayerNames());

                return bingoCard;
            }
            return null;
        }

        public async Task<List<int>> GetBingoCard()
        {
            BingoService.Player player = _bingoService.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null && !_bingoService._isGameActive)
            {
                return player.BingoCard;
            }
            return null;
        }

        public async Task Reset()
        {
            if (!_bingoService._isGameActive)
            {
                _bingoService.Reset();
                await Clients.All.SendAsync("GameReseted");
            }
        }

        public async Task PauseGame()
        {
            if (_bingoService._isGameActive)
            {
                _bingoService.PauseGame();
                await Clients.All.SendAsync("GamePaused");
            }
        }

        public async Task ResumeGame()
        {
            if (!_bingoService._isGameActive)
            {
                _bingoService.ResumeGame();
                await Clients.All.SendAsync("GameResumed");
            }
        }

        public async Task WinGame(List<int> bingoCard)
        {
            var connectionId = Context.ConnectionId;
            BingoService.Player player = _bingoService.GetPlayerByConnectionId(connectionId);

            if (player != null && player.BingoCard.SequenceEqual(bingoCard))
            {
                if (_bingoService.IsBingoCardWinner(bingoCard))
                {
                    await Clients.All.SendAsync("Winner", player.Name);
                    _bingoService.Reset();
                }
            }
        }

        public Task<List<int>> GetSortedNumbers()
        {
            return Task.FromResult(_bingoService.GetSortedNumbers());
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var player = _bingoService.GetPlayerByConnectionId(Context.ConnectionId);
            if (player != null)
            {
                _bingoService.RemovePlayerByConnectionId(Context.ConnectionId);
                await Clients.All.SendAsync("PlayerUnregistered", player.Name);
                await Clients.All.SendAsync("PlayerListUpdated", _bingoService.GetAllPlayerNames());
            }

            await base.OnDisconnectedAsync(exception);
        }


    }
}
