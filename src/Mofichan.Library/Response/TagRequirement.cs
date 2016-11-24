using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;

namespace Mofichan.Library.Response
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
        bool SatisfiedBy(IEnumerable<Tag> tags);
    }

    /// <summary>
    /// Provides static fields and methods.
    /// </summary>
    internal static class TagRequirement
    {
        internal static readonly char AndSeparator = ',';
        internal static readonly char OrSeparator = ';';

        public static ITagRequirement From(IEnumerable<IEnumerable<Tag>> tags)
        {
            var root = new AnyTagRequirement(from orGroup in tags
                                             let andGroup = from tag in orGroup
                                                            select new LeafTagRequirement(tag)
                                             let allTagRequirement = new AllTagRequirement(andGroup)
                                             select allTagRequirement);

            return root;
        }
    }

    internal abstract class CompositeTagRequirement : ITagRequirement
    {
        protected CompositeTagRequirement(IEnumerable<ITagRequirement> children)
        {
            this.Children = children;
        }

        public IEnumerable<ITagRequirement> Children { get; }

        public abstract bool SatisfiedBy(IEnumerable<Tag> tags);
    }

    internal sealed class AllTagRequirement : CompositeTagRequirement
    {
        public AllTagRequirement(IEnumerable<ITagRequirement> children) : base(children)
        {
        }

        public override bool SatisfiedBy(IEnumerable<Tag> tags)
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

        public override bool SatisfiedBy(IEnumerable<Tag> tags)
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
        private readonly Tag requiredTag;

        public LeafTagRequirement(Tag tag)
        {
            this.requiredTag = tag;
        }

        public bool SatisfiedBy(IEnumerable<Tag> tags)
        {
            return tags.Contains(this.requiredTag);
        }

        public override string ToString()
        {
            return this.requiredTag.ToString();
        }
    }
}
