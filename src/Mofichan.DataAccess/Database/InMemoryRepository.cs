using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Mofichan.DataAccess.Database
{
    public class InMemoryRepository : IRepository
    {
        private readonly IList<object> objects;

        public InMemoryRepository(ILogger logger)
        {
            this.objects = new List<object>();

            logger.ForContext<InMemoryRepository>().Information(
                "Instantiated in-memory repository");
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
