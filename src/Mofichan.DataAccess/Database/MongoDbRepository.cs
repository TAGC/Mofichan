using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Mofichan.DataAccess.Database
{
    public class MongoDbRepository : IRepository
    {
        static MongoDbRepository()
        {
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", conventionPack, _ => true);
        }

        private static readonly string DatabaseName = "mofichan";

        private readonly IMongoDatabase database;

        public MongoDbRepository(MongoClient client, ILogger logger)
        {
            this.database = client.GetDatabase(DatabaseName);

            logger.ForContext<MongoDbRepository>().Information(
                "Initialised MongoDB repository " +
                "(database server: {@MongoDbDatabaseUrl}) " +
                "(database: {DatabaseNamespace})",
                client.Settings.Server, this.database.DatabaseNamespace);
        }

        public void Add<T>(T item)
        {
            this.GetCollection<T>().InsertOne(item);
        }

        public IEnumerable<T> All<T>()
        {
            return this.GetCollection<T>().AsQueryable();
        }

        private IMongoCollection<T> GetCollection<T>()
        {
            return this.database.GetCollection<T>(typeof(T).GetTypeInfo().Name);
        }
    }
}
