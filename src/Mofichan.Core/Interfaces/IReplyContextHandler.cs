using System;
using System.Threading.Tasks.Dataflow;

namespace Mofichan.Core.Interfaces
{
    public interface IReplyContextHandler : IPropagatorBlock<ReplyContext, ReplyContext>, IDisposable
    {
        void Start();
    }
}
