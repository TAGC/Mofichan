using System;
using Mofichan.Core;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class ResponseTests
    {
        private readonly BotContext botContext;
        private readonly Func<IResponseBodyBuilder> messageBuilderFactory;
        private readonly Mock<IAttentionManager> mockAttentionManager;

        public ResponseTests()
        {
            this.mockAttentionManager = new Mock<IAttentionManager>();
            mockAttentionManager.Setup(it => it.RenewAttentionTowardsUser(It.IsAny<IUser>()));

            var mockMemoryManager = Mock.Of<IMemoryManager>();

            this.botContext = new BotContext(this.mockAttentionManager.Object, mockMemoryManager);
            this.messageBuilderFactory = () => Mock.Of<IResponseBodyBuilder>();
        }

        private Response.Builder CreateBuilder()
        {
            return new Response.Builder(this.botContext, this.messageBuilderFactory);
        }

        [Fact]
        public void Response_Builder_Should_Accept_Custom_Actions()
        {
            // GIVEN an action.
            bool actionFired = false;
            Action action = () => actionFired = true;

            // GIVEN a response builder passed this action.
            var builder = CreateBuilder()
                .To(new MessageContext())
                .RelevantBecause(it => it.GuaranteesRelevance())
                .WithSideEffect(action);

            // WHEN we construct the response.
            var response = builder.Build();

            // THEN it should contain one action.
            var actualAction = response.SideEffects.ShouldHaveSingleItem();

            // WHEN we fire the action.
            actualAction.Invoke();

            // THEN the action passed to the builder should have fired.
            actionFired.ShouldBeTrue();
        }

        [Fact]
        public void Response_Builder_Should_Require_Message_Context_If_Response_Message_Specified()
        {
            // GIVEN a response builder with no message context specified.
            var builder = CreateBuilder();

            // EXPECT an exception is thrown when we try to configure the builder to include a response message.
            Assert.Throws<InvalidOperationException>(() => builder.WithMessage(mb => mb.FromRaw("foo")));
        }

        [Fact]
        public void Response_Builder_Should_Throw_Exception_If_Building_Without_Specifying_Context()
        {
            // GIVEN a response builder without specifying the message being responded to.
            var builder = CreateBuilder().RelevantBecause(it => it.GuaranteesRelevance());

            // EXPECT an exception is thrown when we try to construct a response from it.
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void Response_Builder_Should_Use_Default_Relevance_Argument_If_Custom_One_Not_Specified()
        {
            // GIVEN a response builder without a specified relevance argument.
            var builder = CreateBuilder().To(new MessageContext());

            // WHEN we create a response from the builder.
            var response = builder.Build();

            // THEN the response should use the default relevance argument.
            response.RelevanceArgument.GuaranteeRelevance.ShouldBeFalse();
            response.RelevanceArgument.MessageTagArguments.ShouldBeEmpty();
        }

        [Fact]
        public void Response_Builder_Should_Accept_Relevance_Argument()
        {
            // GIVEN a response builder.
            var builder = CreateBuilder().To(new MessageContext());

            // WHEN we configure the builder to include an argument about the response relevance.
            builder.RelevantBecause(it => it.SuitsMessageTags("foo", "bar"));

            // AND we construct a response using the builder.
            var response = builder.Build();

            // THEN the response should include the configured relevance argument.
            var argument = response.RelevanceArgument;
            argument.GuaranteeRelevance.ShouldBeFalse();
            argument.MessageTagArguments.ShouldBe(new[] { "foo", "bar" }, ignoreOrder: true);
        }

        [Fact]
        public void Responses_Should_Execute_Side_Effects_On_Acceptance()
        {
            // GIVEN a response configured with a set of side effects.
            var sideEffectATriggered = false;
            var sideEffectBTriggered = false;
            var sideEffectCTriggered = false;

            var response = CreateBuilder()
                .To(new MessageContext())
                .WithSideEffect(() => sideEffectATriggered = true)
                .WithSideEffect(() => sideEffectBTriggered = true)
                .WithSideEffect(() => sideEffectCTriggered = true)
                .Build();

            // WHEN we accept the response.
            response.Accept();

            // THEN the side effects associated with the response should have been triggered.
            sideEffectATriggered.ShouldBeTrue();
            sideEffectBTriggered.ShouldBeTrue();
            sideEffectCTriggered.ShouldBeTrue();
        }

        [Fact]
        public void Responses_Should_Perform_Specified_Changes_To_Bot_Context()
        {
            // GIVEN a mock user.
            var tom = new MockUser("Tom", "Tom");

            // GIVEN a response that will cause attention to be paid to Tom if accepted.
            var response = CreateBuilder()
                .To(new MessageContext())
                .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(tom))
                .Build();

            this.mockAttentionManager.Verify(it => it.RenewAttentionTowardsUser(It.IsAny<IUser>()), Times.Never);

            // WHEN we accept the response.
            response.Accept();

            // THEN attention should be paid to Tom.
            this.mockAttentionManager.Verify(it => it.RenewAttentionTowardsUser(It.IsAny<IUser>()), Times.Once);
        }
    }
}
