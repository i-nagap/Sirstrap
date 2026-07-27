namespace Sirstrap.Core.Cleaner
{
    public sealed class NullStatusLine : IStatusLine
    {
        public void Clear() { }

        public void InvokeWithStatusHidden(Action action) => action();

        public void SetStatus(string status) { }
    }
}
