namespace Sirstrap.Core.Cleaner
{
    public sealed class ConsoleUserPrompt(IStatusLine statusLine) : IUserPrompt
    {
        public bool Confirm(string message, bool defaultAnswer = false)
        {
            bool isConfirmed = defaultAnswer;

            statusLine.RunWithStatusHidden(() =>
            {
                Console.WriteLine(message);
                Console.Write(defaultAnswer ? "Proceed (Y/n): " : "Proceed (y/N): ");

                isConfirmed = IsAffirmative(Console.ReadLine(), defaultAnswer);
            });

            return isConfirmed;
        }

        public static bool IsAffirmative(string? answer, bool defaultAnswer)
        {
            string? trimmedAnswer = answer?.Trim();

            return string.IsNullOrEmpty(trimmedAnswer) ? defaultAnswer : trimmedAnswer.Equals("y", StringComparison.OrdinalIgnoreCase) || trimmedAnswer.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}