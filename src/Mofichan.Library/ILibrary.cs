using System.Collections.Generic;

namespace Mofichan.Library
{
    internal interface ILibrary
    {
        IEnumerable<TaggedArticle> Articles { get; }
    }
}
