using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mofichan.Core.Interfaces
{
    public interface IMessageTarget
    {
        void ReceiveMessage(string message);
    }
}
