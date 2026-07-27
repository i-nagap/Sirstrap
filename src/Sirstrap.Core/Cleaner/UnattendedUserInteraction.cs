namespace Sirstrap.Core.Cleaner
{
    public sealed class UnattendedUserInteraction : IUserInteraction
    {
        public bool Confirm(string message, bool defaultAnswer = false)
        {
            Log.Information("[*] Answered {Answer} to \"{Message}\" because no user is attached.", defaultAnswer, message);

            return defaultAnswer;
        }
    }
}
