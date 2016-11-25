using System.Collections.Generic;
using System.Linq;

namespace Mofichan.Core
{
    using AndGroup = IEnumerable<Tag>;
    using OrGroup = IEnumerable<IEnumerable<Tag>>;
    using TagGroup = IEnumerable<IEnumerable<Tag>>;

    /// <summary>
    /// An enumeration of the possible classifications that can be
    /// used to describe a message. 
    /// </summary>
    /// <remarks>
    /// Associating tags with messages makes it easier for Mofichan to
    /// both interpret incoming messages and generate responses.
    /// </remarks>
    public enum Tag
    {
        /// <summary>
        /// Represents messages directed at Mofichan.
        /// </summary>
        DirectedAtMofichan,

        /// <summary>
        /// Represents inquiries or responses about someone's wellbeing.
        /// </summary>
        Wellbeing,

        /// <summary>
        /// Reprsents messages that constitute phrases.
        /// </summary>
        Phrase,

        /// <summary>
        /// Represents messages that constitute emotes.
        /// </summary>
        Emote,

        /// <summary>
        /// Represents messages that give the impression of happiness.
        /// </summary>
        Happy,

        /// <summary>
        /// Represents messages that can be judged as cute.
        /// </summary>
        Cute,

        /// <summary>
        /// Represents messages that serve as greetings.
        /// </summary>
        Greeting,

        /// <summary>
        /// Represents messages that serve as goodbyes.
        /// </summary>
        Goodbye,

        /// <summary>
        /// Represents messages with a positive emotive quality.
        /// </summary>
        Positive,

        /// <summary>
        /// Represents messages with a negative emotive quality.
        /// </summary>
        Negative,

        /// <summary>
        /// A tag associated with messages for testing purposes.
        /// </summary>
        Test
    }

    /// <summary>
    /// Provides extension methods on <see cref=Tag/>
    /// </summary>
    public static class TagExtensions
    {
        #region As Group
        /// <summary>
        /// Returns a single tag as a <c>TagGroup</c>. 
        /// </summary>
        /// <param name="tag">The single member to compose the group.</param>
        /// <returns>A <c>TagGroup</c> containing <paramref name="tag"/> and nothing else.</returns>
        public static TagGroup AsGroup(this Tag tag)
        {
            return new[] { new[] { tag } };
        }

        /// <summary>
        /// Returns an 'And'ed collection of tags as a <c>TagGroup</c>.
        /// </summary>
        /// <param name="andGroup">The 'And'ed group of tags.</param>
        /// <returns>A <c>TagGroup</c> containing the provided sub-group.</returns>
        public static TagGroup AsGroup(this AndGroup andGroup)
        {
            return new[] { andGroup };
        }

        /// <summary>
        /// Returns an 'Or'd collection of tags as a <c>TagGroup</c>.
        /// <para></para>
        /// As an 'Or'd tag collection is a <c>TagGroup</c>, this method will
        /// return the provided group as is.
        /// </summary>
        /// <param name="orGroup">The Or'd group of tags.</param>
        /// <returns>The provided tag group as is.</returns>
        public static TagGroup AsGroup(this OrGroup orGroup)
        {
            return orGroup;
        }
        #endregion

        #region And
        /// <summary>
        /// Couples two tags together to represent a requirement for <i>both</i>
        /// requirements to be satisfied.
        /// </summary>
        /// <param name="tag">The first member of the <c>AndGroup</c>.</param>
        /// <param name="otherTag">The second member of the <c>AndGroup</c>.</param>
        /// <returns>An <c>AndGroup</c> containing the two provided tags.</returns>
        public static AndGroup And(this Tag tag, Tag otherTag)
        {
            return new[] { tag, otherTag };
        }

