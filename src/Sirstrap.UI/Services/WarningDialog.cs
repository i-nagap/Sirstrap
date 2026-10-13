namespace Sirstrap.UI.Services
{
    public static class WarningDialog
    {
        private const uint WarningIcon = 0x00000030;

        private const int YesAnswer = 6;

        private const uint YesNoButtons = 0x00000004;

        public static Task<bool> ConfirmAsync(string caption, string message)
        {
            return Task.Run(() => NativeMethods.MessageBox(IntPtr.Zero, message, caption, YesNoButtons | WarningIcon) == YesAnswer);
        }
    }
}