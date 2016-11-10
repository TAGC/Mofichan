using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using TestStack.BDDfy;

namespace Mofichan.Spec.Core.Feature
{
    [Story(Title = "Core Functionality")]
    public abstract class BaseScenario : Scenario
    {
        protected const string AddBehaviourTemplate = "Given Mofichan is configured to use behaviour '{0}'";

        private readonly string scenarioTitle;

        public BaseScenario(string scenarioTitle = null) : base(scenarioTitle: scenarioTitle)
        {
            this.scenarioTitle = scenarioTitle;
            this.Backend = new MockBackend("mock");
            this.Behaviours = new List<IMofichanBehaviour>();
        }

        protected IList<IMofichanBehaviour> Behaviours { get; }
        protected IMofichanBackend Backend { get; }
        protected Kernel Mofichan { get; private set; }

        protected void Given_Mofichan_is_configured_with_behaviour(string behaviour)
        {
            this.Behaviours.Add(new MockBehaviour(behaviour));
        }

        protected void Given_Mofichan_is_running()
        {
            this.Mofichan = new Kernel(this.Backend, this.Behaviours);
        }

        #region Temporary
        public class MockBehaviour : IMofichanBehaviour
        {
            private readonly string name;

            public MockBehaviour(string name)
            {
                this.name = name;
            }

            public Task Completion
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

            public MessageContext ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target, out bool messageConsumed)
            {
                throw new NotImplementedException();
            }

            public void Fault(Exception exception)
            {
                throw new NotImplementedException();
            }

            public IDisposable LinkTo(ITargetBlock<MessageContext> target, DataflowLinkOptions linkOptions)
            {
                throw new NotImplementedException();
            }

            public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, MessageContext messageValue, ISourceBlock<MessageContext> source, bool consumeToAccept)
            {
                throw new NotImplementedException();
            }

            public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target)
            {
                throw new NotImplementedException();
            }

            public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target)
            {
                throw new NotImplementedException();
            }
        }

        public class MockBackend : IMofichanBackend
        {
            private readonly string name;

            public MockBackend(string name)
            {
                this.name = name;
            }

            public Task Completion
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

            public MessageContext ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target, out bool messageConsumed)
            {
                throw new NotImplementedException();
            }

            public void Fault(Exception exception)
            {
                throw new NotImplementedException();
            }

            public IDisposable LinkTo(ITargetBlock<MessageContext> target, DataflowLinkOptions linkOptions)
            {
                throw new NotImplementedException();
            }

            public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, MessageContext messageValue, ISourceBlock<MessageContext> source, bool consumeToAccept)
            {
                throw new NotImplementedException();
            }

            public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target)
            {
                throw new NotImplementedException();
            }

            public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
