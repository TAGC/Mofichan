using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Serilog;

namespace Mofichan.DataAccess.Database
{
    /// <summary>
    /// A type of <see cref="IRepository"/> that uses a MongoDb database for data storage and retrieval.
    /// </summary>
    public class MongoDbRepository : IRepository
    {
        private static readonly string DatabaseName = "mofichan";

        private readonly IMongoDatabase database;

        /// <summary>
        /// Initializes static members of the <see cref="MongoDbRepository"/> class.
        /// </summary>
        static MongoDbRepository()
        {
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCase", conventionPack, _ => true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbRepository"/> class.
        /// </summary>
        /// <param name="client">A client for connecting with a MongoDb database.</param>
        /// <param name="logger">The logger.</param>
        public MongoDbRepository(MongoClient client, ILogger logger)
        {
            this.database = client.GetDatabase(DatabaseName);

            logger.ForContext<MongoDbRepository>().Information(
                "Initialised MongoDB repository " +
                "(database server: {@MongoDbDatabaseUrl}) " +
                "(database: {DatabaseNamespace})",
                client.Settings.Server, this.database.DatabaseNamespace);
        }

        /// <summary>
        /// Stores the the specified item within this repository.
        /// </summary>
        /// <typeparam name="T">The type of item to store.</typeparam>
        /// <param name="item">The item to store.</param>
        public void Add<T>(T item)
        {
            this.GetCollection<T>().InsertOne(item);
        }

        /// <summary>
        /// Retrieves all instances of the specified type stored within this repository.
        /// </summary>
        /// <typeparam name="T">The type of item to retrieve.</typeparam>
        /// <returns>
        /// A collection of items of the given type.
        /// </returns>
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
