using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
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
            public ITargetBlock<OutgoingMessage> UpstreamTarget { get; private set; }
            public ITargetBlock<IncomingMessage> DownstreamTarget { get; private set; }

            public IDisposable LinkTo(ITargetBlock<IncomingMessage> target,
                DataflowLinkOptions linkOptions)
            {
                this.DownstreamTarget = target;
                return null;
            }

            public IDisposable LinkTo(ITargetBlock<OutgoingMessage> target,
                DataflowLinkOptions linkOptions)
            {
                this.UpstreamTarget = target;
                return null;
            }

            public Task Completion
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Id
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void Complete()
            {
                throw new NotImplementedException();
            }

            public OutgoingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target, out bool messageConsumed)
            {
                throw new NotImplementedException();
            }

            public IncomingMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target, out bool messageConsumed)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Fault(Exception exception)
            {
                throw new NotImplementedException();
            }

            public void InspectBehaviourStack(IList<IMofichanBehaviour> stack)
            {
                throw new NotImplementedException();
            }

            public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, OutgoingMessage messageValue, ISourceBlock<OutgoingMessage> source, bool consumeToAccept)
            {
                throw new NotImplementedException();
            }

            public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, IncomingMessage messageValue, ISourceBlock<IncomingMessage> source, bool consumeToAccept)
            {
                throw new NotImplementedException();
            }

            public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
            {
                throw new NotImplementedException();
            }

            public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
            {
                throw new NotImplementedException();
            }

            public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<OutgoingMessage> target)
            {
                throw new NotImplementedException();
            }

            public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<IncomingMessage> target)
            {
                throw new NotImplementedException();
            }

            public void Start()
            {
                throw new NotImplementedException();
            }
        }

        private class MockMultiBehaviour : BaseMultiBehaviour
        {
            public MockMultiBehaviour(params IMofichanBehaviour[] subBehaviours)
                : base(subBehaviours)
            {
            }

            public MockMultiBehaviour(IEnumerable<IMofichanBehaviour> subBehaviours)
                : base(subBehaviours.ToArray())
            {
            }

            public override string Id
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }
        #endregion

        [Fact]
        public void Multi_Behaviour_Should_Link_Upstream_Targets_To_Most_Upstream_Sub_Behaviour()
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

            // GIVEN an upstream target to link to.
            var upstreamTarget = Mock.Of<ITargetBlock<OutgoingMessage>>();

            // WHEN we try to link a target to the multi-behaviour.
            multiBehaviour.LinkTo(upstreamTarget);

            // THEN it should have been linked to the most upstream sub-behaviour (and none others).
            var mostUpstreamSubBehaviour = subBehaviours.First();

            mostUpstreamSubBehaviour.UpstreamTarget.ShouldBe(upstreamTarget);
            subBehaviours
                .Skip(1)
                .Select(it => it.UpstreamTarget)
                .ShouldNotContain(upstreamTarget);
        }

        [Fact]
        public void Multi_Behaviour_Should_Link_Downstream_Targets_To_Most_Downstream_Sub_Behaviour()
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

            // GIVEN a downstream target to link to.
            var downstreamTarget = Mock.Of<ITargetBlock<IncomingMessage>>();

            // WHEN we try to link a target to the multi-behaviour.
            multiBehaviour.LinkTo(downstreamTarget);

            // THEN it should have been linked to the most downstream sub-behaviour (and none others).
            var mostDownstreamSubBehaviour = subBehaviours.Last();

            mostDownstreamSubBehaviour.DownstreamTarget.ShouldBe(downstreamTarget);
            subBehaviours
                .Take(subBehaviours.Length-1)
                .Select(it => it.DownstreamTarget)
                .ShouldNotContain(downstreamTarget);
        }

        [Fact]
        public void Multi_Behaviour_Should_Internally_Link_Sub_Behaviours()
        {
            // GIVEN a collection of mock sub-behaviours.
            var first = new MockSubBehaviour();
            var second = new MockSubBehaviour();
            var third = new MockSubBehaviour();

            // WHEN we construct a multi-behaviour using these sub-behaviours.
            var multiBehaviour = new MockMultiBehaviour(first, second, third);

            // THEN the sub-behaviours should be linked appropriately.
            first.DownstreamTarget.ShouldBe(second);
            second.DownstreamTarget.ShouldBe(third);

            third.UpstreamTarget.ShouldBe(second);
            second.UpstreamTarget.ShouldBe(first);
        }
    }
}
