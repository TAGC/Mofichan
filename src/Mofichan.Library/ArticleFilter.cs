using System.Collections.Generic;
using System.Linq;

namespace Mofichan.Library
{
    /// <summary>
    /// Filters instances of <see cref="TaggedArticle"/> based on provided
    /// <see cref="ITagRequirement"/>. 
    /// </summary>
    internal interface IArticleFilter
    {
        /// <summary>
        /// Returns a collection of article bodies based on the tags they
        /// provide and the specified <see cref="ITagRequirement"/>. 
        /// </summary>
        /// <param name="requirement">The requirement the articles must satisfy.</param>
        /// <returns>A filter collection of article bodies.</returns>
        IEnumerable<string> FilterByTagRequirement(ITagRequirement requirement);
    }

    /// <summary>
    /// An instance of <see cref="IArticleFilter"/> that filters against
    /// a set of <see cref="TaggedArticle"/> taken from a collection of <see cref="ILibrary"/>.  
    /// </summary>
    internal class ArticleFilter : IArticleFilter
    {
        private readonly IEnumerable<TaggedArticle> articles;

        public ArticleFilter(IEnumerable<ILibrary> libraries)
        {
            this.articles = libraries.SelectMany(it => it.Articles);
        }

        public IEnumerable<string> FilterByTagRequirement(ITagRequirement requirement)
        {
            return from article in this.articles
                   where requirement.SatisfiedBy(article.Tags)
                   select article.Article;
        }
    }
}
