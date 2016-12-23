using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.DataAccess.Response;

namespace Mofichan.DataAccess
{
    internal class ResponseBodyBuilder : IResponseBodyBuilder
    {
        private static readonly double DefaultChance = 1.0;
        private static readonly string DefaultPrefix = " ";

        private readonly IArticleFilter articleFilter;
        private readonly IArticleResolver articleResolver;
        private readonly StringBuilder stringBuilder;
        private readonly Random random;
        private readonly dynamic context;

        public ResponseBodyBuilder(IArticleFilter articleFilter, IArticleResolver articleResolver)
        {
            this.articleFilter = articleFilter;
            this.articleResolver = articleResolver;
            this.stringBuilder = new StringBuilder();
            this.random = new Random();
            this.context = new ExpandoObject();
        }

        public IResponseBodyBuilder UsingContext(MessageContext messageContext)
        {
            var fromUser = messageContext.From as IUser;
            var toUser = messageContext.To as IUser;
            var body = messageContext.Body;

            this.context.message = new ExpandoObject();
            this.context.message.body = body;

            if (fromUser != null)
            {
                this.context.message.from = new ExpandoObject();
                this.context.message.from.name = fromUser.Name;
            }

            if (toUser != null)
            {
                this.context.message.to = new ExpandoObject();
                this.context.message.to.name = toUser.Name;
            }

            return this;
        }

        public IResponseBodyBuilder FromRaw(string rawString)
        {
            this.stringBuilder.Append(rawString);
            return this;
        }

        public IResponseBodyBuilder FromFormatted(string format, params object[] args)
        {
            this.stringBuilder.Append(string.Format(format, args));
            return this;
        }

        public IResponseBodyBuilder FromAnyOf(params string[] phrases)
        {
            return this.FromAnyOf(DefaultPrefix, DefaultChance, phrases);
        }

        public IResponseBodyBuilder FromAnyOf(string prefix, IEnumerable<string> phrases)
        {
            return this.FromAnyOf(prefix, DefaultChance, phrases);
        }

        public IResponseBodyBuilder FromAnyOf(double chance, IEnumerable<string> phrases)
        {
            return this.FromAnyOf(DefaultPrefix, chance, phrases);
        }

        public IResponseBodyBuilder FromAnyOf(string prefix, double chance, IEnumerable<string> phrases)
        {
            var possibilities = phrases.Select(it => prefix + it);
            var part = this.PickAnyOfWithChance(possibilities, chance);
            this.stringBuilder.Append(part);

            return this;
        }

        public IResponseBodyBuilder FromTags(params string[] tags)
        {
            return this.FromTags(DefaultPrefix, DefaultChance, tags);
        }

        public IResponseBodyBuilder FromTags(string prefix, IEnumerable<string> tags)
        {
            return this.FromTags(prefix, DefaultChance, tags);
        }

        public IResponseBodyBuilder FromTags(double chance, IEnumerable<string> tags)
        {
            return this.FromTags(DefaultPrefix, chance, tags);
        }

        public IResponseBodyBuilder FromTags(string prefix, double chance, IEnumerable<string> tags)
        {
            var tagRequirement = CreateTagRequirement(tags);
            var validArticles = this.articleFilter.FilterByTagRequirement(tagRequirement);

            IList<string> resolvedArticles = new List<string>();

            foreach (var article in validArticles)
            {
                try
                {
                    string resolvedArticle = this.articleResolver.Resolve(article, this.context);
                    resolvedArticles.Add(resolvedArticle);
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }

            var possibilities = resolvedArticles.Select(it => prefix + it);
            var part = this.PickAnyOfWithChance(possibilities, chance);
            this.stringBuilder.Append(part);

            return this;
        }

        public string Build()
        {
            return this.stringBuilder.ToString();
        }

        private static ITagRequirement CreateTagRequirement(IEnumerable<string> tags)
        {
            var representation = string.Join(";", tags);

            return TagRequirement.Parse(representation);
        }

        private string PickAnyOfWithChance(IEnumerable<string> possibilities, double chance)
        {
            if (this.random.NextDouble() < chance)
            {
                return this.PickAnyOf(possibilities);
            }

            return string.Empty;
        }

        private string PickAnyOf(IEnumerable<string> possibilities)
        {
            var array = possibilities.ToArray();

            if (!array.Any())
            {
                return string.Empty;
            }

            return array[this.random.Next(array.Length)];
        }
    }
}
