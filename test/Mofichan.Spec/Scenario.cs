using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Moq;
using Serilog;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Mofichan.Spec
{
    public abstract class Scenario
    {
        protected const string AddBehaviourTemplate = "Given Mofichan is configured to use behaviour '{0}'";

        private readonly string scenarioTitle;
        private readonly ControllableFlowDriver flowDriver;

        private IObserver<IncomingMessage> backendObserver;
        private ILifetimeScope lifetimeScope;

        protected Scenario(string scenarioTitle = null)
        {

            this.scenarioTitle = scenarioTitle;
            this.flowDriver = new ControllableFlowDriver();
            this.MofichanUser = ConstructMockUser("Mofichan", "Mofichan", UserType.Self);
            this.DeveloperUser = ConstructMockUser("ThymineC", "ThymineC", UserType.Adminstrator);
            this.JohnSmithUser = ConstructMockUser("John Smith", "JohnSmith", UserType.NormalUser);
            this.Backend = this.ConstructMockBackend();
            this.Behaviours = new List<IMofichanBehaviour>();
            this.SentMessages = new List<OutgoingMessage>();
            this.Container = this.CreateContainerBuilder().Build();
            this.lifetimeScope = this.Container.BeginLifetimeScope();
            this.MessageSent += (s, e) => this.SentMessages.Add(e.Message);
        }

        [Fact]
        public void Execute()
        {
            this.BDDfy(scenarioTitle: this.scenarioTitle);
        }

        private class ControllableFlowDriver : IFlowDriver
        {
            public void StepFlow()
            {
                this.OnNextStep?.Invoke(this, EventArgs.Empty);
            }

            public event EventHandler OnNextStep;
        }

        #region Setup

        /// <summary>
        /// Test specifications can override this method to customise the creation
        /// of the test IoC container.
        /// </summary>
        /// <returns>The test IoC container builder.</returns>
        protected virtual ContainerBuilder CreateContainerBuilder()
        {
            Func<Type, string> getBehaviourName = it =>
                it.Name.ToLowerInvariant().Replace("behaviour", string.Empty);

            var behaviourAssembly = typeof(BaseBehaviour).Assembly();

            var containerBuilder = new ContainerBuilder();
            containerBuilder
                .RegisterAssemblyTypes(behaviourAssembly)
                .AssignableTo(typeof(IMofichanBehaviour))
                .Named<IMofichanBehaviour>(getBehaviourName)
                .AsImplementedInterfaces();

            containerBuilder
                .RegisterType<BehaviourChainBuilder>()
                .As<IBehaviourChainBuilder>();

            containerBuilder
                .RegisterType<FairFlowTransitionSelector>()
                .As<IFlowTransitionSelector>();

            containerBuilder
                .RegisterType<FlowDrivenAttentionManager>()
                .As<IAttentionManager>()
                .WithParameter("mu", 100)
                .WithParameter("sigma", 10)
                .SingleInstance();

            containerBuilder
                .RegisterType<FlowTransitionManager>()
                .As<IFlowTransitionManager>();

            containerBuilder
                .RegisterType<FlowManager>()
                .As<IFlowManager>();

            containerBuilder
                .RegisterInstance(this.flowDriver)
                .As<IFlowDriver>();

            containerBuilder
                .RegisterInstance(new LoggerConfiguration().CreateLogger())
                .As<ILogger>();

            // Register data access modules.
            containerBuilder
                .RegisterModule<DataAccess.Analysis.AnalysisModule>()
                .RegisterModule<DataAccess.Response.ResponseModule>();

            return containerBuilder;
        }

        private IMofichanBackend ConstructMockBackend()
        {
            var mock = new Mock<IMofichanBackend>();
            mock.Setup(it => it.Subscribe(It.IsAny<IObserver<IncomingMessage>>()))
                .Callback<IObserver<IncomingMessage>>(observer => this.backendObserver = observer);

            mock.Setup(backend => backend.OnNext(It.IsAny<OutgoingMessage>()))
                .Callback<OutgoingMessage>(message => this.OnMessageSent(message));

            return mock.Object;
        }
        #endregion

        protected IContainer Container { get; }
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

        protected virtual void TearDown()
        {
            this.Mofichan?.Dispose();
            this.lifetimeScope?.Dispose();

            this.Behaviours.Clear();
            this.SentMessages.Clear();

            this.lifetimeScope = this.Container.BeginLifetimeScope();
        }

        #region Given
        protected void Given_Mofichan_is_configured_with_behaviour(string behaviour)
        {
            this.Behaviours.Add(this.lifetimeScope.ResolveNamed<IMofichanBehaviour>(behaviour));
        }

        protected void Given_Mofichan_is_configured_with_behaviour(IMofichanBehaviour behaviour)
        {
            this.Behaviours.Add(behaviour);
        }

        protected void Given_Mofichan_is_running()
        {
            this.Mofichan = new Kernel(
                this.Backend,
                this.Behaviours,
                this.lifetimeScope.Resolve<IBehaviourChainBuilder>(),
                this.lifetimeScope.Resolve<IMessageClassifier>(),
                this.lifetimeScope.Resolve<ILogger>());

            this.Mofichan.Start();
        }
        #endregion

        #region When
        protected void When_Mofichan_receives_a_message(IUser sender, string message)
        {
            var incomingMessage = new IncomingMessage(
                new MessageContext(from: sender, to: this.MofichanUser, body: message));

            this.backendObserver.OnNext(incomingMessage);
        }

        protected void When_flows_are_driven_by__stepCount__steps(int stepCount)
        {
            for (var i = 0; i < stepCount; i++)
            {
                this.flowDriver.StepFlow();
            }
        }
        #endregion

        #region Then
        protected void Then_Mofichan_should_have_responded()
        {
            this.SentMessages.ShouldNotBeEmpty();
        }

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

        protected void Then_Mofichan_should_respond_to__message__(string message)
        {
            this.Then_Mofichan_should_respond_to__message__from__user__(this.JohnSmithUser, message);
        }

        protected void Then_Mofichan_should_respond_to__message__from__user__(IUser user, string message)
        {
            this.When_Mofichan_receives_a_message(user, message);
            this.SentMessages.ShouldNotBeEmpty();
        }
        #endregion
    }
}