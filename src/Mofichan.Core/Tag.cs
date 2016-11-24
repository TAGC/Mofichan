using System.Collections.Generic;
using System.Linq;

namespace Mofichan.Core
{
    using AndGroup = IEnumerable<Tag>;
    using OrGroup = IEnumerable<IEnumerable<Tag>>;
    using TagGroup = IEnumerable<IEnumerable<Tag>>;

    public enum Tag
    {
        DirectedAtMofichan,
        Wellbeing,
        Phrase,
        Emote,
        Happy,
        Cute,
        Greeting,
        Goodbye,
        Positive,
        Negative,
        Test
    }

    public static class TagExtensions
    {
        #region As Group
        public static TagGroup AsGroup(this Tag tag)
        {
            return new[] { new[] { tag } };
        }

        public static TagGroup AsGroup(this AndGroup andGroup)
        {
            return new[] { andGroup };
        }

        public static TagGroup AsGroup(this OrGroup orGroup)
        {
            return orGroup;
        }
        #endregion

        #region And
        public static AndGroup And(this Tag tag, Tag otherTag)
        {
            return new[] { tag, otherTag };
        }

        public static AndGroup And(this Tag tag, AndGroup andGroup)
        {
            return andGroup.Prepend(tag);
        }

        public static AndGroup And(this AndGroup andGroup, Tag tag)
        {
            return andGroup.Append(tag);
        }

        public static AndGroup And(this AndGroup andGroup, AndGroup otherAndGroup)
        {
            return andGroup.Concat(otherAndGroup);
        }
        #endregion

        #region Or
        public static OrGroup Or(this Tag tag, Tag otherTag)
        {
            return new[] { new[] { tag }, new[] { otherTag } };
        }

        public static OrGroup Or(this Tag tag, AndGroup andGroup)
        {
            return new[] { new[] { tag }, andGroup };
        }

        public static OrGroup Or(this Tag tag, OrGroup orGroup)
        {
            return orGroup.Prepend(new[] { tag });
        }

        public static OrGroup Or(this AndGroup andGroup, Tag tag)
        {
            return new[] { andGroup, new[] { tag } };
        }

        public static OrGroup Or(this AndGroup andGroup, AndGroup otherAndGroup)
        {
            return new[] { andGroup, otherAndGroup };
        }

        public static OrGroup Or(this AndGroup andGroup, OrGroup orGroup)
        {
            return orGroup.Prepend(andGroup);
        }

        public static OrGroup Or(this OrGroup orGroup, Tag tag)
        {
            return orGroup.Append(new[] { tag });
        }

        public static OrGroup Or(this OrGroup orGroup, AndGroup andGroup)
        {
            return orGroup.Append(andGroup);
        }

        public static OrGroup Or(this OrGroup orGroup, OrGroup otherOrGroup)
        {
            return orGroup.Concat(otherOrGroup);
        }
        #endregion
    }
}
