using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mofichan.Core.Interfaces;

namespace Mofichan.Library
{
    internal class ResponseBuilder : IResponseBuilder
    {
        private static readonly double DefaultChance = 1.0;
        private static readonly string DefaultPrefix = " ";

        private readonly IArticleFilter articleFilter;
        private readonly StringBuilder stringBuilder;
        private readonly Random random;

        public ResponseBuilder(IArticleFilter articleFilter)
        {
            this.articleFilter = articleFilter;
            this.stringBuilder = new StringBuilder();
            this.random = new Random();
        }

        public IResponseBuilder FromRaw(string rawString)
        {
            this.stringBuilder.Append(rawString);
            return this;
        }

        public IResponseBuilder FromAnyOf(params string[] phrases)
        {
            return this.FromAnyOf(phrases, DefaultPrefix, DefaultChance);
        }

        public IResponseBuilder FromAnyOf(string prefix, IEnumerable<string> phrases)
        {
            return this.FromAnyOf(phrases, prefix, DefaultChance);
        }

        public IResponseBuilder FromAnyOf(double chance, IEnumerable<string> phrases)
        {
            return this.FromAnyOf(phrases, DefaultPrefix, chance);
        }

        public IResponseBuilder FromAnyOf(IEnumerable<string> phrases, string prefix, double chance)
        {
            var possibilities = phrases.Select(it => prefix + it);
            var part = this.PickAnyOfWithChance(possibilities, chance);
            this.stringBuilder.Append(part);

            return this;
        }

        public IResponseBuilder FromTags(params string[] tags)
        {
            return this.FromTags(tags, DefaultPrefix, DefaultChance);
        }

        public IResponseBuilder FromTags(string prefix, IEnumerable<string> tags)
        {
            return this.FromTags(tags, prefix, DefaultChance);
        }

        public IResponseBuilder FromTags(double chance, IEnumerable<string> tags)
        {
            return this.FromTags(tags, DefaultPrefix, chance);
        }

        public IResponseBuilder FromTags(IEnumerable<string> tags, string prefix, double chance)
        {
            var tagRequirement = CreateTagRequirement(tags);
            var validArticles = this.articleFilter.FilterByTagRequirement(tagRequirement);
            var possibilities = validArticles.Select(it => prefix + it);
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

            return array[this.random.Next(array.Length)];
        }
    }
}
