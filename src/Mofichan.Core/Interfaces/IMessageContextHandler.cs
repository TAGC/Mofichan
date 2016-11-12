using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Mofichan.Core.Interfaces
{
    public interface IMessageContextHandler : IPropagatorBlock<MessageContext, MessageContext>, IDisposable
    {
        void Start();
    }
}
