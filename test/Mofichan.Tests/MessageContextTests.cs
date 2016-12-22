using System;
using System.Collections.Generic;
using Mofichan.Core;
using Mofichan.Tests.TestUtility;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class MessageContextTests
    {
        public static IEnumerable<object[]> EqualContextExamples
        {
            get
            {
                yield return new object[]
                {
                    new MessageContext(),
                    new MessageContext()
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        created: new DateTime(1, 1, 1)),
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        created: new DateTime(1, 1, 1))
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(1, 1, 1))
                };
            }
        }

        public static IEnumerable<object[]> UnequalContextExamples
        {
            get
            {
                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Thomas", "Thomas"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        created: new DateTime(1, 1, 1)),
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "goodbye",
                        delay: TimeSpan.FromMilliseconds(100),
                        created: new DateTime(1, 1, 1)),
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(200),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(1, 1, 1))
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar", "baz" },
                        created: new DateTime(1, 1, 1))
                };

                yield return new object[]
                {
                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(1, 1, 1)),

                    new MessageContext(
                        from: new MockUser("Tom", "Tom"),
                        to: new MockUser("Jerry", "Jerry"),
                        body: "hello",
                        delay: TimeSpan.FromMilliseconds(100),
                        tags: new[] { "foo", "bar" },
                        created: new DateTime(2, 2, 2))
                };
            }
        }

        [Fact]
        public void Message_Context_Should_Default_To_Zero_Delay()
        {
            // WHEN a message context is created with no specified delay.
            var context = new MessageContext(from: null, to: null, body: null);

            // THEN it should default to having zero delay.
            context.Delay.ShouldBe(TimeSpan.Zero);
        }

        [Theory]
        [MemberData(nameof(EqualContextExamples))]
        public void Message_Contexts_Should_Be_Considered_Equal(MessageContext a, MessageContext b)
        {
            a.Equals(b).ShouldBeTrue();
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Theory]
        [MemberData(nameof(UnequalContextExamples))]
        public void Message_Contexts_Should_Not_Be_Considered_Equal(MessageContext a, MessageContext b)
        {
            a.Equals(b).ShouldBeFalse();
        }
    }
}