        /// <summary>
        /// Couples a tag and an <c>AndGroup</c> together to represent a requirement for <i>all</i>
        /// tags in the <c>AndGroup</c> as well as <paramref name="tag"/> to be be satisfied.
        /// </summary>
        /// <param name="tag">The tag to include within <paramref name="andGroup"/>.</param>
        /// <param name="andGroup">An existing <c>AndGroup</c>.</param>
        /// <returns>
        /// An <c>AndGroup</c> based on <paramref name="andGroup"/> that includes <paramref name="tag"/>
        /// </returns>
        public static AndGroup And(this Tag tag, AndGroup andGroup)
        {
            return andGroup.Prepend(tag);
        }

        /// <summary>
        /// Couples a tag and an <c>AndGroup</c> together to represent a requirement for <i>all</i>
        /// tags in the <c>AndGroup</c> as well as <paramref name="tag"/> to be be satisfied.
        /// </summary>
        /// <param name="andGroup">An existing <c>AndGroup</c>.</param>
        /// <param name="tag">The tag to include within <paramref name="andGroup"/>.</param>
        /// <returns>
        /// An <c>AndGroup</c> based on <paramref name="andGroup"/> that includes <paramref name="tag"/>
        /// </returns>
        public static AndGroup And(this AndGroup andGroup, Tag tag)
        {
            return andGroup.Append(tag);
        }

        /// <summary>
        /// Couples two <c>AndGroup</c> instances together to represent a requirement for all
        /// tags within each group to be satisfied.
        /// </summary>
        /// <param name="andGroup">The first <c>AndGroup</c>.</param>
        /// <param name="otherAndGroup">The second <c>AndGroup</c>.</param>
        /// <returns>An <c>AndGroup</c> formed by merging the two provided groups.</returns>
        public static AndGroup And(this AndGroup andGroup, AndGroup otherAndGroup)
        {
            return andGroup.Concat(otherAndGroup);
        }
        #endregion

        #region Or
        /// <summary>
        /// Couples two tags together to represent a requirement for <i>either</i>
        /// requirement to be satisfied.
        /// </summary>
        /// <param name="tag">The first member of the <c>OrGroup</c>.</param>
        /// <param name="otherTag">The second member of the <c>OrGroup</c>.</param>
        /// <returns>An <c>OrGroup</c> containing the two provided tags.</returns>
        public static OrGroup Or(this Tag tag, Tag otherTag)
        {
            return new[] { new[] { tag }, new[] { otherTag } };
        }

        /// <summary>
        /// Couples a tag and an <c>AndGroup</c> together to represent a requirement for either
        /// <paramref name="tag"/> to be satisfied <i>or</i> all the tags within <paramref name="andGroup"/>.
        /// </summary>
        /// <param name="tag">The tag to include as one member of the <c>OrGroup</c>.</param>
        /// <param name="andGroup">An <c>AndGroup</c> to include as the second member of the <c>OrGroup</c>.</param>
        /// <returns>
        /// An <c>OrGroup</c> containing <paramref name="tag"/> as one member and <paramref name="andGroup"/> as
        /// the second.
        /// </returns>
        public static OrGroup Or(this Tag tag, AndGroup andGroup)
        {
            return new[] { new[] { tag }, andGroup };
        }

        /// <summary>
        /// Couples a tag and an <c>OrGroup</c> together to represent a requirement for <i>any</i>
        /// tag witin the <c>OrGroup</c> or <paramref name="tag"/> to be be satisfied.
        /// </summary>
        /// <param name="tag">The tag to include within <paramref name="orGroup"/>.</param>
        /// <param name="orGroup">An existing <c>OrGroup</c>.</param>
        /// <returns>
        /// An <c>OrGroup</c> based on <paramref name="orGroup"/> that includes <paramref name="tag"/>
        /// </returns>
        public static OrGroup Or(this Tag tag, OrGroup orGroup)
        {
            return orGroup.Prepend(new[] { tag });
        }

