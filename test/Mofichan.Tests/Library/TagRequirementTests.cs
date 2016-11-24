using System.Collections.Generic;
using Mofichan.Core;
using Mofichan.Library.Response;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.Library
{
    public class TagRequirementTests
    {
        public static IEnumerable<object> TagRequirementExamples
        {
            get
            {
                yield return new object[]
                {
                    // Requirement
                    Tag.Happy.AsGroup(),

                    // Satisfied by
                    new[]
                    {
                        new[] { Tag.Happy },
                    },

                    // Unsatisfied by
                    new[]
                    {
                        new[] { Tag.Positive },
                        new[] { Tag.Cute },
                    },
                };

                yield return new object[]
                {
                    // Requirement
                    Tag.Happy.Or(Tag.Positive).AsGroup(),

                    // Satisfied by
                    new[]
                    {
                        new[] { Tag.Happy },
                        new[] { Tag.Happy },
                        new[] { Tag.Happy, Tag.Positive },
                    },

                    // Unsatisfied by
                    new[]
                    {
                        new[] { Tag.Cute },
                    },
                };

                yield return new object[]
                {
                    // Requirement
                    Tag.Happy.And(Tag.Positive).Or(Tag.Cute).AsGroup(),

                    // Satisfied by
                    new[]
                    {
                        new[] { Tag.Happy, Tag.Positive, Tag.Cute },
                        new[] { Tag.Happy, Tag.Positive },
                        new[] { Tag.Happy, Tag.Cute },
                        new[] { Tag.Cute },
                    },

                    // Unsatisfied by
                    new[]
                    {
                        new[] { Tag.Positive },
                        new[] { Tag.Happy },
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TagRequirementExamples))]
        public void Tag_Requirements_Should_Declare_Satisfaction_From_Provided_Tags_As_Expected(
            IEnumerable<IEnumerable<Tag>> tagRequirementGroup,
            IEnumerable<IEnumerable<Tag>> expectedSatisfiedBy,
            IEnumerable<IEnumerable<Tag>> expectedUnsatisfiedBy)
        {
            // GIVEN a tag requirement based on the provided tag requirement group.
            var tagRequirement = TagRequirement.From(tagRequirementGroup);

            // EXPECT that the tag requirement is satisfied by provided groups of tags as appropriate.
            expectedSatisfiedBy.ShouldAllBe(tagGroup => tagRequirement.SatisfiedBy(tagGroup));

            // EXPECT that the tag requirement is unsatisfied by provided groups of tags as appropriate.
            expectedUnsatisfiedBy.ShouldAllBe(tagGroup => !tagRequirement.SatisfiedBy(tagGroup));
        }
    }
}
