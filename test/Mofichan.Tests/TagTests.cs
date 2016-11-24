using System.Collections.Generic;
using Mofichan.Core;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class TagTests
    {
        public static IEnumerable<object[]> Examples
        {
            get
            {
                #region Test single tag
                yield return new object[]
                {
                    Tag.Happy.AsGroup(),
                    new[] { new[] { Tag.Happy }}
                };
                #endregion

                #region Test AND commutativity
                yield return new object[]
                {
                    Tag.Happy.And(Tag.Emote).AsGroup(),
                    new[] { new[] { Tag.Happy, Tag.Emote }}
                };

                yield return new object[]
                {
                    Tag.Emote.And(Tag.Happy).AsGroup(),
                    new[] { new[] { Tag.Emote, Tag.Happy }}
                };
                #endregion

                #region Test AND associativity
                yield return new object[]
                {
                    Tag.Happy.And(Tag.Emote.And(Tag.Positive)).AsGroup(),
                    new[] { new[] { Tag.Happy, Tag.Emote, Tag.Positive }}
                };

                yield return new object[]
                {
                    (Tag.Happy.And(Tag.Emote)).And(Tag.Positive).AsGroup(),
                    new[] { new[] { Tag.Happy, Tag.Emote, Tag.Positive }}
                };
                #endregion

                #region Test OR commutativity
                yield return new object[]
                {
                    Tag.Happy.Or(Tag.Emote).AsGroup(),
                    new[] { new[] { Tag.Happy }, new[] { Tag.Emote }}
                };

                yield return new object[]
                {
                    Tag.Emote.Or(Tag.Happy).AsGroup(),
                    new[] { new[] { Tag.Emote }, new[] { Tag.Happy }}
                };
                #endregion

                #region Test OR associativity
                yield return new object[]
                {
                    Tag.Happy.Or(Tag.Emote.Or(Tag.Positive)).AsGroup(),
                    new[] { new[] { Tag.Happy }, new[] { Tag.Emote }, new[] { Tag.Positive }}
                };

                yield return new object[]
                {
                    (Tag.Happy.Or(Tag.Emote)).Or(Tag.Positive).AsGroup(),
                    new[] { new[] { Tag.Happy }, new[] { Tag.Emote }, new[] { Tag.Positive }}
                };
                #endregion

                #region Test remaining valid combinations
                // Single [OR] AndGroup
                yield return new object[]
                {
                    Tag.Happy.Or(Tag.Emote.And(Tag.Positive)).AsGroup(),
                    new[] { new[] { Tag.Happy }, new[] { Tag.Emote, Tag.Positive }}
                };

                // AndGroup [AND] AndGroup
                yield return new object[]
                {
                    (Tag.Happy.And(Tag.Positive)).And(Tag.Greeting.And(Tag.Emote)).AsGroup(),
                    new[] { new[] { Tag.Happy, Tag.Positive, Tag.Greeting, Tag.Emote }}
                };

                // AndGroup [OR] AndGroup
                yield return new object[]
                {
                    (Tag.Happy.And(Tag.Positive)).Or(Tag.Greeting.And(Tag.Emote)).AsGroup(),
                    new[] { new[] { Tag.Happy, Tag.Positive }, new[] { Tag.Greeting, Tag.Emote }}
                };

                // OrGroup [OR] OrGroup
                yield return new object[]
                {
                    (Tag.Happy.Or(Tag.Positive)).Or(Tag.Greeting.Or(Tag.Emote)).AsGroup(),
                    new[] { new[] { Tag.Happy }, new[] { Tag.Positive }, new[] { Tag.Greeting }, new[] { Tag.Emote }}
                };
                #endregion
            }
        }

        [Theory]
        [MemberData(nameof(Examples))]
        public void Message_Tag_Group_Should_Have_Expected_Structure(
            IEnumerable<IEnumerable<Tag>> actualStructure,
            IEnumerable<IEnumerable<Tag>> expectedStructure)
        {
            actualStructure.ShouldBe(expectedStructure);
        }
    }
}
