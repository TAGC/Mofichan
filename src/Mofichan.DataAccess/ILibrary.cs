using System.Collections.Generic;
using Mofichan.DataAccess.Domain;

namespace Mofichan.DataAccess
{
    internal interface ILibrary
    {
        IEnumerable<TaggedMessage> Articles { get; }
    }
}
