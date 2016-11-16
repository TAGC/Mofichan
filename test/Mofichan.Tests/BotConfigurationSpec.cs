using System;
using System.Collections.Generic;
using Mofichan.Core;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BotConfigurationSpec
    {
        public static IEnumerable<object> BotConfigurationBuildExamples
        {
            get
            {
                yield return new object[]
                {
                    new BotConfiguration.Builder(),
                    new Predicate<BotConfiguration>(it => it.BackendConfiguration.Count == 0)
                };

                yield return new object[]
                {
                    new BotConfiguration.Builder().SetBotName("foo"),
                    new Predicate<BotConfiguration>(it => it.BotName == "foo")
                };

                yield return new object[]
                {
                    new BotConfiguration.Builder().SetDeveloperName("bar"),
                    new Predicate<BotConfiguration>(it => it.DeveloperName == "bar")
                };

                yield return new object[]
                {
                    new BotConfiguration.Builder().SetSelectedBackend("baz"),
                    new Predicate<BotConfiguration>(it => it.SelectedBackend == "baz")
                };

                yield return new object[]
                {
                    new BotConfiguration.Builder()
                        .SetSelectedBackend("baz")
                        .WithBackendSetting("a", "1")
                        .WithBackendSetting("abra", "kadabra"),

                    new Predicate<BotConfiguration>(it =>
                        it.SelectedBackend == "baz" &&
                        it.BackendConfiguration.Count == 2 &&
                        it.BackendConfiguration["a"] == "1" &&
                        it.BackendConfiguration["abra"] == "kadabra")
                };
            }
        }

        [Theory]
        [MemberData(nameof(BotConfigurationBuildExamples))]
        public void Bot_Configuration_Builder_Should_Return_Expected_Configuration(
            BotConfiguration.Builder builder, Predicate<BotConfiguration> configValidator)
        {
            // WHEN we build the configuration using the provided builder.
            var actualConfiguration = builder.Build();

            // THEN the expected configuration should have been created.
            configValidator(actualConfiguration).ShouldBeTrue();
        }
    }
}
