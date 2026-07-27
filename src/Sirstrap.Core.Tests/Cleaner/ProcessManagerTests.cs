using System.Diagnostics;

namespace Sirstrap.Core.Tests.Cleaner
{
    public class ProcessManagerTests
    {
        private readonly Sirstrap.Core.Cleaner.ProcessManager _manager = new();

        [Fact]
        public void IsProcessRunning_ReturnsFalse_ForNonExistentProcess()
        {
            Assert.False(_manager.IsProcessRunning($"sirstrap-nope-{Guid.NewGuid():N}"));
        }

        [Fact]
        public void TryKillProcess_ReturnsTrue_WhenNoInstancesRunning()
        {
            Assert.True(_manager.TryKillProcess($"sirstrap-nope-{Guid.NewGuid():N}"));
        }

        [Fact]
        public void IsProcessRunning_IgnoresTheCurrentProcess()
        {
            using var currentProcess = Process.GetCurrentProcess();

            Assert.False(_manager.IsProcessRunning(currentProcess.ProcessName));
        }

        [Fact]
        public void TryKillProcess_DoesNotKillTheCurrentProcess()
        {
            using var currentProcess = Process.GetCurrentProcess();

            Assert.True(_manager.TryKillProcess(currentProcess.ProcessName));
            Assert.False(currentProcess.HasExited);
        }
    }
}
