namespace Sirstrap.Core.Cleaner
{
    public sealed class ConsoleStatusLine : IStatusLine
    {
        private const string Marker = "» ";

        private readonly Lock _consoleLock = new();

        private int _drawnLength;

        private readonly bool _isConsoleAttached = !Console.IsOutputRedirected;

        private string _status = string.Empty;

        private void Draw()
        {
            if (!_isConsoleAttached || _status.Length == 0)
            {
                return;
            }

            int maximumTextLength = GetConsoleWidth() - Marker.Length;
            string text = _status.Length > maximumTextLength ? _status[..maximumTextLength] : _status;

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.Write($"{Marker}{text}");
            Console.ResetColor();

            _drawnLength = text.Length + Marker.Length;
        }

        private void Erase()
        {
            if (_drawnLength == 0)
            {
                return;
            }

            Console.Write('\r');
            Console.Write(new string(' ', _drawnLength));
            Console.Write('\r');

            _drawnLength = 0;
        }

        private static int GetConsoleWidth()
        {
            try
            {
                return Math.Max(Console.WindowWidth - 1, 20);
            }
            catch (IOException)
            {
                return 79;
            }
        }

        public void Clear()
        {
            Show(string.Empty);
        }

        public void RunWithStatusHidden(Action action)
        {
            lock (_consoleLock)
            {
                Erase();

                action();

                Draw();
            }
        }

        public void Show(string status)
        {
            lock (_consoleLock)
            {
                Erase();

                _status = status;

                Draw();
            }
        }
    }
}