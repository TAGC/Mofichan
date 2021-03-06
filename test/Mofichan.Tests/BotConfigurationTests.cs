﻿using System;
using System.Collections.Generic;
using Mofichan.Core;
using Shouldly;
using Xunit;

namespace Mofichan.Tests
{
    public class BotConfigurationTests
    {
        public static IEnumerable<object> BotConfigurationBuildExamples
        {
            get
            {
                yield return new object[]
                {
                    new BotConfiguration.Builder(),
                    new Predicate<BotConfiguration>(it =>
                        it.BackendConfiguration.Count == 0 &&
                        it.DatabaseAdapterConfiguration.Count == 0)
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

                yield return new object[]
                {
                    new BotConfiguration.Builder()
                        .SetSelectedDatabaseAdapter("mockDb")
                        .WithDatabaseAdapterSetting("url", "http://foo.bar.com")
                        .WithDatabaseAdapterSetting("maxConnections", "3"),

                    new Predicate<BotConfiguration>(it =>
                        it.SelectedDatabaseAdapter == "mockDb" &&
                        it.DatabaseAdapterConfiguration.Count == 2 &&
                        it.DatabaseAdapterConfiguration["url"] == "http://foo.bar.com" &&
                        it.DatabaseAdapterConfiguration["maxConnections"] == "3")
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
