using System.Collections.Generic;

namespace Mofichan.DataAccess
{
    internal interface ILibrary
    {
        IEnumerable<TaggedMessage> Articles { get; }
    }
}
