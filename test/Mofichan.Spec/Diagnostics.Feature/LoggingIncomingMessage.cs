using System;
using System.IO;
using System.Text.RegularExpressions;
using Autofac;
using Mofichan.Core.Interfaces;
using Moq;
using Serilog;
using Shouldly;
using TestStack.BDDfy;

namespace Mofichan.Spec.Diagnostics.Feature
{
    public class LoggingIncomingMessage : BaseScenario
    {
        private TextWriter generatedLogs;

        public LoggingIncomingMessage() : base("An incoming message handled by a behaviour is logged")
        {
            var mockBehaviour = new Mock<IMofichanBehaviour>();
            mockBehaviour.SetupGet(it => it.Id).Returns("mock");

            this.Given(s => s.Given_Mofichan_is_configured_with_behaviour("diagnostics"))
                .Given(s => s.Given_Mofichan_is_configured_with_behaviour(mockBehaviour.Object),
                        "Given Mofichan is configured with a mock behaviour")
                    .And(s => s.Given_Mofichan_is_running())
                .When(s => s.When_Mofichan_receives_a_message(new MockUser(), "foo"))
                .Then(s => s.Then_a_log_should_have_been_created_matching_pattern(
                        "behaviour \"mock\" offered incoming message \"foo\" from \"Joe Somebody\""));
        }

        protected override ContainerBuilder CreateContainerBuilder()
        {
            var builder = base.CreateContainerBuilder();

            this.generatedLogs = new StringWriter();
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TextWriter(this.generatedLogs)
                .CreateLogger();

            builder.RegisterInstance(logger).As<ILogger>();

            return builder;
        }

        private void Then_a_log_should_have_been_created_matching_pattern(string pattern)
        {
            var clumpedLogs = this.generatedLogs.ToString();
            string[] logs = clumpedLogs.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            logs.ShouldContain(it => Regex.IsMatch(it, pattern, RegexOptions.IgnoreCase));
        }

        private class MockUser : IUser
        {
            public string Name
            {
                get
                {
                    return "Joe Somebody";
                }
            }

            public string UserId
            {
                get
                {
                    return "Joe Somebody";
                }
            }

            public UserType Type
            {
                get
                {
                    return UserType.NormalUser;
                }
            }

            public void ReceiveMessage(string message)
            {
                throw new NotImplementedException();
            }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}
