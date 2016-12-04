using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Tests.TestUtility;
using Moq;
using Serilog;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class FlowDrivenAttentionManagerTests
    {
        private static readonly ILogger Log = new LoggerConfiguration().CreateLogger();

        [Fact]
        public void Mofichan_Should_Be_Paying_Attention_To_User_After_Attention_Is_Renewed_To_Them()
        {
            // GIVEN an attention manager.
            var flowDriver = new ControllableFlowDriver();

            using (var attentionManager = new FlowDrivenAttentionManager(0, 1, flowDriver, Log))
            {
                // GIVEN a user.
                var mockUser = new Mock<IUser>();
                mockUser.SetupGet(it => it.UserId).Returns("John Appleseed");

                // WHEN attention is renewed to the user.
                attentionManager.RenewAttentionTowardsUser(mockUser.Object);

                // THEN Mofichan should be paying attention to that user.
                attentionManager.IsPayingAttentionToUser(mockUser.Object).ShouldBeTrue();
            }
        }

        [Theory]
        [InlineData(100, 10, 67, 133)]
        [InlineData(400, 50, 235, 565)]
        [InlineData(1000, 200, 342, 1658)]
        public void Mofichan_Attention_Span_Should_Follow_Gaussian_Distribution(
            double mu,
            double sigma,
            int lowStepBound,
            int highStepBound)
        {
            int numSteps = 0;

            // GIVEN an attention manager specified to sample from a particular gaussian distribution.
            var flowDriver = new ControllableFlowDriver();

            using (var attentionManager = new FlowDrivenAttentionManager(mu, sigma, flowDriver, Log))
            {

                // GIVEN a user.
                var mockUser = new Mock<IUser>();
                mockUser.SetupGet(it => it.UserId).Returns("John Appleseed");

                // WHEN attention is renewed to the user.
                attentionManager.RenewAttentionTowardsUser(mockUser.Object);

                // AND we step for [lowStepBound] number of steps.
                for (; numSteps < lowStepBound; numSteps++)
                {
                    flowDriver.StepFlow();
                }

                // THEN given the attention manager works correctly, Mofichan should still be paying attention
                // to the user with 99.95% probability.
                attentionManager.IsPayingAttentionToUser(mockUser.Object).ShouldBeTrue();

                // WHEN we step until [highStepBound] number of steps.
                for (; numSteps < highStepBound; numSteps++)
                {
                    flowDriver.StepFlow();
                }

                // THEN given the attention manager works correctly, Mofichan should not be paying attention
                // to the user with 99.95% probability.
                attentionManager.IsPayingAttentionToUser(mockUser.Object).ShouldBeFalse();
            }
        }
    }
}
