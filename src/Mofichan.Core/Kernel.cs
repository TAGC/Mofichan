using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core
{
    public class Kernel : IMessageContextHandler
    {
        private IMofichanBackend backend;
        private IMofichanBehaviour[] behaviours;

        public Kernel(string name, IMofichanBackend backend, IEnumerable<IMofichanBehaviour> behaviours)
        {
            Raise.ArgumentNullException.IfIsNull(behaviours, nameof(behaviours));
            Raise.ArgumentException.IfNot(behaviours.Any(), nameof(behaviours),
                string.Format("At least one behaviour must be specified for {0}", name));

            this.backend = backend;
            this.behaviours = behaviours.ToArray();

            this.backend.LinkTo(this.behaviours[0]);
        }

        #region Dataflow Methods
        public Task Completion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Start()
        {
            this.backend.Start();
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public MessageContext ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<MessageContext> target,
            out bool messageConsumed)
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

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, MessageContext messageValue,
            ISourceBlock<MessageContext> source, bool consumeToAccept)
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
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
