﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.BotState;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Mofichan.DataAccess;
using Mofichan.DataAccess.Domain;
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

        private ControllablePulseDriver pulseDriver;
        private IObserver<MessageContext> backendObserver;
        private ILifetimeScope lifetimeScope;
        
        static Scenario()
        {
            DatabaseFixture.Initialise();
        }

        protected Scenario(string scenarioTitle = null)
        {
            this.scenarioTitle = scenarioTitle;
            this.pulseDriver = new ControllablePulseDriver();
            this.MofichanUser = ConstructMockUser("Mofichan", "Mofichan", UserType.Self);
            this.DeveloperUser = ConstructMockUser("ThymineC", "ThymineC", UserType.Adminstrator);
            this.JohnSmithUser = ConstructMockUser("John Smith", "JohnSmith", UserType.NormalUser);
            this.Backend = this.ConstructMockBackend();
            this.Behaviours = new List<IMofichanBehaviour>();
            this.SentMessages = new List<MessageContext>();
            this.Container = this.CreateContainerBuilder().Build();
            this.lifetimeScope = this.Container.BeginLifetimeScope();
            this.pulseDriver = this.lifetimeScope.Resolve<ControllablePulseDriver>();
            this.MessageSent += (s, e) => this.SentMessages.Add(e.Message);
        }

        [Fact]
        public void Execute()
        {
            this.BDDfy(scenarioTitle: this.scenarioTitle);
        }

        private class ControllablePulseDriver : IPulseDriver
        {
            public event EventHandler PulseOccurred;

            public void Pulse()
            {
                this.PulseOccurred?.Invoke(this, EventArgs.Empty);
            }
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
            var botConfig = this.BuildBotConfiguration();
            var containerBuilder = new ContainerBuilder();

            containerBuilder.Register(_ => botConfig);

            containerBuilder
                .RegisterAssemblyTypes(behaviourAssembly)
                .AssignableTo(typeof(IMofichanBehaviour))
                .Named<IMofichanBehaviour>(getBehaviourName)
                .AsImplementedInterfaces();

            containerBuilder
                .RegisterType<BehaviourChainBuilder>()
                .As<IBehaviourChainBuilder>();

            containerBuilder
                .RegisterType<PulseDrivenAttentionManager>()
                .As<IAttentionManager>()
                .WithParameter("mu", 200)
                .WithParameter("sigma", 10)
                .SingleInstance();

            containerBuilder
                .RegisterType<FlowTransitionManager>()
                .As<FlowTransitionManager>();

            containerBuilder
                .RegisterType<BotContext>();

            containerBuilder
                .RegisterType<ControllablePulseDriver>()
                .As<IPulseDriver>()
                .AsSelf()
                .SingleInstance();

            containerBuilder
                .RegisterType<BehaviourVisitorFactory>()
                .As<IBehaviourVisitorFactory>();

            containerBuilder
                .RegisterType<VectorSimilarityEvaluator>()
                .As<IRelevanceArgumentEvaluator>();

            containerBuilder
                .RegisterType<PulseDrivenResponseSelector>()
                .WithParameter("responseWindow", ResponseWindow)
                .As<IResponseSelector>();

            containerBuilder
                .RegisterInstance(new LoggerConfiguration().CreateLogger())
                .As<ILogger>();

            // Register data access modules.
            containerBuilder
                .RegisterModule<DataAccess.Analysis.AnalysisModule>()
                .RegisterModule<DataAccess.Response.ResponseModule>()
                .RegisterModule(new DataAccess.Database.DatabaseModule(botConfig));

            // Override repository registration with test repository.
            containerBuilder
                .Register(_ => DatabaseFixture.CreateTestRepository())
                .As<IRepository>()
                .SingleInstance();

            return containerBuilder;
        }

        private BotConfiguration BuildBotConfiguration()
        {
            return new BotConfiguration.Builder()
                .SetSelectedDatabaseAdapter("in_memory")
                .Build();
        }

        private IMofichanBackend ConstructMockBackend()
        {
            var mock = new Mock<IMofichanBackend>();
            mock.Setup(it => it.Subscribe(It.IsAny<IObserver<MessageContext>>()))
                .Callback<IObserver<MessageContext>>(observer => this.backendObserver = observer);

            mock.Setup(it => it.OnNext(It.IsAny<MessageContext>()))
                .Callback<MessageContext>(message => this.OnMessageSent(message));

            return mock.Object;
        }
        #endregion

        protected static int ResponseWindow
        {
            get
            {
                return 4;
            }
        }

        protected IContainer Container { get; }
        protected IList<MessageContext> SentMessages { get; }
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

        private void OnMessageSent(MessageContext message)
        {
            this.MessageSent?.Invoke(this, new MessageSentEventArgs(message));
        }

        public class MessageSentEventArgs : EventArgs
        {
            public MessageSentEventArgs(MessageContext message)
            {
                this.Message = message;
            }

            public MessageContext Message { get; }
        }

        protected virtual void TearDown()
        {
            this.Mofichan?.Dispose();
            this.lifetimeScope?.Dispose();

            this.Behaviours.Clear();
            this.SentMessages.Clear();

            this.lifetimeScope = this.Container.BeginLifetimeScope();
            this.pulseDriver = this.lifetimeScope.Resolve<ControllablePulseDriver>();
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
                this.pulseDriver,
                this.lifetimeScope.Resolve<IMessageClassifier>,
                this.lifetimeScope.Resolve<IBehaviourVisitorFactory>(),
                this.lifetimeScope.Resolve<IResponseSelector>(),
                this.lifetimeScope.Resolve<ILogger>());

            this.Mofichan.Start();
        }
        #endregion

        #region When
        protected void When_Mofichan_receives_a_message(IUser sender, string message)
        {
            var incomingMessage = new MessageContext(from: sender, to: this.MofichanUser, body: message);
            this.backendObserver.OnNext(incomingMessage);
        }

        protected void When_Mofichan_receives_a_message_with_tags(IUser sender, string message, params string[] tags)
        {
            var incomingMessage = new MessageContext(from: sender, to: this.MofichanUser, body: message, tags: tags);
            this.backendObserver.OnNext(incomingMessage);
        }

        protected void When_behaviours_are_driven_by__pulseCount__pulses(int pulseCount)
        {
            for (var i = 0; i < pulseCount; i++)
            {
                this.pulseDriver.Pulse();
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
            this.SentMessages.Select(it => it.Body).ShouldContain(body);
        }

        protected void Then_Mofichan_should_have_sent_response_with_pattern(
            string pattern, RegexOptions options)
        {
            Assert.True(this.SentMessages
                .Select(it => it.Body)
                .Any(it => Regex.Match(it, pattern, options).Success));
        }

        protected void Then_Mofichan_should_have_sent_response_containing__substring__(string substring)
        {
            Assert.True(this.SentMessages
                .Select(it => it.Body)
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

    internal static class DatabaseFixture
    {
        public static void Initialise()
        {
            var botConfig = new BotConfiguration.Builder()
                .SetSelectedDatabaseAdapter("mongodb")
                .WithDatabaseAdapterSetting("user", "testrunner")
                .WithDatabaseAdapterSetting("password", "testrunner")
                .WithDatabaseAdapterSetting("hostname", "ds141428.mlab.com")
                .WithDatabaseAdapterSetting("port", "41428")
                .Build();

            var logger = new LoggerConfiguration().CreateLogger();
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterInstance(logger).As<ILogger>();
            containerBuilder.RegisterModule(new DataAccess.Database.DatabaseModule(botConfig));

            var container = containerBuilder.Build();
            var productionRepo = container.Resolve<IRepository>();
            BaseTestRepository = new DataAccess.Database.InMemoryRepository(logger);

            productionRepo.All<AnalysisArticle>().ToList().ForEach(it => BaseTestRepository.Add(it));
            productionRepo.All<ResponseArticle>().ToList().ForEach(it => BaseTestRepository.Add(it));
        }

        private static IRepository BaseTestRepository { get; set; }

        public static IRepository CreateTestRepository()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            var newRepository = new DataAccess.Database.InMemoryRepository(logger);

            BaseTestRepository.All<AnalysisArticle>().ToList().ForEach(it => newRepository.Add(it));
            BaseTestRepository.All<ResponseArticle>().ToList().ForEach(it => newRepository.Add(it));

            return newRepository;
        }
    }
}