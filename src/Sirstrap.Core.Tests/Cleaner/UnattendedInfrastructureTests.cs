namespace Sirstrap.Core.Tests.Cleaner
{
    public class UnattendedInfrastructureTests
    {
        [Fact]
        public void NullStatusLine_SetStatusClearAndHidden_DoNotThrow()
        {
            NullStatusLine statusLine = new();
            int actionRuns = 0;

            var exception = Record.Exception(() =>
            {
                statusLine.SetStatus("working");
                statusLine.InvokeWithStatusHidden(() => actionRuns++);
                statusLine.Clear();
            });

            Assert.Null(exception);
            Assert.Equal(1, actionRuns);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UnattendedUserInteraction_Confirm_ReturnsTheDefaultAnswer(bool defaultAnswer)
        {
            UnattendedUserInteraction interaction = new();

            Assert.Equal(defaultAnswer, interaction.Confirm("Proceed?", defaultAnswer));
        }

        [Fact]
        public void UnattendedUserInteraction_Confirm_DeclinesWhenNoDefaultIsGiven()
        {
            UnattendedUserInteraction interaction = new();

            Assert.False(interaction.Confirm("Proceed?"));
        }
    }
}
