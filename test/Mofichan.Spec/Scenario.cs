﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using Autofac;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Moq;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Mofichan.Spec
{
    public abstract class Scenario
    {
        protected const string AddBehaviourTemplate = "Given Mofichan is configured to use behaviour '{0}'";

        private readonly string scenarioTitle;
        private readonly IContainer container;

        private ITargetBlock<IncomingMessage> backendTarget;

        #region Setup
        protected Scenario(string scenarioTitle = null)
        {
            this.scenarioTitle = scenarioTitle;
            this.MofichanUser = ConstructMockUser("Mofichan", "Mofichan", UserType.Self);
            this.DeveloperUser = ConstructMockUser("ThymineC", "ThymineC", UserType.Adminstrator);
            this.JohnSmithUser = ConstructMockUser("John Smith", "JohnSmith", UserType.NormalUser);
            this.Backend = ConstructMockBackend();
            this.Behaviours = new List<IMofichanBehaviour>();

            Func<Type, string> getBehaviourName = it =>
                it.Name.ToLowerInvariant().Replace("behaviour", string.Empty);

            var behaviourAssembly = typeof(BaseBehaviour).Assembly();
            var containerBuilder = new ContainerBuilder();
            containerBuilder
                .RegisterAssemblyTypes(behaviourAssembly)
                .AssignableTo(typeof(IMofichanBehaviour))
                .Named<IMofichanBehaviour>(getBehaviourName)
                .AsImplementedInterfaces();

            this.container = containerBuilder.Build();
            this.SentMessages = new List<OutgoingMessage>();
            this.MessageSent += (s, e) => this.SentMessages.Add(e.Message);
        }

        private IMofichanBackend ConstructMockBackend()
        {
            var mock = new Mock<IMofichanBackend>();
            mock.Setup(backend => backend.LinkTo(
                    It.IsAny<ITargetBlock<IncomingMessage>>(),
                    It.IsAny<DataflowLinkOptions>()))
                .Callback<ITargetBlock<IncomingMessage>, DataflowLinkOptions>(
                    (target, _) => this.backendTarget = target);

            mock.Setup(backend => backend.OfferMessage(
                    It.IsAny<DataflowMessageHeader>(),
                    It.IsAny<OutgoingMessage>(),
                    It.IsAny<ISourceBlock<OutgoingMessage>>(),
                    It.IsAny<bool>()))
                .Callback<DataflowMessageHeader, OutgoingMessage, ISourceBlock<OutgoingMessage>, bool>(
                    (_, message, __, ___) => this.OnMessageSent(message));
            return mock.Object;
        }
        #endregion

        [Fact]
        public void Execute()
        {
            this.BDDfy(scenarioTitle: this.scenarioTitle);
        }

        protected IList<OutgoingMessage> SentMessages { get; }
        protected IList<IMofichanBehaviour> Behaviours { get; }
        protected IMofichanBackend Backend { get; }
        protected IUser MofichanUser { get; }
        protected IUser DeveloperUser { get; }
        protected IUser JohnSmithUser { get; }
        protected Kernel Mofichan { get; private set; }

        protected event EventHandler<MessageSentEventArgs> MessageSent;

        protected static IUser ConstructMockUser(string userId, string userName,
            UserType userType = UserType.NormalUser)
        {
            var mock = new Mock<IUser>();
            mock.SetupGet(user => user.UserId).Returns(userId);
            mock.SetupGet(user => user.Name).Returns(userName);
            mock.SetupGet(user => user.Type).Returns(userType);

            return mock.Object;
        }

        private void OnMessageSent(OutgoingMessage message)
        {
            this.MessageSent?.Invoke(this, new MessageSentEventArgs(message));
        }

        public class MessageSentEventArgs : EventArgs
        {
            public MessageSentEventArgs(OutgoingMessage message)
            {
                this.Message = message;
            }

            public OutgoingMessage Message { get; }
        }

        #region Given
        protected void Given_Mofichan_is_configured_with_behaviour(string behaviour)
        {
            this.Behaviours.Add(this.container.ResolveNamed<IMofichanBehaviour>(behaviour));
        }

        protected void Given_Mofichan_is_configured_with_behaviour(IMofichanBehaviour behaviour)
        {
            this.Behaviours.Add(behaviour);
        }

        protected void Given_Mofichan_is_running()
        {
            this.Mofichan = new Kernel("Mofichan", this.Backend, this.Behaviours);
            this.Mofichan.Start();
        }
        #endregion

        #region When
        protected void When_Mofichan_receives_a_message(IUser sender, string message)
        {
            var incomingMessage = new IncomingMessage(
                new MessageContext(from: sender, to: this.MofichanUser, body: message));

            this.backendTarget.Post(incomingMessage);
        }
        #endregion

        #region Then
        protected void Then_Mofichan_should_have_sent__body__(string body)
        {
            this.SentMessages.Select(it => it.Context.Body).ShouldContain(body);
        }

        protected void Then_Mofichan_should_have_sent_response_with_pattern(
            string pattern, RegexOptions options)
        {
            Assert.True(this.SentMessages
                .Select(it => it.Context.Body)
                .Any(it => Regex.Match(it, pattern, options).Success));
        }

        protected void Then_Mofichan_should_have_sent_response_containing__substring__(string substring)
        {
            Assert.True(this.SentMessages
                .Select(it => it.Context.Body)
                .Any(it => it.Contains(substring)));
        }
        #endregion
    }
}