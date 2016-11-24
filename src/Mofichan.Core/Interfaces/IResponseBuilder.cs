using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    public interface IResponseBuilder
    {
        IResponseBuilder UsingContext(MessageContext messageContext);

        IResponseBuilder FromRaw(string rawString);

        IResponseBuilder FromTags(IEnumerable<IEnumerable<Tag>> tags);
        IResponseBuilder FromTags(double chance, IEnumerable<IEnumerable<Tag>> tags);
        IResponseBuilder FromTags(string prefix, IEnumerable<IEnumerable<Tag>> tags);
        IResponseBuilder FromTags(string prefix, double chance, IEnumerable<IEnumerable<Tag>> tags);

        IResponseBuilder FromAnyOf(params string[] phrases);
        IResponseBuilder FromAnyOf(double chance, IEnumerable<string> phrases);
        IResponseBuilder FromAnyOf(string prefix, IEnumerable<string> phrases);
        IResponseBuilder FromAnyOf(string prefix, double chance, IEnumerable<string> phrases);

        string Build();
    }
}
