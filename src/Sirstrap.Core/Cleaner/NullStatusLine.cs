namespace Sirstrap.Core.Cleaner
{
    public sealed class NullStatusLine : IStatusLine
    {
        public void Clear()
        {
        }

        public void RunWithStatusHidden(Action action)
        {
            action();
        }

        public void Show(string status)
        {
        }
    }
}