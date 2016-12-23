using System.Collections.Generic;

namespace Mofichan.DataAccess
{
    internal interface IRepository
    {
        void Add<T>(T item);

        IEnumerable<T> All<T>();
    }
}
