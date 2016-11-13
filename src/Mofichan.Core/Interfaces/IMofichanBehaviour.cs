using System;
using System.Threading.Tasks.Dataflow;

namespace Mofichan.Core.Interfaces
{
    public interface IMofichanBehaviour :
        IPropagatorBlock<IncomingMessage, IncomingMessage>,
        IPropagatorBlock<OutgoingMessage, OutgoingMessage>,
        IDisposable
    {
        string Id { get; }
        void Start();
    }
}
