using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.DataAccess.Domain;

namespace Mofichan.DataAccess.Database
{
    internal sealed class RepositoryBasedMemoryManager : IQueryableMemoryManager
    {
        private readonly IRepository repository;

        public RepositoryBasedMemoryManager(IRepository repository)
        {
            this.repository = repository;
        }

        public IEnumerable<TaggedMessage> LoadAnalyses()
        {
            return this.repository.All<AnalysisArticle>().Select(it => it.Article);
        }

        public IEnumerable<TaggedMessage> LoadResponses()
        {
            return this.repository.All<ResponseArticle>().Select(it => it.Article);
        }

        public void SaveAnalysis(string message, IEnumerable<string> classifications)
        {
            var article = new AnalysisArticle
            {
                Article = TaggedMessage.From(message, classifications.ToArray())
            };

            this.repository.Add(article);
        }
    }
}
