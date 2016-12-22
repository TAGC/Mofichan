using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Shouldly;
using Xunit;
using static Mofichan.Tests.TestUtility.MessageUtil;

namespace Mofichan.Tests
{
    public class PulseDrivenResponseSelectorTests
    {
        private static void SetupVisitors(IBehaviourVisitor[] visitors)
        {
            visitors[0].RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("a"))
                .RelevantBecause(it => it.SuitsMessageTags("a")));

            visitors[0].RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("b"))
                .RelevantBecause(it => it.SuitsMessageTags("b")));

            visitors[1].RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("c"))
                .RelevantBecause(it => it.SuitsMessageTags("c")));

            visitors[2].RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("d"))
                .RelevantBecause(it => it.SuitsMessageTags("d")));

            visitors[3].RegisterResponse(rb => rb
                .WithMessage(mb => mb.FromRaw("e"))
                .RelevantBecause(it => it.SuitsMessageTags("e")));
        }

        private static OnMessageVisitor CreateMessageVisitor(MessageContext message)
        {
            return new OnMessageVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder);
        }

        private static OnPulseVisitor CreatePulseVisitor(MessageContext message)
        {
            return new OnPulseVisitor(message, MockBotContext.Instance, CreateSimpleMessageBuilder);
        }

        private readonly ControllablePulseDriver pulseDriver;

        private PulseDrivenResponseSelector responseSelector;

        public PulseDrivenResponseSelectorTests()
        {
            this.pulseDriver = new ControllablePulseDriver();
        }

        private void SetupResponseSelector(int responseWindow)
        {
            // GIVEN a mock relevance argument evaluator that scores relevance by lexicographic ordering of tags.
            var mockArgumentEvaluator = new Mock<IRelevanceArgumentEvaluator>();
            mockArgumentEvaluator
                .Setup(it => it.Evaluate(It.IsAny<IEnumerable<RelevanceArgument>>(), It.IsAny<MessageContext>()))
                .Returns<IEnumerable<RelevanceArgument>, MessageContext>((arguments, _) =>
                {
                    return from argument in arguments
                           let letter = argument.MessageTagArguments.ElementAt(0)[0]
                           let score = (double)letter
                           select Tuple.Create(argument, score);
                });

            this.responseSelector = new PulseDrivenResponseSelector(responseWindow, this.pulseDriver,
                mockArgumentEvaluator.Object, MockLogger.Instance);
        }

        private void GeneratePulses(int count)
        {
            for (var i = 0; i < count; i++)
            {
                this.pulseDriver.Pulse();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Response_Selector_Should_Throw_Exception_If_Response_Window_Is_Invalid(int responseWindow)
        {
            Assert.Throws<ArgumentException>(() => new PulseDrivenResponseSelector(responseWindow,
                Mock.Of<IPulseDriver>(), Mock.Of<IRelevanceArgumentEvaluator>(), MockLogger.Instance));
        }

        [Fact]
        public void Response_Selector_Should_Throw_Exception_If_Pulse_Driver_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new PulseDrivenResponseSelector(1, null,
                Mock.Of<IRelevanceArgumentEvaluator>(), MockLogger.Instance));
        }

        [Fact]
        public void Response_Selector_Should_Throw_Exception_If_Evaluator_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new PulseDrivenResponseSelector(1,
                Mock.Of<IPulseDriver>(), null, MockLogger.Instance));
        }

        [Fact]
        public void Response_Selector_Should_Not_Select_Response_Until_Enough_Pulses_Have_Occurred()
        {
            // GIVEN a response selector with a response window of 5 pulses.
            SetupResponseSelector(5);

            Response selectedResponse = null;
            responseSelector.ResponseSelected += (s, e) => selectedResponse = e.Response;

            bool expirationOccurred = false;
            responseSelector.ResponseWindowExpired += (s, e) => expirationOccurred = true;

            // GIVEN a visitor carrying a message.
            var message = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "foo");
            var visitor = CreateMessageVisitor(message);

            // WHEN the response selector receives the visitor.
            responseSelector.InspectVisitor(visitor);

            // AND only 4 pulses occur.
            GeneratePulses(4);

            // THEN a response should not have been selected.
            selectedResponse.ShouldBeNull();

            // AND the expiration event should have not fired.
            expirationOccurred.ShouldBeFalse();
        }

        [Fact]
        public void Response_Selector_Should_Signal_Response_Window_Expiration_If_No_Candidate_Responses_Found()
        {
            // GIVEN a response selector with a response window of 3 pulses.
            SetupResponseSelector(3);

            Response selectedResponse = null;
            responseSelector.ResponseSelected += (s, e) => selectedResponse = e.Response;

            bool expirationOccurred = false;
            responseSelector.ResponseWindowExpired += (s, e) => expirationOccurred = true;

            // GIVEN a visitor carrying a message.
            var message = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "foo");
            var visitor = CreateMessageVisitor(message);

            // WHEN the response selector receives the visitor.
            responseSelector.InspectVisitor(visitor);

            // AND 3 pulses occur.
            GeneratePulses(3);

            // THEN a response should not have been selected.
            selectedResponse.ShouldBeNull();

            // AND the expiration event should have fired.
            expirationOccurred.ShouldBeTrue();
        }

        [Fact]
        public void Response_Selector_Should_Consider_Responses_From_All_Visitors_Within_Window()
        {
            // GIVEN a response selector with a response window of 3 pulses.
            SetupResponseSelector(3);

            Response selectedResponse = null;
            responseSelector.ResponseSelected += (s, e) => selectedResponse = e.Response;

            bool expirationOccurred = false;
            responseSelector.ResponseWindowExpired += (s, e) => expirationOccurred = true;

            // GIVEN a collection of four visitors with registered responses, all responding to the same message.
            var message = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "foo");

            var visitors = new IBehaviourVisitor[]
            {
                CreateMessageVisitor(message),
                CreatePulseVisitor(message),
                CreatePulseVisitor(message),
                CreatePulseVisitor(message),
            };

            SetupVisitors(visitors);

            // WHEN the response selector receives a visitor on each pulse.
            foreach (var visitor in visitors)
            {
                responseSelector.InspectVisitor(visitor);
                pulseDriver.Pulse();
            }

            // THEN the response selector should have selected the response with body "d".
            selectedResponse.Message.Body.ShouldBe("d");

            // AND the expiration event should not have fired.
            expirationOccurred.ShouldBeFalse();
        }

        [Fact]
        public void Response_Selector_Should_Only_Group_Visitors_Responding_To_Same_Message()
        {
            // GIVEN a response selector with a response window of 3 pulses.
            SetupResponseSelector(3);

            var selectedResponses = new List<Response>();
            responseSelector.ResponseSelected += (s, e) => selectedResponses.Add(e.Response);

            bool expirationOccurred = false;
            responseSelector.ResponseWindowExpired += (s, e) => expirationOccurred = true;

            // GIVEN two messages.
            var messageA = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "foo");
            var messageB = new MessageContext(Mock.Of<IUser>(), Mock.Of<IUser>(), "bar");
            Assert.False(messageA.Equals(messageB), "The message contexts should be considered different");

            // GIVEN a collection of four visitors with registered responses, responding to different messages.
            var visitors = new IBehaviourVisitor[]
            {
                CreateMessageVisitor(messageA),
                CreateMessageVisitor(messageB),
                CreatePulseVisitor(messageB),
                CreatePulseVisitor(messageA),
            };

            SetupVisitors(visitors);

            // WHEN the response selector receives a visitor on each pulse.
            foreach (var visitor in visitors)
            {
                responseSelector.InspectVisitor(visitor);
                pulseDriver.Pulse();
            }

            // THEN the response selector should have selected response "b" (to message A).
            selectedResponses.Count.ShouldBe(2);
            selectedResponses[0].Message.Body.ShouldBe("b");

            // AND the response selector should have selected repsonse "d" (to message B).
            selectedResponses[1].Message.Body.ShouldBe("d");

            // AND the expiration event should not have fired.
            expirationOccurred.ShouldBeFalse();
        }
    }
}
