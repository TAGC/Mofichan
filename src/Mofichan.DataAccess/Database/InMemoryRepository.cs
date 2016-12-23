using System.Collections.Generic;
using System.Linq;

namespace Mofichan.DataAccess.Database
{
    public class InMemoryRepository : IRepository
    {
        private readonly IList<object> objects;

        public InMemoryRepository()
        {
            this.objects = new List<object>();
        }

        public void Add<T>(T item)
        {
            this.objects.Add(item);
        }

        public IEnumerable<T> All<T>()
        {
            return this.objects.OfType<T>();
        }
    }
}
