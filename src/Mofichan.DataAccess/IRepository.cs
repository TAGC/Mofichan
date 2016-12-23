using System.Collections.Generic;

namespace Mofichan.DataAccess
{
    /// <summary>
    /// Represents a repository for storing objects of various types.
    /// </summary>
    internal interface IRepository
    {
        /// <summary>
        /// Stores the the specified item within this repository.
        /// </summary>
        /// <typeparam name="T">The type of item to store.</typeparam>
        /// <param name="item">The item to store.</param>
        void Add<T>(T item);

        /// <summary>
        /// Retrieves all instances of the specified type stored within this repository.
        /// </summary>
        /// <typeparam name="T">The type of item to retrieve.</typeparam>
        /// <returns>A collection of items of the given type.</returns>
        IEnumerable<T> All<T>();
    }
}
