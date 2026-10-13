namespace Sirstrap.Core.Cleaner
{
    public interface IStatusLine
    {
        void Clear();

        void RunWithStatusHidden(Action action);

        void Show(string status);
    }
}