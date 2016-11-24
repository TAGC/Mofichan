using System.Collections.Generic;
using System.Linq;

namespace Mofichan.Core
{
    public enum Tag
    {
        DirectedAtMofichan,
        WellbeingResponse,
        Phrase,
        Emote,
        Happy,
        Cute,
        Greeting,
        Goodbye,
        Pleasant,
        Unpleasant,
        Test
    }

    public static class TagExtensions
    {
        public static IEnumerable<IEnumerable<Tag>> AsGroup(this Tag tag)
        {
            return new[] { new[] { tag } };
        }

        public static IEnumerable<IEnumerable<Tag>> AsGroup(this IEnumerable<Tag> partialTagGroup)
        {
            return new[] { partialTagGroup };
        }

        public static IEnumerable<IEnumerable<Tag>> AsGroup(this IEnumerable<IEnumerable<Tag>> tagGroup)
        {
            return tagGroup;
        }

        public static IEnumerable<Tag> And(this Tag tag, Tag otherTag)
        {
            return new[] { tag, otherTag };
        }

        public static IEnumerable<Tag> And(this Tag tag, IEnumerable<Tag> andGroup)
        {
            return andGroup.Prepend(tag);
        }

        public static IEnumerable<Tag> And(this IEnumerable<Tag> andGroup, Tag tag)
        {
            return andGroup.Append(tag);
        }

        public static IEnumerable<Tag> And(this IEnumerable<Tag> andGroup, IEnumerable<Tag> otherAndGroup)
        {
            return andGroup.Concat(otherAndGroup);
        }

        public static IEnumerable<IEnumerable<Tag>> Or(this IEnumerable<Tag> andGroup, Tag otherTag)
        {
            return new[] { andGroup, new[] { otherTag } };
        }

        public static IEnumerable<IEnumerable<Tag>> Or(this IEnumerable<Tag> andGroup, IEnumerable<Tag> otherAndGroup)
        {
            return new[] { andGroup, otherAndGroup };
        }

        public static IEnumerable<IEnumerable<Tag>> Or(this Tag tag, Tag otherTag)
        {
            return new[] { new[] { tag }, new[] { otherTag } };
        }
    }
}
