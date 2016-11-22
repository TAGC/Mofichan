using System;
using System.IO;
using System.Text.RegularExpressions;
using Autofac;
using Mofichan.Core.Interfaces;
using Serilog;
using Shouldly;

namespace Mofichan.Spec.Diagnostics.Feature
{
    public abstract class BaseScenario : Scenario
    {
        protected BaseScenario(string scenarioTitle = null) : base(scenarioTitle)
        {
        }

        protected TextWriter GeneratedLogs { get; private set; }

        protected override ContainerBuilder CreateContainerBuilder()
        {
            var builder = base.CreateContainerBuilder();

            this.GeneratedLogs = new StringWriter();
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TextWriter(this.GeneratedLogs)
                .CreateLogger();

            builder.RegisterInstance(logger).As<ILogger>();

            return builder;
        }

        protected void Then_a_log_should_have_been_created_matching_pattern(string pattern)
        {
            var clumpedLogs = this.GeneratedLogs.ToString();
            string[] logs = clumpedLogs.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            logs.ShouldContain(it => Regex.IsMatch(it, pattern, RegexOptions.IgnoreCase));
        }

        protected class MockUser : IUser
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
