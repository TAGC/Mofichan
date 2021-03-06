﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mofichan.DataAccess
{
    /// <summary>
    /// Represents a tag requirement.
    /// <para></para>
    /// These will typically be used to filter the kind of responses
    /// Mofichan will choose to respond with based on the tags
    /// associated with each possible response she knows about.
    /// </summary>
    internal interface ITagRequirement
    {
        /// <summary>
        /// Returns whether this <c>ITagRequirement</c> is satisfied by
        /// the provided collection of tags.
        /// </summary>
        /// <param name="tags">The tag collection.</param>
        /// <returns><c>true</c> if <c>this</c> is satisfied; otherwise, <c>false</c>.</returns>
        bool SatisfiedBy(IEnumerable<string> tags);
    }

    /// <summary>
    /// Provides static fields and methods.
    /// </summary>
    internal static class TagRequirement
    {
        internal static readonly char AndSeparator = ',';
        internal static readonly char OrSeparator = ';';

        private static readonly string TagMatch = @"[a-zA-Z0-9\-]+";
        private static readonly string AndMatcher = string.Format(@"((?<and>{0}){1})*(?<and>{0})", TagMatch, AndSeparator);
        private static readonly string OrMatcher = string.Format(@"^((?<or>{0}){1})*(?<or>{0})$", AndMatcher, OrSeparator);

        /// <summary>
        /// Parses a string and returns the represented <see cref="ITagRequirement"/>. 
        /// </summary>
        /// <param name="representation">The tag requirement string representation.</param>
        /// <returns>The represented tag requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if the representation is invalid.</exception>
        public static ITagRequirement Parse(string representation)
        {
            var root = new AnyTagRequirement(from orGroup in GetMatchesFromRegex(representation, OrMatcher, "or")
                                             let andGroup = from tag in GetMatchesFromRegex(orGroup, AndMatcher, "and")
                                                            select new LeafTagRequirement(tag)
                                             let allTagRequirement = new AllTagRequirement(andGroup)
                                             select allTagRequirement);

            return root;
        }

        private static IEnumerable<string> GetMatchesFromRegex(string input, string pattern, string matchName)
        {
            var regex = Regex.Match(input, pattern);

            if (!regex.Success)
            {
                var message = string.Format("Input '{0}' is invalid for pattern '{1}'", input, pattern);
                throw new ArgumentException(message);
            }

            var captures = regex.Groups[matchName].Captures;

            return from i in Enumerable.Range(0, captures.Count)
                   select captures[i].Value;
        }
    }

    internal abstract class CompositeTagRequirement : ITagRequirement
    {
        protected CompositeTagRequirement(IEnumerable<ITagRequirement> children)
        {
            this.Children = children;
        }

        public IEnumerable<ITagRequirement> Children { get; }

        public abstract bool SatisfiedBy(IEnumerable<string> tags);
    }

    internal sealed class AllTagRequirement : CompositeTagRequirement
    {
        public AllTagRequirement(IEnumerable<ITagRequirement> children) : base(children)
        {
        }

        public override bool SatisfiedBy(IEnumerable<string> tags)
        {
            return this.Children.All(it => it.SatisfiedBy(tags));
        }

        public override string ToString()
        {
            return string.Join(TagRequirement.AndSeparator.ToString(), this.Children);
        }
    }

    internal sealed class AnyTagRequirement : CompositeTagRequirement
    {
        public AnyTagRequirement(IEnumerable<ITagRequirement> children) : base(children)
        {
        }

        public override bool SatisfiedBy(IEnumerable<string> tags)
        {
            return this.Children.Any(it => it.SatisfiedBy(tags));
        }

        public override string ToString()
        {
            return string.Join(TagRequirement.OrSeparator.ToString(), this.Children);
        }
    }

    internal sealed class LeafTagRequirement : ITagRequirement
    {
        private readonly string requiredTag;

        public LeafTagRequirement(string tag)
        {
            this.requiredTag = tag;
        }

        public bool SatisfiedBy(IEnumerable<string> tags)
        {
            return tags.Contains(this.requiredTag);
        }

        public override string ToString()
        {
            return this.requiredTag;
        }
    }
}
