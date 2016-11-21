using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    public interface IResponseBuilder
    {
        IResponseBuilder FromRaw(string rawString);

        IResponseBuilder FromTags(params string[] tags);
        IResponseBuilder FromTags(double chance, IEnumerable<string> tags);
        IResponseBuilder FromTags(string prefix, IEnumerable<string> tags);
        IResponseBuilder FromTags(IEnumerable<string> tags, string prefix, double chance);

        IResponseBuilder FromAnyOf(params string[] phrases);
        IResponseBuilder FromAnyOf(double chance, IEnumerable<string> phrases);
        IResponseBuilder FromAnyOf(string prefix, IEnumerable<string> phrases);
        IResponseBuilder FromAnyOf(IEnumerable<string> phrases, string prefix, double chance);

        string Build();
    }
}
