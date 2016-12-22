using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Behaviour
{
    public class MultiBehaviourTests
    {
        #region Mocks
        private class MockSubBehaviour : IMofichanBehaviour
        {
            private readonly Action onNextCallback;

            public IObserver<IBehaviourVisitor> Observer { get; private set; }

            public MockSubBehaviour(Action onNextCallback = null)
            {
                this.onNextCallback = onNextCallback ?? new Action(() => { });
            }

            public string Id
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool Started { get; private set; }
            public bool Completed { get; private set; }

            public IDisposable Subscribe(IObserver<IBehaviourVisitor> observer)
            {
                this.Observer = observer;
                return null;
            }

            public void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
            {
                throw new NotImplementedException();
            }

            public void Start()
            {
                this.Started = true;
            }

            public void OnCompleted()
            {
                this.Completed = true;
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext(IBehaviourVisitor value)
            {
                this.onNextCallback();
            }
        }

        private class MockMultiBehaviour : BaseMultiBehaviour
        {
            public MockMultiBehaviour(params IMofichanBehaviour[] subBehaviours)
                : base(ChainBuilder, subBehaviours)
            {
            }

            public override string Id
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            private static IBehaviourChainBuilder ChainBuilder
            {
                get
                {
                    return new BehaviourChainBuilder();
                }
            }
        }
        #endregion

        [Fact]
        public void Multi_Behaviour_Should_Signal_Start_To_All_Sub_Behaviours_When_Started()
        {
            // GIVEN a collection of mock sub-behaviours.
            var subBehaviours = new[]
            {
                new MockSubBehaviour(),
                new MockSubBehaviour(),
                new MockSubBehaviour(),
            };

            // GIVEN a multi-behaviour containing these sub-behaviours.
            var multiBehaviour = new MockMultiBehaviour(subBehaviours);
            subBehaviours.ShouldAllBe(it => !it.Started && !it.Completed);

            // WHEN we start the multi-behaviour.
            multiBehaviour.Start();

            // THEN the sub-behaviours should have each started.
            subBehaviours.ShouldAllBe(it => it.Started);
        }

        [Fact]
        public void Multi_Behaviour_Should_Signal_Completion_To_All_Sub_Behaviours_When_Completed()
        {
            // GIVEN a collection of mock sub-behaviours.
            var subBehaviours = new[]
            {
                new MockSubBehaviour(),
                new MockSubBehaviour(),
                new MockSubBehaviour(),
            };

            // GIVEN a multi-behaviour containing these sub-behaviours.
            var multiBehaviour = new MockMultiBehaviour(subBehaviours);
            subBehaviours.ShouldAllBe(it => !it.Started && !it.Completed);

            // WHEN we signal completion to the multi-behaviour.
            multiBehaviour.OnCompleted();

            // THEN the sub-behaviours should have each have been signalled completion.
            subBehaviours.ShouldAllBe(it => it.Completed);
        }

        [Fact]
        public void Multi_Behaviour_Should_Subscribe_Observers_To_Most_Downstream_Sub_Behaviour()
        {
            // GIVEN a collection of mock sub-behaviours.
            var subBehaviours = new[]
            {
                new MockSubBehaviour(),
                new MockSubBehaviour(),
                new MockSubBehaviour(),
            };

            // GIVEN a multi-behaviour containing these sub-behaviours.
            var multiBehaviour = new MockMultiBehaviour(subBehaviours);

            // GIVEN an observer to subscribe.
            var observer = Mock.Of<IObserver<IBehaviourVisitor>>();

            // WHEN we try to subscribe the observer to the multi-behaviour.
            multiBehaviour.Subscribe(observer);

            // THEN it should have been subscribed to the most downstream sub-behaviour (and none others).
            var mostDownstreamSubBehaviour = subBehaviours.Last();

            mostDownstreamSubBehaviour.Observer.ShouldBe(observer);
            subBehaviours
                .Take(subBehaviours.Length - 1)
                .Select(it => it.Observer)
                .ShouldNotContain(observer);
        }
    }
}
