namespace Sirstrap.Core.Cleaner
{
    public sealed class UnattendedUserPrompt : IUserPrompt
    {
        public bool Confirm(string message, bool defaultAnswer = false)
        {
            Log.Debug("Answered a prompt automatically because no one can answer it (Answer: {0}, Prompt: {1}).", defaultAnswer, message);

            return defaultAnswer;
        }
    }
}