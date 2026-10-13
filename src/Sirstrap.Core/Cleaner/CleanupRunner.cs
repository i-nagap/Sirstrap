namespace Sirstrap.Core.Cleaner
{
    public sealed class CleanupRunner(IEnumerable<ICleanupStep> steps, IStatusLine statusLine)
    {
        public void Run()
        {
            List<ICleanupStep> orderedSteps = [.. steps];

            int failedStepCount = 0;

            for (int index = 0; index < orderedSteps.Count; index++)
            {
                int stepNumber = index + 1;

                ICleanupStep step = orderedSteps[index];

                statusLine.Show(string.Format(CultureInfo.InvariantCulture, "Step {0} of {1}: {2}", stepNumber, orderedSteps.Count, step.Name));

                Log.Information("Step {0} of {1}: {2}.", stepNumber, orderedSteps.Count, step.Name);

                try
                {
                    step.Execute();

                    Log.Information("Step {0} of {1} completed.", stepNumber, orderedSteps.Count);
                }
                catch (Exception ex)
                {
                    failedStepCount++;

                    Log.Error(ex, "Step {0} of {1} failed, moving on to the next step (Step: {2}).", stepNumber, orderedSteps.Count, step.Name);
                }
            }

            statusLine.Clear();

            if (failedStepCount == 0)
            {
                Log.Information("Cleanup completed successfully (Steps: {0}).", orderedSteps.Count);
            }
            else
            {
                Log.Warning("Cleanup completed with errors, some items may not have been removed (Failed steps: {0} of {1}).", failedStepCount, orderedSteps.Count);
            }
        }
    }
}