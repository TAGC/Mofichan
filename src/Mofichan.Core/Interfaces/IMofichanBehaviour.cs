using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Mofichan.Core.Interfaces
{
    public interface IMofichanBehaviour :
        IPropagatorBlock<IncomingMessage, IncomingMessage>,
        IPropagatorBlock<OutgoingMessage, OutgoingMessage>,
        IDisposable
    {
        void InspectBehaviourStack(IList<IMofichanBehaviour> stack);
        string Id { get; }
        void Start();
    }
}
