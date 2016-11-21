using System;
using System.Collections.Generic;
using Mofichan.Library;
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
                    "foo",

                    // Satisfied by
                    new[]
                    {
                        new[] { "foo" },
                    },

                    // Unsatisfied by
                    new[]
                    {
                        new[] { "bar" },
                        new[] { "baz" },
                    },
                };

                yield return new object[]
                {
                    // Requirement
                    "foo;bar",

                    // Satisfied by
                    new[]
                    {
                        new[] { "foo" },
                        new[] { "foo" },
                        new[] { "foo", "bar" },
                    },

                    // Unsatisfied by
                    new[]
                    {
                        new[] { "baz" },
                    },
                };

                yield return new object[]
                {
                    // Requirement
                    "foo,bar;baz",

                    // Satisfied by
                    new[]
                    {
                        new[] { "foo", "bar", "baz" },
                        new[] { "foo", "bar" },
                        new[] { "foo", "baz" },
                        new[] { "baz" },
                    },

                    // Unsatisfied by
                    new[]
                    {
                        new[] { "bar" },
                        new[] { "foo" },
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TagRequirementExamples))]
#pragma warning disable S2368 // Public methods should not have multidimensional array parameters
        public void No_Exception_Should_Be_Thrown_When_Valid_Tag_Requirement_Representation_Is_Parsed(
#pragma warning restore S2368 // Public methods should not have multidimensional array parameters
            string validRepresentation, string[][] _, string[][] __)
        {
            // EXPECT we can parse the valid tag requirement representation without exception.
            TagRequirement.Parse(validRepresentation).ShouldNotBeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("@illegal?characters")]
        [InlineData("multiword tag without hyphen")]
        public void Exception_Should_Be_Thrown_When_Invalid_Tag_Requirement_Representation_Is_Parsed(
            string invalidRepresentation)
        {
            // EXPECT that an exception is thrown when we try to parse the invalid representation.
            Assert.Throws<ArgumentException>(() => TagRequirement.Parse(invalidRepresentation));
        }

        [Theory]
        [MemberData(nameof(TagRequirementExamples))]
#pragma warning disable S2368 // Public methods should not have multidimensional array parameters
        public void Tag_Requirements_Should_Declare_Satisfaction_From_Provided_Tags_As_Expected(
#pragma warning restore S2368 // Public methods should not have multidimensional array parameters
            string tagRequirementRepr,
            string[][] expectedSatisfiedBy,
            string[][] expectedUnsatisfiedBy)
        {
            // GIVEN a tag requirement based on the provided representation.
            var tagRequirement = TagRequirement.Parse(tagRequirementRepr);

            // EXPECT that the tag requirement is satisfied by provided groups of tags as appropriate.
            expectedSatisfiedBy.ShouldAllBe(tagGroup => tagRequirement.SatisfiedBy(tagGroup));

            // EXPECT that the tag requirement is unsatisfied by provided groups of tags as appropriate.
            expectedUnsatisfiedBy.ShouldAllBe(tagGroup => !tagRequirement.SatisfiedBy(tagGroup));
        }
    }
}
