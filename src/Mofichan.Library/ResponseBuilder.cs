using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Library
{
    internal class ResponseBuilder : IResponseBuilder
    {
        private readonly IArticleFilter articleFilter;

        public ResponseBuilder(IArticleFilter articleFilter)
        {
            this.articleFilter = articleFilter;
        }

        public IResponseBuilder With(params string[] tags)
        {
            throw new NotImplementedException();
        }

        public IResponseBuilder With(double chance, params string[] tags)
        {
            throw new NotImplementedException();
        }

        public IResponseBuilder WithAnyOf(params string[] phrases)
        {
            throw new NotImplementedException();
        }

        public IResponseBuilder WithAnyOf(double chance, params string[] phrases)
        {
            throw new NotImplementedException();
        }

        public string Build()
        {
            throw new NotImplementedException();
        }
    }
}
