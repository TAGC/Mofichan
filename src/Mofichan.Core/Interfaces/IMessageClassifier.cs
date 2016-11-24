using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    public interface IMessageClassifier
    {
        IEnumerable<Tag> Classify(string message);
    }
}
