using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Flow;
using Mofichan.Core.Flow;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class FairFlowTransitionSelectorTests
    {
        [Fact]
        public void Fair_Flow_Transition_Selector_Should_Select_Transitions_Fairly_From_Weights()
        {
            // GIVEN a fair flow transition selector.
            var transitionSelector = new FairFlowTransitionSelector();

            // GIVEN transitions with various weights.
            var transitions = new[]
            {
                new FlowTransition("A") { Weight = 0.3 },
                new FlowTransition("B") { Weight = 0.6 },
                new FlowTransition("C") { Weight = 0.1 }
            };

            // EXPECT that the transition selector picks fairly from the selection.
            var selectedTransitions = new List<string>();
            Action performSelection = () =>
            {
                selectedTransitions.Add(transitionSelector.Select(transitions).Id);
            };

            /*
             * Generate observations.
             */
            for (var i = 0; i < 1000; i++)
            {
                performSelection();
            }

            TestHypothesis(selectedTransitions).ShouldBeTrue(
                "Transition selector is incorrectly implemented with 99.5% probability");
        }

        private bool TestHypothesis(IEnumerable<string> observedSelections)
        {
            /*
             * Hypothesis: 30% of selected transitions will be "A", 60% "B" and 10% "C".
             * 
             * We want to be 99.5% certain that the transition selector is implemented incorrectly
             * if this test fails.
             */
            const double criticalValue = 10.597; // 2 degrees of freedom, 0.005 signicance level

            var numObservedSelections = observedSelections.Count();

            var expectedA = (int)Math.Round(numObservedSelections * 0.3);
            var expectedB = (int)Math.Round(numObservedSelections * 0.6);
            var expectedC = (int)Math.Round(numObservedSelections * 0.1);

            var actualA = observedSelections.Count(it => it == "A");
            var actualB = observedSelections.Count(it => it == "B");
            var actualC = observedSelections.Count(it => it == "C");

            Func<double, double, double> f = (e, a) => Math.Pow(e - a, 2) / e;

            double chiSquared = f(expectedA, actualA) + f(expectedB, actualB) + f(expectedC, actualC);

            return chiSquared < criticalValue;
        }
    }
}
