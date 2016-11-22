using System;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    public class MofichanIsStartedAndStopped : BaseScenario
    {
        private readonly PropagationTestingBehaviour[] mockBehaviours;

        public MofichanIsStartedAndStopped() : base("Mofichan is started and stopped")
        {
            this.mockBehaviours = new[] {
                new PropagationTestingBehaviour(1),
                new PropagationTestingBehaviour(2),
                new PropagationTestingBehaviour(3),
            };

            this.mockBehaviours.ShouldAllBe(it => !it.Started && !it.Completed);

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.mockBehaviours[0]))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.mockBehaviours[1]))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(this.mockBehaviours[2]))
                    .And(s => s.Given_Mofichan_is_running())
                .Then(s => s.Then_the_behaviours_in_the_chain_should_all_have_started())
                .When(s => s.When_mofichan_is_disposed())
                .Then(s => s.Then_the_behaviours_in_the_chain_should_all_have_completed());
        }

        private void When_mofichan_is_disposed()
        {
            this.Mofichan.Dispose();
        }

        private void Then_the_behaviours_in_the_chain_should_all_have_started()
        {
            this.mockBehaviours.ShouldAllBe(it => it.Started);
        }

        private void Then_the_behaviours_in_the_chain_should_all_have_completed()
        {
            this.mockBehaviours.ShouldAllBe(it => it.Completed);
        }

        private class PropagationTestingBehaviour : BaseBehaviour
        {
            public PropagationTestingBehaviour(int id) : base(() => Mock.Of<IResponseBuilder>())
            {
                this.Id = "Propagation behaviour " + id;
            }

            public override string Id { get; }

            public bool Completed { get; private set; }

            public bool Started { get; private set; }

            public override void Start()
            {
                base.Start();

                this.Started = true;
            }

            public override void OnCompleted()
            {
                base.OnCompleted();

                this.Completed = true;
            }

            protected override bool CanHandleIncomingMessage(IncomingMessage message)
            {
                return false;
            }

            protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
            {
                return false;
            }

            protected override void HandleIncomingMessage(IncomingMessage message)
            {
                throw new NotImplementedException();
            }

            protected override void HandleOutgoingMessage(OutgoingMessage message)
            {
                throw new NotImplementedException();
            }
        }
    }
}
