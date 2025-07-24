using Microsoft.AspNetCore.SignalR;
using QuickBingo.Hubs;

namespace QuickBingo.Services
{
    public class BingoService
    {
        private readonly IHubContext<BingoHub> _hubContext;
        private readonly List<int> _remainingNumbers = new List<int>();
        private readonly List<int> _sortedNumbers = new List<int>();
        private readonly Random _random = new Random();

        public bool _isGameActive { get; private set; } = false;
        private bool _isDrawing = false;
        public record Player(string Name, string ConnectionId, List<int> BingoCard);
        private readonly List<Player> _players = new List<Player>();

        private CancellationTokenSource? _cts;

        public BingoService(IHubContext<BingoHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private void InitializeBingoNumbers()
        {
            _remainingNumbers.Clear();
            _sortedNumbers.Clear();
            for (int i = 1; i <= 75; i++)
            {
                _remainingNumbers.Add(i); // Format numbers as two digits
            }
        }

        public List<int> GenerateBingoCard()
        {
            List<int> cardNumbers = Enumerable.Range(1, 75)
                .OrderBy(x => _random.Next())
                .Take(24) // 24 numbers for a Bingo card
                .ToList();

            return cardNumbers;
        }

        public bool IsBingoCardWinner(List<int> bingoCard)
        {
            if (bingoCard == null || bingoCard.Count != 24)
                return false;

            // Inserir "FREE" no centro
            var fullCard = new List<int>();
            for (int i = 0; i < 12; i++)
                fullCard.Add(bingoCard[i]);

            fullCard.Add(-1); // FREE SPACE (posição 12)

            for (int i = 12; i < bingoCard.Count; i++)
                fullCard.Add(bingoCard[i]);

            // Converter em matriz 5x5
            int[,] grid = new int[5, 5];
            for (int i = 0; i < 25; i++)
            {
                grid[i / 5, i % 5] = fullCard[i];
            }


            for (int row = 0; row < 5; row++)
            {
                if (Enumerable.Range(0, 5).All(col => grid[row, col] == -1 || _sortedNumbers.Contains(grid[row, col])))
                    return true;
            }

            for (int col = 0; col < 5; col++)
            {
                if (Enumerable.Range(0, 5).All(row => grid[row, col] == -1 || _sortedNumbers.Contains(grid[row, col])))
                    return true;
            }

            if (Enumerable.Range(0, 5).All(i => grid[i, i] == -1 || _sortedNumbers.Contains(grid[i, i])))
                return true;

            if (Enumerable.Range(0, 5).All(i => grid[i, 4 - i] == -1 || _sortedNumbers.Contains(grid[i, 4 - i])))
                return true;

            return false;
        }

        public List<int> RegisterPlayer(string playerName, string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                var bingoCard = GenerateBingoCard();
                var player = new Player(playerName, connectionId, bingoCard);
                _players.Add(player);

                return bingoCard;
            }
            return null;
        }


        public List<int> GetBingoCard(string playerName)
        {
            var player = _players.FirstOrDefault(p => p.Name == playerName);
            if (player != null && !_isGameActive)
            {
                return player.BingoCard;
            }
            return null;
        }

        public void PauseGame()
        {
            if (_isGameActive)
            {
                _cts?.Cancel();
                _isGameActive = false;
            }
        }

        public void ResumeGame()
        {
            if (!_isGameActive)
            {
                if (_remainingNumbers == null || _remainingNumbers.Count == 0)
                {
                    InitializeBingoNumbers();
                }   
                _isGameActive = true;
                StartAutoDraw();
            }
        }

        public void Reset()
        {
            _cts?.Cancel();
            InitializeBingoNumbers();
            _isGameActive = false;
        }

        public void StartAutoDraw()
        {
            if (_isDrawing)
                return; // already running

            _isDrawing = true;
            _cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    await DrawNumberAsync();
                    try
                    {
                        await Task.Delay(5000, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
                _isDrawing = false;
            }, _cts.Token);
        }


        private async Task DrawNumberAsync()
        {
            if (_remainingNumbers.Count == 0)
            {
                await _hubContext.Clients.All.SendAsync("GameEnded");
                Reset();
                return;
            }
            int index = _random.Next(_remainingNumbers.Count);
            int drawnNumber = _remainingNumbers[index];
            _remainingNumbers.RemoveAt(index);
            _sortedNumbers.Add(drawnNumber);
            await BroadcastNumberAsync(drawnNumber);
        }

        public async Task BroadcastNumberAsync(int number)
        {
            await _hubContext.Clients.All.SendAsync("NumberDrawn", number);
        }

        public Player GetPlayerByConnectionId(string connectionId)
        {
            return _players.FirstOrDefault(p => p.ConnectionId == connectionId);
        }

        public List<string> GetAllPlayerNames()
        {
            return _players.Select(p => p.Name).ToList();
        }

        public List<int> GetSortedNumbers()
        {
            return _sortedNumbers.ToList();
        }

        public void RemovePlayerByConnectionId(string connectionId)
        {
            var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (player != null)
                _players.Remove(player);
        }

    }
}
