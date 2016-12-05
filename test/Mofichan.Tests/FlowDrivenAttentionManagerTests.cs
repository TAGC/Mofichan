using Mofichan.Core.Flow;
using Mofichan.Tests.TestUtility;
using Serilog;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class FlowDrivenAttentionManagerTests
    {
        private static readonly ILogger Log = new LoggerConfiguration().CreateLogger();

        [Fact]
        public void Mofichan_Should_Lose_Attention_To_All_Users_When_Requested()
        {
            // GIVEN an attention manager.
            var flowDriver = new ControllableFlowDriver();

            using (var attentionManager = new FlowDrivenAttentionManager(0, 1, flowDriver, Log))
            {
                // GIVEN a collection of users.
                var tom = new MockUser("Tom", "Tom");
                var dick = new MockUser("Dick", "Dick");
                var harry = new MockUser("Harry", "Harry");

                // WHEN Mofichan's attention is renewed towards Tom.
                attentionManager.RenewAttentionTowardsUser(tom);

                // THEN she should only be paying attention towards Tom.
                attentionManager.IsPayingAttentionToUser(tom).ShouldBeTrue();
                attentionManager.IsPayingAttentionToUser(dick).ShouldBeFalse();
                attentionManager.IsPayingAttentionToUser(harry).ShouldBeFalse();

                // WHEN Mofichan's attention is renewed towards Dick.
                attentionManager.RenewAttentionTowardsUser(dick);

                // THEN she should be paying attention to Tom and Dick.
                attentionManager.IsPayingAttentionToUser(tom).ShouldBeTrue();
                attentionManager.IsPayingAttentionToUser(dick).ShouldBeTrue();
                attentionManager.IsPayingAttentionToUser(harry).ShouldBeFalse();

                // WHEN Mofichan's attention is renewed towards Harry.
                attentionManager.RenewAttentionTowardsUser(harry);

                // THEN she should be paying attentio towards Tom, Dick and Harry.
                attentionManager.IsPayingAttentionToUser(tom).ShouldBeTrue();
                attentionManager.IsPayingAttentionToUser(dick).ShouldBeTrue();
                attentionManager.IsPayingAttentionToUser(harry).ShouldBeTrue();

                // WHEN Mofichan is requested to lose attention towards everyone.
                attentionManager.LoseAttentionToAllUsers();

                // THEN she should not be paying attention towards Tom, Dick or Harry.
                attentionManager.IsPayingAttentionToUser(tom).ShouldBeFalse();
                attentionManager.IsPayingAttentionToUser(dick).ShouldBeFalse();
                attentionManager.IsPayingAttentionToUser(harry).ShouldBeFalse();
            }
        }

        [Fact]
        public void Mofichan_Should_Be_Paying_Attention_To_User_After_Attention_Is_Renewed_To_Them()
        {
            // GIVEN an attention manager.
            var flowDriver = new ControllableFlowDriver();

            using (var attentionManager = new FlowDrivenAttentionManager(0, 1, flowDriver, Log))
            {
                // GIVEN a user.
                var john = new MockUser("John", "John");

                // WHEN attention is renewed to the user.
                attentionManager.RenewAttentionTowardsUser(john);

                // THEN Mofichan should be paying attention to that user.
                attentionManager.IsPayingAttentionToUser(john).ShouldBeTrue();
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
                var john = new MockUser("John", "John");

                // WHEN attention is renewed to the user.
                attentionManager.RenewAttentionTowardsUser(john);

                // AND we step for [lowStepBound] number of steps.
                for (; numSteps < lowStepBound; numSteps++)
                {
                    flowDriver.StepFlow();
                }

                // THEN given the attention manager works correctly, Mofichan should still be paying attention
                // to the user with 99.95% probability.
                attentionManager.IsPayingAttentionToUser(john).ShouldBeTrue();

                // WHEN we step until [highStepBound] number of steps.
                for (; numSteps < highStepBound; numSteps++)
                {
                    flowDriver.StepFlow();
                }

                // THEN given the attention manager works correctly, Mofichan should not be paying attention
                // to the user with 99.95% probability.
                attentionManager.IsPayingAttentionToUser(john).ShouldBeFalse();
            }
        }
    }
}
