namespace QuickBingo.Services
{
    public class BingoService
    {
        private readonly List<int> _bingoNumbers;
        private readonly Random _random;

        public BingoService()
        {
            _bingoNumbers = new List<int>();
            _random = new Random();
            InitializeBingoNumbers();
        }

        private void InitializeBingoNumbers()
        {
            for (int i = 1; i <= 75; i++)
            {
                _bingoNumbers.Add(i); // Format numbers as two digits
            }
        }

        public int DrawNumber()
        {
            if (_bingoNumbers.Count == 0)
            {
                return -1; // No more numbers to draw
            }
            int index = _random.Next(_bingoNumbers.Count);
            int drawnNumber = _bingoNumbers[index];
            _bingoNumbers.RemoveAt(index); // Remove the drawn number
            return drawnNumber;
        }

        public void Reset()
        {
            _bingoNumbers.Clear();
            InitializeBingoNumbers();
        }

        public void GenerateBingoCard()
        {
            List<int> cardNumbers = Enumerable.Range(1, 75)
                .OrderBy(x => _random.Next())
                .Take(24) // 24 numbers for a Bingo card
                .ToList();
        }
    }
}
