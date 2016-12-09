using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class VectorSimilarityEvaluatorTests
    {
        private static MessageContext CreateMessageFromTags(params string[] tags)
        {
            var from = new MockUser("Tom", "Tom");
            var to = new MockUser("Jerry", "Jerry");
            var body = "foo";

            return new MessageContext(from, to, body, tags: tags);
        }

        public static IEnumerable<object[]> Examples
        {
            get
            {
                // Best argument matches all tags associated with message.
                yield return new object[]
                {
                    CreateMessageFromTags("foo", "bar", "baz"),
                    new RelevanceArgument(new[] { "foo", "bar", "baz" }, false),
                    new[]
                    {
                        new RelevanceArgument(new[] { "foo" }, false),
                        new RelevanceArgument(new[] { "foo", "bar", "baz" }, false),
                        new RelevanceArgument(new[] { "foo", "bar" }, false),
                    }
                };

                // Two arguments match all tags associated with message, but one contains
                // an unmatched tag - that weakens it.
                yield return new object[]
                {
                    CreateMessageFromTags("foo", "bar"),
                    new RelevanceArgument(new[] { "foo", "bar" }, false),
                    new[]
                    {
                        new RelevanceArgument(new[] { "foo" }, false),
                        new RelevanceArgument(new[] { "foo", "bar", "baz" }, false),
                        new RelevanceArgument(new[] { "foo", "bar" }, false),
                    }
                };

                // "foo" has lower occurrance in all arguments than "bar", so has higher weight.
                yield return new object[]
                {
                    CreateMessageFromTags("foo", "bar"),
                    new RelevanceArgument(new[] { "foo" }, false),
                    new[]
                    {
                        new RelevanceArgument(new[] { "foo" }, false),
                        new RelevanceArgument(new[] { "bar" }, false),
                        new RelevanceArgument(new[] { "bar" }, false),
                    }
                };
            }
        }

        [Fact]
        public void Exception_Should_Be_Thrown_If_Multiple_Arguments_Guarantee_Relevance()
        {
            // GIVEN a vector similarity evaluator.
            var evaluator = new VectorSimilarityEvaluator();

            // GIVEN a message.
            var message = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "foo");

            // GIVEN two arguments that guarantee relevance to the message.
            var arguments = new[]
            {
                new RelevanceArgument(Enumerable.Empty<string>(), true),
                new RelevanceArgument(Enumerable.Empty<string>(), true)
            };

            // EXPECT an exception is thrown when evaluating these arguments.
            Assert.Throws<ArgumentException>(() => evaluator.Evaluate(arguments, message));
        }

        [Theory]
        [MemberData(nameof(Examples))]
        public void Vector_Similarity_Evaluator_Should_Always_Assign_Highest_Score_To_Argument_With_Guaranteed_Relevance(
            MessageContext message,
            RelevanceArgument _,
            IEnumerable<RelevanceArgument> arguments)
        {
            // GIVEN a vector similarity evaluator.
            var evaluator = new VectorSimilarityEvaluator();

            // GIVEN an argument that is guarantee to be relevant for the message.
            var expectedBestArgument = new RelevanceArgument(Enumerable.Empty<string>(), true);

            // GIVEN a collection of arguments containing this argument.
            arguments = arguments.Append(expectedBestArgument);
            Assert.Single(arguments, arg => arg.GuaranteeRelevance);

            // WHEN we evaluate the collection of arguments.
            var scoredArguments = evaluator.Evaluate(arguments, message).OrderByDescending(it => it.Item2).ToList();

            // THEN the argument that guarantees relevance should always be scored highest.
            var actualBestArgument = scoredArguments[0].Item1;
            var actualBestScore = scoredArguments[0].Item2;
            var remainingScores = scoredArguments.Skip(1).Select(it => it.Item2);

            actualBestArgument.ShouldBe(expectedBestArgument);
            actualBestScore.ShouldBe(1, double.Epsilon);
            remainingScores.ShouldAllBe(it => Math.Abs(it) < double.Epsilon);
        }

        [Theory]
        [MemberData(nameof(Examples))]
        public void Vector_Similarity_Evaluator_Should_Normalise_Scores_Between_0_and_1(
            MessageContext message,
            RelevanceArgument _,
            IEnumerable<RelevanceArgument> arguments)
        {
            // GIVEN a vector similarity evaluator.
            var evaluator = new VectorSimilarityEvaluator();

            // WHEN we evaluate the arguments.
            var scoredArguments = evaluator.Evaluate(arguments, message);

            // THEN all argument scores should be between 0 and 1.
            var scores = scoredArguments.Select(it => it.Item2);

            scores.ShouldAllBe(it => it >= 0);
            scores.ShouldAllBe(it => it <= 1);
        }

        [Theory]
        [MemberData(nameof(Examples))]
        public void Vector_Similarity_Evaluator_Should_Assign_1_To_Most_Relevant_Argument(
            MessageContext message,
            RelevanceArgument _,
            IEnumerable<RelevanceArgument> arguments)
        {
            // GIVEN a vector similarity evaluator.
            var evaluator = new VectorSimilarityEvaluator();

            // WHEN we evaluate the arguments.
            var scoredArguments = evaluator.Evaluate(arguments, message);

            // THEN the highest scored argument should be 1.
            scoredArguments.Select(it => it.Item2).Max().ShouldBe(1, double.Epsilon);
        }

        [Theory]
        [MemberData(nameof(Examples))]
        public void Vector_Similarity_Evaluator_Should_Score_Expected_Argument_Highest(
                    MessageContext message,
                    RelevanceArgument expectedBestArgument,
                    IEnumerable<RelevanceArgument> arguments)
        {
            // GIVEN a vector similarity evaluator.
            var evaluator = new VectorSimilarityEvaluator();

            // WHEN we evaluate the arguments.
            var scoredArguments = evaluator.Evaluate(arguments, message);

            // THEN the highest scored argument should match expectations.
            var query = from o in scoredArguments
                        let argument = o.Item1
                        let score = o.Item2
                        orderby score descending
                        select argument;

            query.First().ShouldBe(expectedBestArgument);
        }
    }
}