        /// <summary>
        /// Couples a tag and an <c>AndGroup</c> together to represent a requirement for either
        /// <paramref name="tag"/> to be satisfied <i>or</i> all the tags within <paramref name="andGroup"/>.
        /// </summary>
        /// <param name="andGroup">An <c>AndGroup</c> to include as the second member of the <c>OrGroup</c>.</param>
        /// <param name="tag">The tag to include as one member of the <c>OrGroup</c>.</param>
        /// <returns>
        /// An <c>OrGroup</c> containing <paramref name="tag"/> as one member and <paramref name="andGroup"/> as
        /// the second.
        /// </returns>
        public static OrGroup Or(this AndGroup andGroup, Tag tag)
        {
            return new[] { andGroup, new[] { tag } };
        }

        /// <summary>
        /// Couples two <c>AndGroup</c> instances together to represent a requirement for either
        /// <i>all</i> the tags within the first group to be satisfied or <i>all</i> the tags
        /// within the second group to be satisfied.
        /// </summary>
        /// <param name="andGroup">The first <c>AndGroup</c>.</param>
        /// <param name="otherAndGroup">The second <c>AndGroup</c>.</param>
        /// <returns>
        /// An <c>OrGroup</c> containing <paramref name="andGroup"/> as one member and
        /// <paramref name="otherAndGroup"/> as the second.
        /// </returns>
        public static OrGroup Or(this AndGroup andGroup, AndGroup otherAndGroup)
        {
            return new[] { andGroup, otherAndGroup };
        }

        /// <summary>
        /// Couples an <c>AndGroup</c> and an <c>OrGroup</c> together to represent a requirement for
        /// either <i>all</i> the tags within the <c>AndGroup</c> to be satisfied or <i>any</i>
        /// requirement within the <c>OrGroup</c> to be satisfied.
        /// </summary>
        /// <param name="andGroup">An existing <c>AndGroup</c>.</param>
        /// <param name="orGroup">An existing <c>OrGroup</c>.</param>
        /// <returns>
        /// An <c>OrGroup</c> based on <paramref name="orGroup"/> that includes <paramref name="andGroup"/>
        /// </returns>
        public static OrGroup Or(this AndGroup andGroup, OrGroup orGroup)
        {
            return orGroup.Prepend(andGroup);
        }

        /// <summary>
        /// Couples a tag and an <c>OrGroup</c> together to represent a requirement for <i>any</i>
        /// tag witin the <c>OrGroup</c> or <paramref name="tag"/> to be be satisfied.
        /// </summary>
        /// <param name="orGroup">An existing <c>OrGroup</c>.</param>
        /// <param name="tag">The tag to include within <paramref name="orGroup"/>.</param>
        /// <returns>
        /// An <c>OrGroup</c> based on <paramref name="orGroup"/> that includes <paramref name="tag"/>
        /// </returns>
        public static OrGroup Or(this OrGroup orGroup, Tag tag)
        {
            return orGroup.Append(new[] { tag });
        }

        /// <summary>
        /// Couples an <c>AndGroup</c> and an <c>OrGroup</c> together to represent a requirement for
        /// either <i>all</i> the tags within the <c>AndGroup</c> to be satisfied or <i>any</i>
        /// requirement within the <c>OrGroup</c> to be satisfied.
        /// </summary>
        /// <param name="orGroup">An existing <c>OrGroup</c>.</param>
        /// <param name="andGroup">An existing <c>AndGroup</c>.</param>
        /// <returns>
        /// An <c>OrGroup</c> based on <paramref name="orGroup"/> that includes <paramref name="andGroup"/>
        /// </returns>
        public static OrGroup Or(this OrGroup orGroup, AndGroup andGroup)
        {
            return orGroup.Append(andGroup);
        }

        /// <summary>
        /// Couples two <c>OrGroup</c> instances together to represent a requirement for any
        /// tags within each group to be satisfied.
        /// </summary>
        /// <param name="orGroup">The first <c>OrGroup</c>.</param>
        /// <param name="otherOrGroup">The second <c>OrGroup</c>.</param>
        /// <returns>An <c>OrGroup</c> formed by merging the two provided groups.</returns>
        public static OrGroup Or(this OrGroup orGroup, OrGroup otherOrGroup)
        {
            return orGroup.Concat(otherOrGroup);
        }
        #endregion
    }
}
