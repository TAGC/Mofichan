using System.Collections.Generic;

namespace Mofichan.Library.Response
{
    internal interface ILibrary
    {
        IEnumerable<TaggedMessage> Articles { get; }
    }
}
