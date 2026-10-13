namespace Sirstrap.Core.Cleaner
{
    public interface IUserPrompt
    {
        bool Confirm(string message, bool defaultAnswer = false);
    }
}