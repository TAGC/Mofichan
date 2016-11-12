using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Mofichan.Core.Interfaces
{
    public interface IMofichanBehaviour :
        IPropagatorBlock<IncomingMessage, IncomingMessage>,
        IPropagatorBlock<OutgoingMessage, OutgoingMessage>,
        IDisposable
    {
    }
}
