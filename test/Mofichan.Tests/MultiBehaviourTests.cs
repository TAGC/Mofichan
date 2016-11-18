using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class MultiBehaviourTests
    {
        #region Mocks
        private class MockSubBehaviour : IMofichanBehaviour
        {
            private readonly Action onNextCallback;

            public IObserver<OutgoingMessage> UpstreamObserver { get; private set; }
            public IObserver<IncomingMessage> DownstreamObserver { get; private set; }

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

            public IDisposable Subscribe(IObserver<IncomingMessage> observer)
            {
                this.DownstreamObserver = observer;
                return null;
            }

            public IDisposable Subscribe(IObserver<OutgoingMessage> observer)
            {
                this.UpstreamObserver = observer;
                return null;
            }

            public void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
            {
                throw new NotImplementedException();
            }

            public void Start()
            {
                throw new NotImplementedException();
            }

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext(IncomingMessage value)
            {
                this.onNextCallback();
            }

            public void OnNext(OutgoingMessage value)
            {
                this.onNextCallback();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
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
        public void Multi_Behaviour_Should_Subscribe_Upstream_Observers_To_Most_Upstream_Sub_Behaviour()
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

            // GIVEN an upstream observer to subscribe.
            var upstreamObserver = Mock.Of<IObserver<OutgoingMessage>>();

            // WHEN we try to subscribe the observer to the multi-behaviour.
            multiBehaviour.Subscribe(upstreamObserver);

            // THEN it should have been subscribed to the most upstream sub-behaviour (and none others).
            var mostUpstreamSubBehaviour = subBehaviours.First();

            mostUpstreamSubBehaviour.UpstreamObserver.ShouldBe(upstreamObserver);
            subBehaviours
                .Skip(1)
                .Select(it => it.UpstreamObserver)
                .ShouldNotContain(upstreamObserver);
        }

        [Fact]
        public void Multi_Behaviour_Should_Subscribe_Downstream_Observers_To_Most_Downstream_Sub_Behaviour()
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

            // GIVEN a downstream observer to subscribe.
            var downstreamObserver = Mock.Of<IObserver<IncomingMessage>>();

            // WHEN we try to subscribe the observer to the multi-behaviour.
            multiBehaviour.Subscribe(downstreamObserver);

            // THEN it should have been subscribed to the most downstream sub-behaviour (and none others).
            var mostDownstreamSubBehaviour = subBehaviours.Last();

            mostDownstreamSubBehaviour.DownstreamObserver.ShouldBe(downstreamObserver);
            subBehaviours
                .Take(subBehaviours.Length - 1)
                .Select(it => it.DownstreamObserver)
                .ShouldNotContain(downstreamObserver);
        }
    }
}
